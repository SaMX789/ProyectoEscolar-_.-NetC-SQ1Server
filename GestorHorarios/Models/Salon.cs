namespace GestorHorarios.Models
{
    public class Salon
    {
        public int IdSalon { get; set; }
        public string Nombre { get; set; } = "";
        public int Capacidad { get; set; }
        public string NombreEdificio { get; set; } = "";
        public string NombreCarrera { get; set; } = "";
        public int? IdCarrera { get; set; }
        public int? IdEdificio { get; set; }
    }
}
