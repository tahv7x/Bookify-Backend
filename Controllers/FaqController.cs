using Bookify_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaqController : BaseController
    {
        private readonly BookifyDbContext _context;

        public FaqController(BookifyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetFaqs()
        {
            var faqs = await _context.Faqs
                .OrderBy(f => f.IdFaq)
                .ToListAsync();
            return Ok(faqs);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddFaq([FromBody] FaqDto dto)
        {
            var faq = new Faq
            {
                Question = dto.Question,
                Reponse = dto.Reponse
            };

            _context.Faqs.Add(faq);
            return await SaveAsyncChanges(_context, faq);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateFaq(int id, [FromBody] FaqDto dto)
        {
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound("FAQ non trouvée.");

            faq.Question = dto.Question;
            faq.Reponse = dto.Reponse;

            return await SaveAsyncChanges(_context, faq);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound("FAQ non trouvée.");

            _context.Faqs.Remove(faq);
            return await SaveAsyncChanges(_context);
        }
    }

    public class FaqDto
    {
        public string Question { get; set; } = null!;
        public string Reponse { get; set; } = null!;
    }
}
