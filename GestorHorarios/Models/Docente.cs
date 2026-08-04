using System.Collections.Generic;

namespace GestorHorarios.Models
{
    public class Docente
    {
        public int IdDocente { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string TipoTiempo { get; set; } = "";
        public string CarreraPrincipal { get; set; } = "";
        public string CarreraSecundaria { get; set; } = "Ninguna";
        public string Materias { get; set; } = "";
        public string HorarioLaboral { get; set; } = "";
        public int HorasMaximas { get; set; }
        public int HorasAsignadas { get; set; }
        public int IdCarreraPrincipal { get; set; }
        public List<int> MateriasIds { get; set; } = new();
        public List<int> CarrerasSecundariasIds { get; set; } = new();
        public string Nombre { get; internal set; }

        // Propiedad clave para las restricciones de horario del motor matemático
        public HashSet<string> DisponibilidadBloques { get; set; } = new();
    }

    public class Carrera
    {
        public int IdCarrera { get; set; }
        public string Nombre { get; set; } = "";
    }
}