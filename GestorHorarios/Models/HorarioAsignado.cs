using System.Collections.Generic;

namespace GestorHorarios.Models
{
    public class HorarioAsignado
    {
        public int DiaSemana { get; set; } // 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes
        public int BloqueHora { get; set; } // 1..12 (0 = Receso en UI)
        public string NombreMateria { get; set; } = "";
        public string NombreDocente { get; set; } = "";
        public int IdDocente { get; set; }
        public string NombreSalon { get; set; } = "";
        public string NombreEdificio { get; set; } = "";
    }

    public class HorarioPersonal
    {
        public int Dia { get; set; }
        public int Bloque { get; set; }
        public string Materia { get; set; } = "";
        public string Grupo { get; set; } = "";
    }

    public class BloqueHorarioInfo
    {
        public string Horario { get; set; } = "";
        public int IdBloque { get; set; }
        public bool EsReceso { get; set; }

        public static IReadOnlyList<BloqueHorarioInfo> Matutino { get; } = new List<BloqueHorarioInfo>
        {
            new() { Horario = "7:30 - 8:30", IdBloque = 1 },
            new() { Horario = "8:30 - 9:20", IdBloque = 2 },
            new() { Horario = "9:20 - 9:40", IdBloque = 0, EsReceso = true },
            new() { Horario = "9:40 - 10:30", IdBloque = 3 },
            new() { Horario = "10:30 - 11:30", IdBloque = 4 },
            new() { Horario = "11:30 - 12:30", IdBloque = 5 },
            new() { Horario = "12:30 - 13:30", IdBloque = 6 },
            new() { Horario = "13:30 - 14:30", IdBloque = 7 }
        };

        public static IReadOnlyList<BloqueHorarioInfo> Vespertino { get; } = new List<BloqueHorarioInfo>
        {
            new() { Horario = "12:30 - 13:30", IdBloque = 6 },
            new() { Horario = "13:30 - 14:30", IdBloque = 7 },
            new() { Horario = "14:30 - 15:20", IdBloque = 8 },
            new() { Horario = "15:20 - 15:40", IdBloque = 0, EsReceso = true },
            new() { Horario = "15:40 - 16:30", IdBloque = 9 },
            new() { Horario = "16:30 - 17:30", IdBloque = 10 },
            new() { Horario = "17:30 - 18:30", IdBloque = 11 },
            new() { Horario = "18:30 - 19:30", IdBloque = 12 }
        };

        public static IReadOnlyList<BloqueHorarioInfo> Unificados { get; } = new List<BloqueHorarioInfo>
        {
            new() { Horario = "7:30 - 8:30", IdBloque = 1 },
            new() { Horario = "8:30 - 9:20", IdBloque = 2 },
            new() { Horario = "9:20 - 9:40", IdBloque = 0, EsReceso = true },
            new() { Horario = "9:40 - 10:30", IdBloque = 3 },
            new() { Horario = "10:30 - 11:30", IdBloque = 4 },
            new() { Horario = "11:30 - 12:30", IdBloque = 5 },
            new() { Horario = "12:30 - 13:30", IdBloque = 6 },
            new() { Horario = "13:30 - 14:30", IdBloque = 7 },
            new() { Horario = "14:30 - 15:20", IdBloque = 8 },
            new() { Horario = "15:20 - 15:40", IdBloque = 0, EsReceso = true },
            new() { Horario = "15:40 - 16:30", IdBloque = 9 },
            new() { Horario = "16:30 - 17:30", IdBloque = 10 },
            new() { Horario = "17:30 - 18:30", IdBloque = 11 },
            new() { Horario = "18:30 - 19:30", IdBloque = 12 }
        };
    }
}
