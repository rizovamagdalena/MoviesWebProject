using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Reflection.Metadata.BlobBuilder;

namespace MoviesWEB.Service.Implementation
{
    public class ScreeningService : IScreeningService
    {

        private readonly string _apiBaseUrl;
        private readonly HttpClient _client;

        public ScreeningService(IOptions<DbSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _apiBaseUrl = settings.Value.DbApi ?? throw new ArgumentNullException(nameof(settings.Value.DbApi));
        }

        public async Task<List<Screening>> GetScreeningsAsync()
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl + "/api/screenings");

                if (!response.IsSuccessStatusCode)
                    return new List<Screening>();

                var json = await response.Content.ReadAsStringAsync();
                var screenings = JsonConvert.DeserializeObject<List<Screening>>(json);
                return screenings ?? new List<Screening>();
            }
        }

        public async Task<List<Screening>> GetScreeningsForMovieAsync(long id)
        {
            var allScreenings = await GetScreeningsAsync();

            var movieScreenings = allScreenings
                .Where(s => s.Movie_Id == id)        
                .OrderBy(s => s.Screening_Date_Time)
                .ToList();

            return movieScreenings;
        }


        public async Task<Screening> GetScreeningByIdAsync(long id)
        {

            var response = await _client.GetAsync($"{_apiBaseUrl}/api/screenings/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Screening>(json);
        }


        public async Task<bool> CreateScreeningAsync(CreateScreening createRequest)
        {
            System.Diagnostics.Debug.WriteLine($"In :CreateScreeningAsync");

            var jsonContent = JsonConvert.SerializeObject(createRequest);
            System.Diagnostics.Debug.WriteLine($"Json Content: {jsonContent}");

            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine($"Request Content: {requestContent}");


            var response = await _client.PostAsync($"{_apiBaseUrl}/api/screenings", requestContent);

            System.Diagnostics.Debug.WriteLine($"Response Content: {response}");

            return response.IsSuccessStatusCode;
        }


       public async Task<bool> UpdateScreeningAsync(int id, Screening updatedScreening)
        {
            var jsonContent = JsonConvert.SerializeObject(updatedScreening);
            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{_apiBaseUrl}/api/screenings/{id}", requestContent);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteScreeningAsync(int id)
        {
            var response = await _client.DeleteAsync($"{_apiBaseUrl}/api/screenings/{id}");
            return response.IsSuccessStatusCode;
        }

       

        public async Task<bool> BookSeatsAsync(int ScreeningId, string username, List<int> SeatIds)
        {
            var SendRequest = new
            {
                username,
                SelectedSeatsId = SeatIds
            };

            var jsonContent = JsonConvert.SerializeObject(SendRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
               
                var response = await _client.PostAsync($"{_apiBaseUrl}/api/screenings/{ScreeningId}/book", content);
                System.Diagnostics.Debug.WriteLine($"Response : {response}");


                if (response.IsSuccessStatusCode)
                {

                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }

        }

        public async Task<List<HallSeat>> GetAllSeatsAsync(int hallId)
        {
            try
            {
                var response = await _client.GetAsync($"{_apiBaseUrl}/api/halls/{hallId}/seats");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("Raw seats API response:");
                System.Diagnostics.Debug.WriteLine(content);

                var seats = JsonConvert.DeserializeObject<List<HallSeat>>(content);

                //foreach (var seat in seats)
                //{
                //    System.Diagnostics.Debug.WriteLine($"Seat ID: {seat.Id}, Row: {seat.RowNumber}, Number: {seat.SeatNumber}");
                //}

                return seats;
            }
            catch
            {
                throw;
            }
        }
        public class SeatForScreeningDto
        {
            public int Id { get; set; }
            public int ScreeningId { get; set; }
            public int HallSeatId { get; set; }
            public int RowNumber { get; set; }
            public int SeatNumber { get; set; }
            public long? UserId { get; set; }
        }

        public async Task<List<int>> GetTakenSeatsAsync(int screeningId)
        {
            try
            {
                var response = await _client.GetAsync($"{_apiBaseUrl}/api/screenings/{screeningId}/reservedseats");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var reservedSeats = JsonConvert.DeserializeObject<List<SeatForScreeningDto>>(content);

                return reservedSeats.Select(s => s.HallSeatId).ToList();

            }
            catch
            {
                throw;
            }
        }


        public async Task<List<Hall>> getAllHalls()
        {

            try
            {
                var response = await _client.GetAsync($"{_apiBaseUrl}/api/halls");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(content);

                var halls = JsonConvert.DeserializeObject<List<Hall>>(content);

                //foreach (var hall in halls)
                //{
                //    System.Diagnostics.Debug.WriteLine($"Hall ID: {hall.Id}, Hall rows: {hall.Rows}, Hall seats per row: {hall.Seats_Per_Row}");
                //}

                return halls;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<TimeOnly>> AllowedTimes(int hall_id, DateOnly date)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"In AllowedTimes:");

                var response = await _client.GetAsync($"{_apiBaseUrl}/api/halls/{hall_id}/{date}");
                response.EnsureSuccessStatusCode();
                System.Diagnostics.Debug.WriteLine($"AResponse:{response}");


                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Content:{content}");
                var allowedTimes = JsonConvert.DeserializeObject<List<TimeOnly>>(content);

                return allowedTimes.ToList();

            }
            catch
            {
                throw;
            }
        }
       
        public async Task<List<string>> GetAvailableTimeSlotsAsync(int hallId, DateOnly date)
        {
            var dateString = date.ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"{_apiBaseUrl}/api/halls/{hallId}/freeslots/{dateString}");
            System.Diagnostics.Debug.WriteLine($"In service afeter response, the response is : {response}");


            if (!response.IsSuccessStatusCode)
                return new List<string>(); 

            var content = await response.Content.ReadAsStringAsync();

            var slots = JsonConvert.DeserializeObject<List<string>>(content);

 

            return slots ?? new List<string>();
        }

    }
}
