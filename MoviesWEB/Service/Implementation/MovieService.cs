using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Options;
using MoviesWEB.Controllers;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace MoviesWEB.Service.Implementation
{
    public class MovieService : IMovieService
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieService(IOptions<DbSettings> settings, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _apiBaseUrl = settings.Value.DbApi ?? throw new ArgumentNullException(nameof(settings.Value.DbApi));
            _httpContextAccessor = httpContextAccessor;
        }

        private long? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim != null && long.TryParse(claim, out var userId))
                return userId;
            return null;
        }

        private async Task PopulateUserRatingAsync(Movie movie)
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/{movie.Id}/rating/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var rating = await response.Content.ReadFromJsonAsync<MovieRating>();
                    movie.UserRating = rating;
                }
                else
                {
                    movie.UserRating = null;
                }
            }
            else
            {
                movie.UserRating = null;
            }
        }

        private async Task PopulateUserRatingsAsync(IEnumerable<Movie> movies)
        {
            var tasks = movies.Select(PopulateUserRatingAsync);
            await Task.WhenAll(tasks);
        }

        public async Task<List<Movie>> GetMoviesAsync()
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies");

            if (!response.IsSuccessStatusCode)
            {
                return new List<Movie>();
            }
            System.Diagnostics.Debug.WriteLine($"In  GetMoviesAsync response:{response}");


            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"In  GetMoviesAsync json:{json}");

            var movies = JsonConvert.DeserializeObject<List<Movie>>(json) ?? new List<Movie>();

            await PopulateUserRatingsAsync(movies);

            return movies;

        }

        public async Task<List<Movie>> GetMoviesByGenreAsync(string genre)
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/genre/{genre}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<Movie>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var movies = JsonConvert.DeserializeObject<List<Movie>>(json) ?? new List<Movie>();

            await PopulateUserRatingsAsync(movies);
            return movies;
        }

        public async Task<Movie> GetMovieByIdAsync(long id)
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var movie = JsonConvert.DeserializeObject<Movie>(json);

            if (movie != null)
                await PopulateUserRatingAsync(movie);

            return movie;
        }

        public async Task<bool> CreateMovieAsync(CreateAndUpdateMovie movie)
        {
            var json = JsonConvert.SerializeObject(movie);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_apiBaseUrl}/api/movies", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMovieAsync(int id, CreateAndUpdateMovie updatedMovie)
        {

            System.Diagnostics.Debug.WriteLine($"In UpdateMovieAsync  updateMovie: Id={updatedMovie.Id}, Name={updatedMovie.Name}");



            var jsonContent = JsonConvert.SerializeObject(updatedMovie);
            System.Diagnostics.Debug.WriteLine($"json={jsonContent}, ");

            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            System.Diagnostics.Debug.WriteLine($"requestcontent={requestContent}, ");


            var response = await _client.PutAsync($"{_apiBaseUrl}/api/movies/{id}", requestContent);
            System.Diagnostics.Debug.WriteLine($"response={response}, ");


            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var response = await _client.DeleteAsync($"{_apiBaseUrl}/api/movies/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Movie>> SearchMoviesAsync(string query) 
        {

            var allMovies = await GetMoviesAsync();

            var moviesThatMatchQuery = allMovies
            .Where(m => m.Name != null && m.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

            //foreach (var movie in moviesThatMatchQuery)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Found Movie: Id={movie.Id}, Name={movie.Name}, Poster_Path={movie.Poster_Path}");
            //}

            return moviesThatMatchQuery;
        }


        public async Task<List<FutureMovie>> GetFutureMoviesAsync()
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/futuremovies");

            if (!response.IsSuccessStatusCode)
            {
                return new List<FutureMovie>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<FutureMovie>>(json) ?? new List<FutureMovie>();

        }

        public async Task<bool> CreateFutureMovieAsync(CreateFutureMovie createMovie)
        {
            var json = JsonConvert.SerializeObject(createMovie);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_apiBaseUrl}/api/movies/futuremovies", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFutureMovieAsync(int id)
        {
            var response = await _client.DeleteAsync($"{_apiBaseUrl}/api/movies/futuremovies/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<String>> GetAllGenres()
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/genres");


            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public async Task<(bool success, string message)> UpdateUserRating(long movieId, long userId, int rating,string comment)
        {
            var info = new CreateRating
            {
                MovieId = movieId,   
                UserId = userId,
                Rating = rating,
                Comment = comment
            };

            var response = await _client.PutAsJsonAsync($"{_apiBaseUrl}/api/movies/{movieId}/rating", info);

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, content);

            return (false, content);
        }

        public async Task<MovieRating> GetUserRatingForMovie(long movieId,long userId)
        {
            System.Diagnostics.Debug.WriteLine($"in :GetUserRatingForMovie ");

           
            System.Diagnostics.Debug.WriteLine($"userid:{userId}  ");


            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/{movieId}/rating/{userId}");
            System.Diagnostics.Debug.WriteLine($"response:{response}  ");


            if (!response.IsSuccessStatusCode)
                return null;

            var userRating = await response.Content.ReadFromJsonAsync<MovieRating>();
            System.Diagnostics.Debug.WriteLine($"readmform json:{userRating}  ");

            
            return userRating;
        }

        public async Task<List<Movie>> GetTopNMoviesAsync(int n)
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/movies/toprated/{n}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<Movie>();
            }


            var json = await response.Content.ReadAsStringAsync();

            var movies = JsonConvert.DeserializeObject<List<Movie>>(json) ?? new List<Movie>();

            await PopulateUserRatingsAsync(movies);

            return movies;

        }


    }
}
