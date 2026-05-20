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
            try
            {
                if (request == null || request.CarroId <= 0 || string.IsNullOrWhiteSpace(request.ClienteEmail))
                {
                    return BadRequest(new { success = false, message = "Dados da reserva inválidos." });
                }

                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.Email == request.ClienteEmail)
                    .Select(c => new { c.Id })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return BadRequest(new { success = false, message = "Cliente não encontrado" });
                }

                // Busca só Id e Status — evita erro de tipo em colunas texto (autonomia, bateria, etc.)
                var veiculo = await _context.Veiculos
                    .AsNoTracking()
                    .Where(v => v.Id == request.CarroId)
                    .Select(v => new { v.Id, v.Status })
                    .FirstOrDefaultAsync();

                if (veiculo == null)
                {
                    return BadRequest(new { success = false, message = "Veículo não encontrado no banco. Verifique se o ID do carro existe na tabela Veiculo." });
                }

                var status = (veiculo.Status ?? "").Trim().ToLowerInvariant();
                if (status != "disponível" && status != "disponivel")
                {
                    return BadRequest(new { success = false, message = "Veículo não está disponível" });
                }

                if (!DateTime.TryParse(request.DataRetirada, out var dataRetirada) ||
                    !DateTime.TryParse(request.DataDevolucao, out var dataDevolucao))
                {
                    return BadRequest(new { success = false, message = "Datas inválidas." });
                }

                string token = "EC-" + Guid.NewGuid().ToString()[..8].ToUpper() + "-" + Random.Shared.Next(100, 999);

                var reserva = new Reserva
                {
                    ClienteId = cliente.Id,
                    VeiculoId = veiculo.Id,
                    DataRetirada = dataRetirada,
                    DataDevolucao = dataDevolucao,
                    Status = "Confirmado",
                    Token = token,
                    ValorTotal = request.ValorTotal
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                await _context.Veiculos
                    .Where(v => v.Id == veiculo.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(v => v.Status, "alugado"));

                return Ok(new
                {
                    success = true,
                    message = "Reserva confirmada!",
                    token,
                    reservaId = reserva.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao criar reserva. Verifique se as tabelas Reserva e Veiculo estão alinhadas com a API.",
                    detalhe = ex.Message
                });
            }
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