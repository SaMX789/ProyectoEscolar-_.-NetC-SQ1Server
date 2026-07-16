namespace GestorHorarios.Models
{
    class HorarioAsignado
    {

        public int DiaSemana { get; set; } // 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes
        public int BloqueHora { get; set; } // 0, 1, 3, 4, 5, 6 (saltando el 2 que es receso)
        public string NombreMateria { get; set; } = "";
        public string NombreDocente { get; set; } = "";

    }
}
