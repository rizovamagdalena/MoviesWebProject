namespace MoviesWEB.Models
{
    public class CartItem
    {
        public int ScreeningId { get; set; }
        public string MovieName { get; set; }
        public DateTime ScreeningTime { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerTicket { get; set; }
        public List<int> SeatIds { get; set; } = new List<int>();
        public List<string> SeatNumbers { get; set; } = new List<string>();


    }
}
