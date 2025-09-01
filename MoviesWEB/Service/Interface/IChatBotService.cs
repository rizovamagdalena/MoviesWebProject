using MoviesWEB.Models;

namespace MoviesWEB.Service.Interface
{
    public interface IChatBotService
    {
        Task<List<Faq>> GetAllFaqAsync();                    
        Task<Faq?> GetFaqByIdAsync(int id);                       
        Task AddFaqAsync(Faq faq);                            
        Task UpdateFaqAsync(Faq faq);                              
        Task DeleteFaqAsync(int id);                         
        Task<Faq?> GetClosestMatchToFaqAsync(string userQuestion);  
    }
}
