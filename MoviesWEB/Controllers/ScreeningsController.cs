using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MoviesWEB.Extensions;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Implementation;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;



namespace MoviesWEB.Controllers
{
    [Authorize]
    public class ScreeningsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IScreeningService _screeningService;
        private readonly IMovieService _movieService;
       

        public ScreeningsController(ILogger<HomeController> logger ,IScreeningService screeningService,IMovieService movieService)
        {
            _logger = logger;
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
                    .FirstOrDefault(),
                Hall_Id = s.Hall_Id
            }).ToList();

            return View(fullScreenings);
        }

        // GET: ScreeningsController/Details/5
        public async Task<ActionResult> Details(long id)
        {
            var screening = await _screeningService.GetScreeningByIdAsync(id);
           
            if (screening == null)
            {
                return NotFound();
            }

            var movieInScreening = await _movieService.GetMovieByIdAsync(screening.Movie_Id);

            var allSeats = await _screeningService.GetAllSeatsAsync(screening.Hall_Id);

            var takenSeatIds = await _screeningService.GetTakenSeatsAsync(screening.Id);

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();
            var cartSeatIds = cart.SelectMany(c => c.SeatIds).ToList(); 

            var seats = allSeats.Select(s => new HallSeat
            {
                Id = s.Id,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                IsAvailable = !takenSeatIds.Contains(s.Id) && !cartSeatIds.Contains(s.Id)
            }).ToList();



            var fullScreening = new ShowScreening
            {
                Id = screening.Id,
                Movie_Id = screening.Movie_Id,
                Screening_Date_Time = screening.Screening_Date_Time,
                Total_Tickets = screening.Total_Tickets,
                Available_Tickets = screening.Available_Tickets,
                Movie = new MovieSummary
                {
                    Id = movieInScreening.Id,
                    Name = movieInScreening.Name,
                    Poster_Path = movieInScreening.Poster_Path
                },
                Hall_Id = screening.Hall_Id,
                HallSeats = seats,
                ticketPrice = movieInScreening.Amount
            };

            return View(fullScreening);
        }


        // GET: Screenings/Create
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var halls = await _screeningService.getAllHalls();

            var movies = await _movieService.GetMoviesAsync();


            ViewBag.Halls = new SelectList(halls, "Id", "Name");
            ViewBag.Movies = new SelectList(movies, "Id", "Name");
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(CreateScreening model,string Time)
        {
            System.Diagnostics.Debug.WriteLine($"In Create Post");

            if (!ModelState.IsValid)
            {
                ViewBag.Halls = new SelectList(await _screeningService.getAllHalls(), "Id", "Name");
                ViewBag.Movies = new SelectList(await _movieService.GetMoviesAsync(), "Id", "Name");
                return View(model);
            }

            if (!TimeSpan.TryParse(Time, out var timeSpan))
            {
                ModelState.AddModelError("Time", "Invalid time slot selected.");
                ViewBag.Halls = new SelectList(await _screeningService.getAllHalls(), "Id", "Name");
                ViewBag.Movies = new SelectList(await _movieService.GetMoviesAsync(), "Id", "Name");
                return View(model);
            }

            model.Screening_Date_Time = model.Screening_Date_Time.Date.Add(timeSpan);

            await _screeningService.CreateScreeningAsync(model);

            return RedirectToAction("Index");
        }


        // GET: ScreeningsController/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var screening = await _screeningService.GetScreeningByIdAsync(id);
            if (screening == null) return NotFound();

            var model = new Screening
            {
                Id = screening.Id,
                Movie_Id = screening.Movie_Id,
                Screening_Date_Time = screening.Screening_Date_Time,
                Total_Tickets = screening.Total_Tickets,
                Available_Tickets = screening.Available_Tickets,
         
            };


            return View(model);
        }

        // POST: S/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Screening updatedScreening)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedScreening);

            }

            bool updated = await _screeningService.UpdateScreeningAsync(id, updatedScreening);

            if (updated)
            {
                TempData["SuccessMessage"] = "Screening updated successfully.";
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                ModelState.AddModelError("", "Error updating screening.");
                return View(updatedScreening);
            }
        }


        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var screening = await _screeningService.GetScreeningByIdAsync(id);
            if (screening == null) return NotFound();

            await _screeningService.DeleteScreeningAsync(id);

            return RedirectToAction("Index", "Screenings"); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            int screeningId = request.ScreeningId;
            int quantity = request.Quantity;

            var screening = await _screeningService.GetScreeningByIdAsync(screeningId);

            if (screening == null)
            {
                return NotFound();
            }

            var movieInScreening = await _movieService.GetMovieByIdAsync(screening.Movie_Id);

            var fullScreening = new ShowScreening
            {
                Id = screening.Id,
                Movie_Id = screening.Movie_Id,
                Screening_Date_Time = screening.Screening_Date_Time,
                Total_Tickets = screening.Total_Tickets,
                Available_Tickets = screening.Available_Tickets,
                Movie = new MovieSummary
                {
                    Id = movieInScreening.Id,
                    Name = movieInScreening.Name,
                    Poster_Path = movieInScreening.Poster_Path,
                    Amount = movieInScreening.Amount
                }
            };
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c => c.ScreeningId == screeningId);
            var existingSeatIds = existingItem?.SeatIds ?? new List<int>();
            var newSeatIds = request.SeatIds.Except(existingSeatIds).ToList();


            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.SeatIds = existingItem.SeatIds
                                   .Union(request.SeatIds)
                                   .ToList();
            }
            else
            {

                cart.Add(new CartItem
                {
                    ScreeningId = screeningId,
                    MovieName = fullScreening.Movie.Name,
                    ScreeningTime = fullScreening.Screening_Date_Time,
                    Quantity = quantity,
                    PricePerTicket = fullScreening.Movie.Amount,
                    SeatIds = request.SeatIds,
                    SeatNumbers = request.SeatNumbers
                });
            }

            HttpContext.Session.SetObjectAsJson("ShoppingCart", cart);

            return Json(new { success = true, message = "Added to cart." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int id) 
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();
            var itemToRemove = cart.FirstOrDefault(c => c.ScreeningId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson("ShoppingCart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Item not found in cart." });
        }


        public IActionResult ShoppingCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int hallId, DateOnly date)
        {
            System.Diagnostics.Debug.WriteLine($"GetAvailableTimeSlots called ");

            var slots = await _screeningService.GetAvailableTimeSlotsAsync(hallId, date);

            System.Diagnostics.Debug.WriteLine($"After slots = await called {Json(slots)}");

            return Json(slots);
        }

    


    }
}
