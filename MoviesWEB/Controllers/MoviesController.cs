using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoviesWEB.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly IMovieService _movieService;
        private readonly IScreeningService _screeningService;


        public MoviesController(ILogger<MoviesController> logger, IMovieService movieService,IScreeningService screeningService)
        {
            _logger = logger;
            _movieService = movieService;
            _screeningService = screeningService;
        }


        public async Task<IActionResult> Index(string genre)
        {
            IEnumerable<Movie> movies;
            var allGenres=await _movieService.GetAllGenres();

            if (string.IsNullOrEmpty(genre) || genre == "All")
            {
                movies = await _movieService.GetMoviesAsync();
            }
            else
            {
                movies = await _movieService.GetMoviesByGenreAsync(genre);
            }
            System.Diagnostics.Debug.WriteLine($"In  index controller:{movies}");

            ViewBag.SelectedGenre = genre ?? "All";
            ViewData["genres"] = allGenres;

            return View(movies);
        }


        // GET: MoviesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirst("Id")?.Value;
            long userId = 0;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                userId = long.Parse(userIdClaim);
            }

            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            movie.UserRating = await _movieService.GetUserRatingForMovie(movie.Id,userId);

            System.Diagnostics.Debug.WriteLine($"userratign:{movie.UserRating}  ");
            System.Diagnostics.Debug.WriteLine($"movie:{movie.Name}  ");

            var screenings = await _screeningService.GetScreeningsForMovieAsync(id);

            var vm = new MovieDetailsVM
            {
                Movie = movie,
                Screenings = screenings
            };


            return View(vm);
        }

        // GET: Movies/Create
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var genres = await _movieService.GetAllGenres();
            ViewBag.Genres = new MultiSelectList(genres);
            return View(); 
        }

        // POST: Movies/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAndUpdateMovie movie)
        {
            if (ModelState.IsValid)
            {
                await _movieService.CreateMovieAsync(movie); 
                return RedirectToAction(nameof(Index));   
            }

            var genres = await _movieService.GetAllGenres();
            ViewBag.Genres = new MultiSelectList(genres);
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddTicket(CreateTicket createRequest)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Movies");
            }

            bool success = await _movieService.AddTicketAsync(createRequest);

            if (success)
            {
                TempData["SuccessMessage"] = $"{createRequest.Price} ticket(s) added to cart.";
                return RedirectToAction("Details", "Movies", new { id = createRequest.Movie_Id });
            }
            else
            {
                ModelState.AddModelError("", "Error adding tickets.");
                return RedirectToAction("Index", "Movies");
            }
        }


        // GET: MoviesController/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null) return NotFound();

            var model = new CreateAndUpdateMovie
            {
                Id = movie.Id,
                Name = movie.Name,
                Duration = movie.Duration,
                Release_Date = movie.Release_Date,
                Amount = movie.Amount,
                Poster_Path = movie.Poster_Path,
                Plot = movie.Plot,
                Directors = movie.Directors,
                Actors = movie.Actors,
                Genres = movie.Genres.Split(',').Select(g => g.Trim()).ToList()
            };

            await SetGenresMultiSelect(model);

            return View(model);
        }

        private async Task SetGenresMultiSelect(CreateAndUpdateMovie model)
        {
            var allGenres = await _movieService.GetAllGenres();
            var selectedGenres = model.Genres ?? new List<string>();
            ViewBag.Genres = new MultiSelectList(
                allGenres.Select(g => new SelectListItem { Value = g, Text = g }),
                "Value",
                "Text",
                selectedGenres
            );
        }

        // POST: MoviesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, CreateAndUpdateMovie updatedMovie)
        {
            System.Diagnostics.Debug.WriteLine($"in  Edit Post  ");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("Invalid model state:");

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }


                await SetGenresMultiSelect(updatedMovie);
                return View(updatedMovie);
            }

            System.Diagnostics.Debug.WriteLine($"UpdateMovie BeforeX Calling  ");

            bool updated = await _movieService.UpdateMovieAsync(id, updatedMovie);
            System.Diagnostics.Debug.WriteLine($"UpdateMovie After Calling  {updated}");


            if (updated)
            {
                TempData["SuccessMessage"] = "Movie updated successfully.";
                System.Diagnostics.Debug.WriteLine($"Successs {updated}");


                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Fail {updated}");
                await SetGenresMultiSelect(updatedMovie);


                ModelState.AddModelError("", "Error updating movie.");
                return View(updatedMovie);
            }


        }
        

        // POST: MoviesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _movieService.DeleteMovieAsync(id);

            if (deleted)
            {
                TempData["SuccessMessage"] = "Movie deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                    return NotFound();

                ModelState.AddModelError("", "Error deleting movie.");
                return View("Edit", movie);
            }
        }

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<Movie>());
            }

            var movies = await _movieService.SearchMoviesAsync(query);

            //foreach (var movie in movies)
            //{

            //    System.Diagnostics.Debug.WriteLine($"Found Movie in Controller: Id={movie.Id}, Name={movie.Name}, Poster_Path={movie.Poster_Path}");

            //}

            ViewData["Query"] = query;

            var result = movies.Select(m => new
            {
                m.Id,
                m.Name,
                m.Poster_Path
            });

            var jsonResult = Json(result);
            System.Diagnostics.Debug.WriteLine("Returning JSON: " + JsonConvert.SerializeObject(result));
            return jsonResult;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> FutureMovies()
        {
            var movies = await _movieService.GetFutureMoviesAsync();
            return View(movies);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult CreateFutureMovies()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateFutureMovies(CreateFutureMovie movie)
        {
            if (!ModelState.IsValid) return View(movie);

            await _movieService.CreateFutureMovieAsync(movie);
            return RedirectToAction(nameof(FutureMovies));
        }



        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteFutureMovieConfirmed(int id)
        {
            await _movieService.DeleteFutureMovieAsync(id);
            return RedirectToAction(nameof(FutureMovies));
        }

        public class UpdateRatingRequest
        {
            public long MovieId { get; set; }
            public int Rating { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRating([FromBody] UpdateRatingRequest request)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateRating called for :{request.MovieId} and {request.Rating}");

            var userIdClaim = User.FindFirst("Id")?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var userId = long.Parse(userIdClaim);

            var (success, message) = await _movieService.UpdateUserRating(request.MovieId, userId, request.Rating);

            return Ok(new { success, message });
        }

    }
}
