using Humanizer.Localisation;
using MoviesWEB.Models;

namespace MoviesWEB.Service.Interface
{
    public interface IMovieService
    {
        Task<List<Movie>> GetMoviesAsync();
        Task<List<Movie>> GetMoviesByGenreAsync(string genre);
        Task<Movie> GetMovieByIdAsync(long id);
        Task<bool> CreateMovieAsync(CreateAndUpdateMovie movie);
        Task<bool> UpdateMovieAsync(int id, CreateAndUpdateMovie updatedMovie);
        Task<bool> DeleteMovieAsync(int id);
        Task<List<Movie>> SearchMoviesAsync(string query);
        Task<List<FutureMovie>> GetFutureMoviesAsync();
        Task<bool> CreateFutureMovieAsync(CreateFutureMovie createMovie);
        Task<bool> DeleteFutureMovieAsync(int id);
        Task<List<String>> GetAllGenres();
        Task<(bool success, string message)> UpdateUserRating(long movieId, long userId, int rating);
        Task<int> GetUserRatingForMovie(long movieId,long userId);
        Task<List<Movie>> GetTopNMoviesAsync(int n);
    }
}
