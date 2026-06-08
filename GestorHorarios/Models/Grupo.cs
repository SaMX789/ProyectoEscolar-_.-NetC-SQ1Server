namespace GestorHorarios.Models
{
    public class Grupo
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = "";
        public int Semestre { get; set; }
        public string Turno { get; set; } = "";
        public int IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = "";

        // Ciclo académico derivado del semestre
        // Ciclo A (Ene-Jun): semestres pares 2,4,6,8
        // Ciclo B (Ago-Dic): semestres impares 1,3,5,7,9
        public string Ciclo => Semestre % 2 == 0 ? "A (Enero-Junio)" : "B (Agosto-Diciembre)";
    }
}
