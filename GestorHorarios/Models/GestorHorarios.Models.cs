namespace GestorHorarios.Models
{
    class GestorHorarios
    {
        public class Proyecto
        {
            public int IdProyecto { get; set; }
            public string Ciclo { get; set; } = "";
            public int Anio { get; set; }
            public string Periodo { get; set; } = "";
        }

        public class Grupo
        {
            public int IdGrupo { get; set; }
            public string Nombre { get; set; } = "";
            public int Semestre { get; set; }
            public string Turno { get; set; } = "";
            public string NombreCarrera { get; set; } = "";
        }

        public class Materia
        {
            public int IdMateria { get; set; }
            public string Nombre { get; set; } = "";
            public string Clave { get; set; } = "";
            public int Creditos { get; set; }
            public int Semestre { get; set; }
            public int IdDocenteAsignado { get; set; } // Opcional, si el docente ya está pre-asignado a la materia
        }

        public class Docente
        {
            public int IdDocente { get; set; }
            public string Nombre { get; set; } = "";
        }

        // Modelo crucial para la Vista: representa una celda del horario ya calculada
        public class HorarioAsignado
        {
            public int DiaSemana { get; set; } // 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes
            public int BloqueHora { get; set; } // 0, 1, 3, 4, 5, 6 (saltando el 2 que es receso)
            public string NombreMateria { get; set; } = "";
            public string NombreDocente { get; set; } = "";
        }
    }
}
