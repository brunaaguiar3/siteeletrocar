using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EletroCarAPI.Data;
using EletroCarAPI.Models;

namespace EletroCarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("criar")]
        public async Task<IActionResult> CriarReserva([FromBody] ReservaRequest request)
        {
            // Buscar cliente
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.ClienteEmail);

            if (cliente == null)
            {
                return BadRequest(new { success = false, message = "Cliente não encontrado" });
            }

            // Buscar veículo (Carro)
            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(c => c.Id == request.CarroId);

            if (veiculo == null)
            {
                return BadRequest(new { success = false, message = "Veículo não encontrado" });
            }

            // Verificar se veículo está disponível
            if (veiculo.Status != "disponível")
            {
                return BadRequest(new { success = false, message = "Veículo não está disponível" });
            }

            // Gerar token
            string token = "EC-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() + "-" + new Random().Next(100, 999);

            // Criar reserva
            var reserva = new Reserva
            {
                ClienteId = cliente.Id,
                VeiculoId = veiculo.Id,
                DataRetirada = DateTime.Parse(request.DataRetirada),
                DataDevolucao = DateTime.Parse(request.DataDevolucao),
                Status = "Confirmado",
                Token = token,
                ValorTotal = request.ValorTotal
            };

            _context.Reservas.Add(reserva);

            // Atualizar status do veículo
            veiculo.Status = "alugado";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Reserva confirmada!",
                token = token,
                reservaId = reserva.Id
            });
        }

        // Listar reservas de um cliente
        [HttpGet("cliente/{email}")]
        public async Task<IActionResult> GetReservasPorCliente(string email)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
            {
                return BadRequest(new { success = false, message = "Cliente não encontrado" });
            }

            var reservas = await _context.Reservas
                .Where(r => r.ClienteId == cliente.Id)
                .Include(r => r.Veiculo)
                .OrderByDescending(r => r.DataRetirada)
                .ToListAsync();

            return Ok(reservas);
        }
    }

    public class ReservaRequest
    {
        public int CarroId { get; set; }
        public string ClienteEmail { get; set; }
        public string DataRetirada { get; set; }
        public string DataDevolucao { get; set; }
        public decimal ValorTotal { get; set; }
    }
}