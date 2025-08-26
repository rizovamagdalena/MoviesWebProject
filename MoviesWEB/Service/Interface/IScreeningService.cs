using MoviesWEB.Models;
using static MoviesWEB.Service.Implementation.ScreeningService;

namespace MoviesWEB.Service.Interface
{
    public interface IScreeningService
    {
        Task<List<Screening>> GetScreeningsAsync();
        Task<List<Screening>> GetScreeningsForMovieAsync(long id);
        Task<Screening> GetScreeningByIdAsync(long id);
        Task<bool> CreateScreeningAsync(CreateScreening createRequest);
        Task<bool> UpdateScreeningAsync(int id, Screening updatedScreening);
        Task<bool> DeleteScreeningAsync(int id);
        Task<bool> BookSeatsAsync(int ScreeningId,string username,List<int> SeatIds);
        Task<List<HallSeat>> GetAllSeatsAsync(int hallId);
        Task<List<int>> GetTakenSeatsAsync(int screeningId);
        Task<List<Hall>> getAllHalls();
        Task<List<TimeOnly>> AllowedTimes(int hall_id, DateOnly date);
        Task<List<string>> GetAvailableTimeSlotsAsync(int hallId, DateOnly date);
    }
}
