using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Implementation;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace MoviesWEB.Controllers
{
    public class HomeController : Controller
    {

        private readonly IScreeningService _screeningService;
        private readonly IMovieService _movieService;


        public HomeController(IScreeningService screeningService,IMovieService movieService)
        {
            _screeningService = screeningService;
            _movieService = movieService;
        }
        public async Task<IActionResult> Index()
        {
            var screenings = await _screeningService.GetScreeningsAsync();
            var moviesInScreenings = await _movieService.GetMoviesAsync();

            var fullScreenings = screenings.Select(s => new ShowScreening
            {
                Id = s.Id,
                Movie_Id = s.Movie_Id,
                Screening_Date_Time = s.Screening_Date_Time,
                Total_Tickets = s.Total_Tickets,
                Available_Tickets = s.Available_Tickets,
                Movie = moviesInScreenings
                    .Where(m => m.Id == s.Movie_Id)
                    .Select(m => new MovieSummary
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Poster_Path = m.Poster_Path
                    })
                    .FirstOrDefault()
            }).ToList();

            var futureMovies = await _movieService.GetFutureMoviesAsync();
            var top3Movies = await _movieService.GetTopNMoviesAsync(3);

            ViewData["FutureMovies"] = futureMovies;
            ViewData["TopMovies"] = top3Movies;


            return View(fullScreenings);
        }


    }
}
