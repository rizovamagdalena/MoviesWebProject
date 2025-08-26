namespace MoviesWEB.Models
{
    public class Movie
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public DateTime Release_Date { get; set; }
        public decimal Amount { get; set; }
        public string Poster_Path { get; set; }
        public string Plot { get; set; }
        public string Actors { get; set; }
        public string Directors { get; set; }
        public string Genres { get; set; }
        public double Rating { get; set; }
        public int UserRating { get; set; } 

    }

    public class CreateAndUpdateMovie
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public DateTime Release_Date { get; set; }
        public decimal Amount { get; set; }
        public string Poster_Path { get; set; }
        public string Plot { get; set; }
        public string Actors { get; set; }
        public string Directors { get; set; }
        public List<string> Genres { get; set; } = new List<string>();

    }



    public class MovieDetailsVM
    {
        public Movie Movie { get; set; }
        public List<Screening> Screenings { get; set; } = new();
    }

    public class FutureMovie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genres { get; set; }
        public string Poster_Path { get; set; }
    }

    public class CreateFutureMovie
    {
        public string Name { get; set; }
        public string Genres { get; set; }
        public string Poster_Path { get; set; }
    }

}
