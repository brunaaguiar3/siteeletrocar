using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EletroCarAPI.Data;
using EletroCarAPI.Models;

namespace EletroCarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VeiculoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/veiculo — lista todos os veículos
        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var veiculos = await _context.Veiculos.ToListAsync();
            return Ok(veiculos);
        }

        // GET api/veiculo/disponiveis — só os disponíveis
        [HttpGet("disponiveis")]
        public async Task<IActionResult> GetDisponiveis()
        {
            var veiculos = await _context.Veiculos
                .Where(v => v.Status == "disponível")
                .ToListAsync();
            return Ok(veiculos);
        }

        // GET api/veiculo/{id} — um veículo específico
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return NotFound(new { success = false, message = "Veículo não encontrado" });
            return Ok(veiculo);
        }
    }
}