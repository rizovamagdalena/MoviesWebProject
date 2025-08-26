namespace MoviesWEB.Models
{
    public class Ticket
    {
        public long Id { get; set; }
        public string MovieName { get; set; }
        public string PosterPath { get; set; }

        public string UserName { get; set; }
        public DateTime Watch_Movie { get; set; }
        public decimal Price { get; set; }
        public string HallName { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class CreateTicket
    {
        public long Movie_Id { get; set; }
        public long User_Id { get; set; }
        public DateTime Watch_Movie { get; set; }
        public decimal Price { get; set; }
    }
   


    public class ViewTicket
    {
        public long Id { get; set; }
        public string MovieName { get; set; }
        public string Username { get; set; }
        public string PosterPath { get; set; }
        public DateTime WatchDate { get; set; }
        public string HallName { get; set; }
        public string Seat { get; set; } 
        public decimal Price { get; set; }
    }


    public class AddToCartRequest
    {
        public int ScreeningId { get; set; }
        public int Quantity { get; set; }
        public List<int> SeatIds { get; set; } = new List<int>();
        public List<string> SeatNumbers { get; set; } = new List<string>();
    }

}
