using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Security.Claims;

namespace MoviesWEB.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _apiBaseUrl;
        private readonly IScreeningService _screeningService;
        private readonly IMovieService _movieService;



        public TicketsController(ILogger<HomeController> logger, IOptions<DbSettings> settings, IScreeningService screeningService, IMovieService movieService)
        {
            _logger = logger;
            _apiBaseUrl = settings.Value.DbApi ?? throw new ArgumentNullException(nameof(settings.Value.DbApi));
            _screeningService = screeningService;
            _movieService = movieService;

        }


        protected async Task<List<ViewTicket>> GetTicketsAsync()
        {
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_apiBaseUrl + "/api/tickets");

            if (!response.IsSuccessStatusCode)
                return new List<ViewTicket>();
            System.Diagnostics.Debug.WriteLine("response" + response);

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("json" + json);

            var tickets = JsonConvert.DeserializeObject<List<Ticket>>(json);
            System.Diagnostics.Debug.WriteLine("json" + tickets);

            foreach (var t in tickets)
            {
                System.Diagnostics.Debug.WriteLine("Ticket " + t);

                System.Diagnostics.Debug.WriteLine("Ticket row  " + t.Row);
                System.Diagnostics.Debug.WriteLine("Ticket col " + t.Column);


            }

            var viewTickets = tickets.Select(t => new ViewTicket
            {
                Id = t.Id,
                MovieName = t.MovieName,
                Username=t.UserName,
                PosterPath = t.PosterPath ?? string.Empty,
                WatchDate = t.Watch_Movie,
                HallName = t.HallName ?? "Unknown Hall",
                Seat = $"Row {t.Row} - Seat {t.Column}",
                Price = t.Price
            }).ToList();

            return viewTickets;
        }

        protected async Task<List<ViewTicket>> GetTicketsForUserAsync(string username)
        {
            var tickets = await GetTicketsAsync();
            System.Diagnostics.Debug.WriteLine("Tickets got " + tickets.Count);

            foreach(var t in tickets)
            {
                System.Diagnostics.Debug.WriteLine("Tickets: " + t.Username + " " + t.Id);

            }

            var userTickets = tickets.Where(t => t.Username == username).ToList();

            return userTickets;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSeats([FromBody] BookSeatsRequest request)
        {
            var userNameClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userNameClaim == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            System.Diagnostics.Debug.WriteLine($"In Bookseats Ok e, reqestot e : {request.SeatIds}, {request.SeatIds.Count}");

            if (request.SeatIds == null || request.SeatIds.Count == 0)
                return Json(new { success = false, message = "No seats selected." });

            var username = userNameClaim.Value;

            try
            {
                var result = await _screeningService.BookSeatsAsync(request.ScreeningId, username, request.SeatIds);
                System.Diagnostics.Debug.WriteLine($"Result: {result}");

                if (result)
                {
                    TempData["SuccessMessage"] = "Thank you for your purchase! Your tickets have been booked.";
                    return Json(new { success = true });
                }
                else
                {
                    TempData["SuccessMessage"] = "Some Seats are alrady booked.";
                    return Json(new { success = false, message = "Some seats are already booked." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("ShoppingCart");
            return Ok(new { success = true });
        }

        // Controller action
        [Authorize(Roles="Administrator")]
        public async Task<IActionResult> AllTickets()
        {
            var viewTickets = await GetTicketsAsync(); 
            return View(viewTickets);
        }

        // Controller action
        public async Task<IActionResult> MyTickets()
        {
            var usernameClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usernameClaim == null)
                return Unauthorized();

            string currentUser = usernameClaim.Value;

            var viewTickets = await GetTicketsForUserAsync(currentUser); 
            return View(viewTickets);
        }

    

    }
}
