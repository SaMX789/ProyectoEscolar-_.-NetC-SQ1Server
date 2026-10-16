using System;
using System.Collections.Generic;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Xceed.Drawing;
using GestorHorarios.Models;

namespace GestorHorarios.Services
{
    public class GeneradorWordService
    {
        public class GrupoData
        {
            public string NombreGrupo { get; set; } = "";
            public string Turno { get; set; } = "";
            public int TotalHoras { get; set; }
            public List<HorarioAsignado> Horarios { get; set; } = new List<HorarioAsignado>();
        }

        public class DocenteDataWord
        {
            public string NombreDocente { get; set; } = "";
            public int Hfg { get; set; }
            public List<HorarioPersonal> Horarios { get; set; } = new List<HorarioPersonal>();
        }

        // =====================================================================================
        // EXPORTAR HORARIOS DE CARRERAS (GRUPOS)
        // =====================================================================================
        public void ExportarHorarioGrupos(string rutaArchivo, string nombreCarrera, string ciclo, List<GrupoData> gruposData)
        {
            using (var document = DocX.Create(rutaArchivo))
            {
                document.MarginLeft = 30f;
                document.MarginRight = 30f;
                document.MarginTop = 40f;

                for (int i = 0; i < gruposData.Count; i++)
                {
                    var data = gruposData[i];
                    bool esMatutino = data.Turno?.ToLower().Contains("matutino") == true || data.Turno?.ToLower().StartsWith("m") == true;
                    var bloques = esMatutino ? BloqueHorarioInfo.Matutino : BloqueHorarioInfo.Vespertino;

                    var titulo = document.InsertParagraph($"Horarios — {nombreCarrera}");
                    titulo.FontSize(18).Color(Xceed.Drawing.Color.Parse(144, 25, 65)).Bold().Alignment = Alignment.center;

                    var subtitulo = document.InsertParagraph(ciclo);
                    subtitulo.FontSize(12).Color(Xceed.Drawing.Color.Gray).Alignment = Alignment.center;
                    subtitulo.SpacingAfter(20d);

                    var infoGrupo = document.InsertParagraph($"Grupo: {data.NombreGrupo}  |  Turno: {(esMatutino ? "Matutino" : "Vespertino")}  |  Horas a la semana: {data.TotalHoras}");
                    infoGrupo.FontSize(14).Color(Xceed.Drawing.Color.Parse(144, 25, 65)).Bold().Alignment = Alignment.left;
                    infoGrupo.SpacingAfter(10d);

                    Table table = document.AddTable(bloques.Count + 1, 6);
                    table.Alignment = Alignment.center;
                    table.Design = TableDesign.TableGrid;

                    table.SetColumnWidth(0, 70);
                    for (int c = 1; c < 6; c++) table.SetColumnWidth(c, 95);

                    string[] encabezados = { "Hora", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
                    for (int c = 0; c < 6; c++)
                    {
                        var cell = table.Rows[0].Cells[c];
                        cell.FillColor = Xceed.Drawing.Color.Parse(144, 25, 65);
                        var p = cell.Paragraphs.First().Append(encabezados[c]).Color(Xceed.Drawing.Color.White).Bold().FontSize(11);
                        p.Alignment = Alignment.center;
                        cell.VerticalAlignment = VerticalAlignment.Center;
                        cell.MarginTop = 5;
                        cell.MarginBottom = 5;
                    }

                    for (int r = 0; r < bloques.Count; r++)
                    {
                        var bloque = bloques[r];
                        var row = table.Rows[r + 1];

                        row.Cells[0].FillColor = bloque.EsReceso ? Xceed.Drawing.Color.Parse(255, 236, 179) : Xceed.Drawing.Color.Parse(255, 243, 224);

                        var parHora = row.Cells[0].Paragraphs.First();
                        parHora.Alignment = Alignment.center;
                        var textoHora = parHora.Append(bloque.Horario).FontSize(9);

                        if (bloque.EsReceso)
                        {
                            textoHora.Bold().Color(Xceed.Drawing.Color.Parse(230, 81, 0));
                        }
                        row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

                        if (bloque.EsReceso)
                        {
                            row.MergeCells(1, 5);
                            row.Cells[1].FillColor = Xceed.Drawing.Color.Parse(255, 236, 179);

                            var parReceso = row.Cells[1].Paragraphs.First();
                            parReceso.Alignment = Alignment.center;
                            parReceso.Append("R E C E S O").Color(Xceed.Drawing.Color.Parse(230, 81, 0)).Bold().FontSize(11);

                            row.Cells[1].VerticalAlignment = VerticalAlignment.Center;
                        }
                        else
                        {
                            for (int c = 1; c <= 5; c++)
                            {
                                var cell = row.Cells[c];
                                cell.VerticalAlignment = VerticalAlignment.Center;
                                cell.MarginTop = 4;
                                cell.MarginBottom = 4;

                                var clase = data.Horarios.FirstOrDefault(h => h.DiaSemana == c && h.BloqueHora == bloque.IdBloque);
                                if (clase != null)
                                {
                                    var paragraph = cell.Paragraphs.First();
                                    paragraph.Alignment = Alignment.center;

                                    paragraph.Append(clase.NombreMateria).FontSize(8.5).Bold().Color(Xceed.Drawing.Color.Parse(51, 51, 51));

                                    if (!string.IsNullOrEmpty(clase.NombreDocente) && clase.NombreDocente != "0")
                                    {
                                        paragraph.AppendLine($"({clase.NombreDocente})").FontSize(7.5).Color(Xceed.Drawing.Color.Gray).Italic();
                                    }

                                    string textoSalon = string.IsNullOrEmpty(clase.NombreSalon) ? "[Sin Salón]" : $"[{clase.NombreSalon} - {clase.NombreEdificio}]";
                                    var pSalon = paragraph.AppendLine(textoSalon).FontSize(8.5).Bold();

                                    if (string.IsNullOrEmpty(clase.NombreSalon))
                                        pSalon.Color(Xceed.Drawing.Color.Firebrick);
                                    else
                                        pSalon.Color(Xceed.Drawing.Color.ForestGreen);
                                }
                            }
                        }
                    }

                    document.InsertTable(table);

                    if (i < gruposData.Count - 1)
                    {
                        document.InsertParagraph("").InsertPageBreakAfterSelf();
                    }
                }

                document.Save();
            }
        }

        // =====================================================================================
        // EXPORTAR HORARIOS INDIVIDUALES DE DOCENTES
        // =====================================================================================
        public void ExportarHorariosDocentes(string rutaArchivo, string tituloCiclo, List<DocenteDataWord> docentesData)
        {
            using (var document = DocX.Create(rutaArchivo))
            {
                document.MarginLeft = 30f;
                document.MarginRight = 30f;
                document.MarginTop = 40f;

                for (int i = 0; i < docentesData.Count; i++)
                {
                    var data = docentesData[i];

                    // Títulos en color Azul
                    var titulo = document.InsertParagraph("Horario del Personal Docente");
                    titulo.FontSize(18).Color(Xceed.Drawing.Color.Parse(21, 101, 192)).Bold().Alignment = Alignment.center;

                    var subtitulo = document.InsertParagraph(tituloCiclo);
                    subtitulo.FontSize(12).Color(Xceed.Drawing.Color.Gray).Alignment = Alignment.center;
                    subtitulo.SpacingAfter(20d);

                    var infoGrupo = document.InsertParagraph($"Docente: {data.NombreDocente}  |  Horas Asignadas: {data.Horarios.Count}  |  HFG Oficiales: {data.Hfg}");
                    infoGrupo.FontSize(14).Color(Xceed.Drawing.Color.Parse(21, 101, 192)).Bold().Alignment = Alignment.left;
                    infoGrupo.SpacingAfter(10d);

                    Table table = document.AddTable(BloqueHorarioInfo.Unificados.Count + 1, 6);
                    table.Alignment = Alignment.center;
                    table.Design = TableDesign.TableGrid;

                    table.SetColumnWidth(0, 80);
                    for (int c = 1; c < 6; c++) table.SetColumnWidth(c, 110);

                    string[] encabezados = { "Hora", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
                    for (int c = 0; c < 6; c++)
                    {
                        var cell = table.Rows[0].Cells[c];
                        cell.FillColor = Xceed.Drawing.Color.Parse(51, 51, 51); // Gris muy oscuro
                        var p = cell.Paragraphs.First().Append(encabezados[c]).Color(Xceed.Drawing.Color.White).Bold().FontSize(11);
                        p.Alignment = Alignment.center;
                        cell.VerticalAlignment = VerticalAlignment.Center;
                        cell.MarginTop = 5;
                        cell.MarginBottom = 5;
                    }

                    for (int r = 0; r < BloqueHorarioInfo.Unificados.Count; r++)
                    {
                        var bloque = BloqueHorarioInfo.Unificados[r];
                        var row = table.Rows[r + 1];

                        row.Cells[0].FillColor = Xceed.Drawing.Color.White;

                        var parHora = row.Cells[0].Paragraphs.First();
                        parHora.Alignment = Alignment.center;
                        var textoHora = parHora.Append(bloque.Horario).FontSize(9);

                        if (bloque.EsReceso)
                        {
                            textoHora.Bold().Color(Xceed.Drawing.Color.Parse(158, 158, 158));
                        }
                        row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

                        if (bloque.EsReceso)
                        {
                            row.MergeCells(1, 5);
                            row.Cells[1].FillColor = Xceed.Drawing.Color.Parse(245, 245, 245);

                            var parReceso = row.Cells[1].Paragraphs.First();
                            parReceso.Alignment = Alignment.center;
                            parReceso.Append("R E C E S O").Color(Xceed.Drawing.Color.Parse(158, 158, 158)).Bold().FontSize(11);
                            row.Cells[1].VerticalAlignment = VerticalAlignment.Center;
                        }
                        else
                        {
                            for (int c = 1; c <= 5; c++)
                            {
                                var cell = row.Cells[c];
                                cell.VerticalAlignment = VerticalAlignment.Center;
                                cell.MarginTop = 4;
                                cell.MarginBottom = 4;

                                var clase = data.Horarios.FirstOrDefault(h => h.Dia == c && h.Bloque == bloque.IdBloque);
                                if (clase != null)
                                {
                                    // Pintamos de azul muy clarito la celda si tiene clase
                                    cell.FillColor = Xceed.Drawing.Color.Parse(227, 242, 253);

                                    var paragraph = cell.Paragraphs.First();
                                    paragraph.Alignment = Alignment.center;

                                    paragraph.Append(clase.Materia).FontSize(8.5).Bold().Color(Xceed.Drawing.Color.Parse(51, 51, 51));

                                    if (!string.IsNullOrEmpty(clase.Grupo))
                                    {
                                        paragraph.AppendLine($"Grupo: {clase.Grupo}").FontSize(8.5).Color(Xceed.Drawing.Color.Parse(21, 101, 192));
                                    }
                                }
                            }
                        }
                    }

                    document.InsertTable(table);

                    if (i < docentesData.Count - 1)
                    {
                        document.InsertParagraph("").InsertPageBreakAfterSelf();
                    }
                }

                document.Save();
            }
        }
    }
}