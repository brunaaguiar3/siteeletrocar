using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EletroCarAPI.Data;
using EletroCarAPI.Models;

namespace EletroCarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.Email && c.Senha == request.Senha);

            if (cliente != null)
            {
                return Ok(new
                {
                    success = true,
                    type = "cliente",
                    nome = cliente.Nome,
                    email = cliente.Email,
                    cpf = cliente.CPF,
                    telefone = cliente.Telefone,
                    cnh = cliente.CNH
                });
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Email == request.Email && f.Senha == request.Senha);

            if (funcionario != null)
            {
                return Ok(new
                {
                    success = true,
                    type = "funcionario",
                    nome = funcionario.Nome,
                    email = funcionario.Email,
                    nivel = funcionario.NivelAcesso
                });
            }

            return Unauthorized(new { success = false, message = "E-mail ou senha incorretos" });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request)
        {
            if (await _context.Clientes.AnyAsync(c => c.Email == request.Email))
                return BadRequest(new { success = false, message = "E-mail já cadastrado" });

            if (await _context.Clientes.AnyAsync(c => c.CPF == request.CPF))
                return BadRequest(new { success = false, message = "CPF já cadastrado" });

            var novoCliente = new Cliente
            {
                Nome = request.Nome,
                CPF = request.CPF,
                Email = request.Email,
                Telefone = request.Telefone,
                CNH = request.CNH,
                Senha = request.Senha,
            };

            _context.Clientes.Add(novoCliente);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Conta criada com sucesso!" });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }

    public class RegistrarRequest
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string CNH { get; set; }
        public string Senha { get; set; }
    }
}