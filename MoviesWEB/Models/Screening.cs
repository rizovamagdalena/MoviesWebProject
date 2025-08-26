using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MoviesWEB.Models
{
    public class Screening
    {
        public int Id { get; set; }
        public int Movie_Id { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Total_Tickets { get; set; }
        public int Available_Tickets { get; set; }
        public int Hall_Id { get; set; }
    }

    public class CreateScreening
    {
        public int Movie_Id { get; set; }        
        public int Hall_Id { get; set; }      
        public DateTime Screening_Date_Time { get; set; }

    }
    public class ShowScreening
    {
        public int Id { get; set; }
        public int Movie_Id { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Total_Tickets { get; set; }
        public int Available_Tickets { get; set; }
        public MovieSummary Movie { get; set; }
        public int Hall_Id { get; set; }
        public List<HallSeat> HallSeats { get; set; } = new List<HallSeat>();
        public decimal ticketPrice { get; set; }


    }



    public class HallSeat
    {
        [JsonProperty("seatId")]
        public int Id { get; set; }            
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }  
    }

    public class MovieSummary
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Poster_Path { get; set; }
        public decimal Amount { get; set; }
    }

    public class BookSeatsRequest
    {
        [JsonPropertyName("screeningId")]
        public int ScreeningId { get; set; }

        [JsonPropertyName("seatIds")]
        public List<int> SeatIds { get; set; } = new List<int>();
}
}
