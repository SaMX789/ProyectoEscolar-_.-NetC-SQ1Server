namespace GestorHorarios.Models
{
    public class Proyecto
    {
        public int IdProyecto { get; set; }
        public string Nombre { get; set; } = "";
        public int Anio { get; set; }
        public string Periodo { get; set; } = "";
        public string Ciclo { get; set; } = "";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
