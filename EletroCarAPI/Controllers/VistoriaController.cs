using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EletroCarAPI.Data;
using EletroCarAPI.Models;

namespace EletroCarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VistoriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VistoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listar todas as vistorias pendentes (para qualquer funcionário)
        [HttpGet("pendentes")]
        public async Task<IActionResult> GetVistoriasPendentes()
        {
            var vistorias = await _context.Vistorias
                .Where(v => v.Status == "Pendente")
                .Include(v => v.Reserva)
                    .ThenInclude(r => r.Cliente)
                .Include(v => v.Reserva)
                    .ThenInclude(r => r.Veiculo)
                .ToListAsync();

            return Ok(vistorias);
        }

        // Solicitar vistoria (quando cliente devolve)
        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarVistoria([FromBody] VistoriaRequest request)
        {
            // Buscar reserva pelo token
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Token == request.ReservaToken);

            if (reserva == null)
            {
                return BadRequest(new { success = false, message = "Reserva não encontrada" });
            }

            // Criar vistoria
            var vistoria = new Vistoria
            {
                ReservaId = reserva.Id,
                Status = "Pendente",
                Checklist = request.Checklist,
                Observacoes = request.Observacoes,
                Imagens = request.Imagens != null ? string.Join(",", request.Imagens) : "",
                DataSolicitacao = DateTime.Now
            };

            _context.Vistorias.Add(vistoria);

            // Atualizar status da reserva
            reserva.Status = "Aguardando Vistoria";

            // Atualizar status do veículo
            var veiculo = await _context.Veiculos.FindAsync(vistoria.Reserva.VeiculoId);
            if (veiculo != null)
            {
                veiculo.Status = "vistoria";
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vistoria solicitada com sucesso" });
        }

        // Aprovar vistoria (qualquer funcionário pode fazer)
        [HttpPost("aprovar/{id}")]
        public async Task<IActionResult> AprovarVistoria(int id)
        {
            var vistoria = await _context.Vistorias
                .Include(v => v.Reserva)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vistoria == null)
            {
                return BadRequest(new { success = false, message = "Vistoria não encontrada" });
            }

            vistoria.Status = "Aprovada";
            vistoria.DataAprovacao = DateTime.Now;

            // Atualizar status da reserva
            vistoria.Reserva.Status = "Concluído";

            // Atualizar status do veículo
            var veiculo = await _context.Veiculos.FindAsync(vistoria.Reserva.VeiculoId);

            if (veiculo != null)
            {
                veiculo.Status = "disponível";
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vistoria aprovada com sucesso" });
        }

        // Reprovar vistoria
        [HttpPost("reprovar/{id}")]
        public async Task<IActionResult> ReprovarVistoria(int id, [FromBody] string motivo)
        {
            var vistoria = await _context.Vistorias.FindAsync(id);

            if (vistoria == null)
            {
                return BadRequest(new { success = false, message = "Vistoria não encontrada" });
            }

            vistoria.Status = "Reprovada";
            vistoria.MotivoReprovacao = motivo;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vistoria reprovada" });
        }
    }

    public class VistoriaRequest
    {
        public string ReservaToken { get; set; }
        public string Checklist { get; set; }
        public string Observacoes { get; set; }
        public List<string> Imagens { get; set; }
    }
}