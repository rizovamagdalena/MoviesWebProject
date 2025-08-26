namespace MoviesWEB.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Rows { get; set; }
        public int Seats_Per_Row { get; set; }
    }

    public class HallDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Rows { get; set; }
        public int Seats_Per_Row { get; set; }
        public List<HallSeatDto> Seats { get; set; } = new List<HallSeatDto>();
    }

    public class HallSeatDto
    {
        public int SeatId { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
    }

 

}
