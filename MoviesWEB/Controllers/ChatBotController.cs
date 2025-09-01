using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesWEB.Models;
using MoviesWEB.Service.Interface;

namespace MoviesWEB.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class ChatBotController : Controller
    {
        private readonly IChatBotService _chatBotService;

        public ChatBotController(IChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        public async Task<IActionResult> Index()
        {
            var faqs = await _chatBotService.GetAllFaqAsync();
            return View(faqs);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Faq faq)
        {
            if (ModelState.IsValid)
            {
                await _chatBotService.AddFaqAsync(faq);
                return RedirectToAction(nameof(Index));
            }
            return View(faq);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _chatBotService.GetFaqByIdAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Faq faq)
        {
            if (id != faq.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _chatBotService.UpdateFaqAsync(faq);
                return RedirectToAction(nameof(Index));
            }
            return View(faq);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _chatBotService.DeleteFaqAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question is required.");

            var faq = await _chatBotService.GetClosestMatchToFaqAsync(request.Question);

            if (faq != null)
                return Ok(new { answer = faq.Answer });

            return Ok(new { answer = "Sorry, I don't have an answer for that right now." });
        }
        public class ChatRequest
        {
            public string Question { get; set; } = string.Empty;
        }

        // GET: /ChatBot/faqs
        [HttpGet]
        public async Task<IActionResult> Faqs()
        {
            var faqs = await _chatBotService.GetAllFaqAsync();

            System.Diagnostics.Debug.WriteLine("Retrieved FAQs from service:");
            if (faqs != null)
            {
                foreach (var faq in faqs)
                {
                    System.Diagnostics.Debug.WriteLine($"Question: {faq.Question}, Category: {faq.Category}, Answer: {faq.Answer}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("FAQ list is null");
            }

            return Ok(new { faqs = faqs ?? new List<Faq>() });
        }
       
    }
}
