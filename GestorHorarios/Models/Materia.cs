namespace GestorHorarios.Models
{
    public class Materia
    {
        public int IdMateria { get; set; }

        public string Nombre { get; set; } = "";

        public string Clave { get; set; } = "";

        public int Creditos { get; set; }

        public int Semestre { get; set; }
    }
}
