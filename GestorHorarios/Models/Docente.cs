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

        // Lista de ids de materias impartidas
        public List<int> MateriasIds { get; set; } = new();

        // Lista de ids de carreras secundarias
        public List<int> CarrerasSecundariasIds { get; set; } = new();
        public string Nombre { get; internal set; }
    }

    public class Carrera
    {
        public int IdCarrera { get; set; }

        public string Nombre { get; set; } = "";
    }
}
