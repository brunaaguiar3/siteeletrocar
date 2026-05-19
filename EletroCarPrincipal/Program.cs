using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace EletroCarConsole
{

    class VeiculoDto
    {
        [JsonPropertyName("id")]        public int    Id         { get; set; }
        [JsonPropertyName("nome")]      public string Nome       { get; set; }
        [JsonPropertyName("marca")]     public string Marca      { get; set; }
        [JsonPropertyName("autonomia")] public int    Autonomia  { get; set; }
        [JsonPropertyName("bateria")]   public string Bateria    { get; set; }
        [JsonPropertyName("status")]    public string Status     { get; set; }
        [JsonPropertyName("localizacao")] public string Localizacao { get; set; }
    }

    class ClienteDto
    {
        [JsonPropertyName("id")]   public int    Id   { get; set; }
        [JsonPropertyName("nome")] public string Nome { get; set; }
    }

    class ReservaDto
    {
        [JsonPropertyName("id")]              public int        Id           { get; set; }
        [JsonPropertyName("clienteId")]       public int        ClienteId    { get; set; }
        [JsonPropertyName("veiculoId")]       public int        VeiculoId    { get; set; }
        [JsonPropertyName("dataRetirada")]    public DateTime   DataRetirada { get; set; }
        [JsonPropertyName("dataDevolucao")]   public DateTime   DataDevolucao{ get; set; }
        [JsonPropertyName("status")]          public string     Status       { get; set; }
        [JsonPropertyName("token")]           public string     Token        { get; set; }
        [JsonPropertyName("valorTotal")]      public decimal    ValorTotal   { get; set; }
        [JsonPropertyName("veiculo")]         public VeiculoDto Veiculo      { get; set; }
        [JsonPropertyName("cliente")]         public ClienteDto Cliente      { get; set; }
    }

    class VistoriaDto
    {
        [JsonPropertyName("id")]               public int      Id              { get; set; }
        [JsonPropertyName("reservaId")]        public int      ReservaId       { get; set; }
        [JsonPropertyName("status")]           public string   Status          { get; set; }
        [JsonPropertyName("checklist")]        public string   Checklist       { get; set; }
        [JsonPropertyName("observacoes")]      public string   Observacoes     { get; set; }
        [JsonPropertyName("dataSolicitacao")]  public DateTime DataSolicitacao { get; set; }
        [JsonPropertyName("reserva")]          public ReservaDto Reserva       { get; set; }
    }

    class LoginResponse
    {
        [JsonPropertyName("success")] public bool   Success { get; set; }
        [JsonPropertyName("type")]    public string Type    { get; set; }
        [JsonPropertyName("nome")]    public string Nome    { get; set; }
        [JsonPropertyName("email")]   public string Email   { get; set; }
        [JsonPropertyName("cpf")]     public string Cpf     { get; set; }
        [JsonPropertyName("telefone")]public string Telefone{ get; set; }
        [JsonPropertyName("cnh")]     public string Cnh     { get; set; }
        [JsonPropertyName("nivel")]   public string Nivel   { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
    }

    class ReservaResponse
    {
        [JsonPropertyName("success")]   public bool   Success   { get; set; }
        [JsonPropertyName("message")]   public string Message   { get; set; }
        [JsonPropertyName("token")]     public string Token     { get; set; }
        [JsonPropertyName("reservaId")] public int    ReservaId { get; set; }
    }

    class GenericResponse
    {
        [JsonPropertyName("success")] public bool   Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
    }

    // ─── Serviço HTTP central ────────────────────────────────

    static class ApiService
    {
        // ⚠️  Ajuste a URL caso sua API rode em outra porta
        private const string BaseUrl = "http://localhost:5186";

        private static readonly HttpClient Http = new HttpClient(
            new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true }
        )
        { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // GET genérico
        public static async Task<T> Get<T>(string path)
        {
            var resp = await Http.GetAsync($"{BaseUrl}/{path}");
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        // POST genérico
        public static async Task<T> Post<T>(string path, object body)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );
            var resp = await Http.PostAsync($"{BaseUrl}/{path}", content);
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        // Testa se a API está online
        public static async Task<bool> Ping()
        {
            try { var r = await Http.GetAsync($"{BaseUrl}/veiculo"); return r.IsSuccessStatusCode || true; }
            catch { return false; }
        }
    }

    // ─── Programa principal ──────────────────────────────────

    class Program
    {
        // Estado em memória (sem listas fictícias — tudo vem da API)
        static string currentUserEmail   = null;
        static string currentUserNome    = null;
        static string currentUserTipo    = null;   // "cliente" ou "funcionario"
        static string activeRentalToken  = null;
        static int    activeRentalCarId  = 0;
        static string activeRentalCarNome= null;

        // ─── Ponto de entrada ────────────────────────────────

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "EletroCar — Mobilidade Elétrica Inteligente";

            Console.WriteLine("Conectando à EletroCarAPI...");
            bool online = await ApiService.Ping();
            if (!online)
            {
                Console.WriteLine("⚠️  Não foi possível conectar à API. Verifique se ela está rodando.");
                Console.WriteLine("   URL esperada: http://localhost:5186");
                Console.WriteLine("\nPressione ENTER para tentar continuar mesmo assim...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("✅ API conectada com sucesso!");
                await Task.Delay(800);
            }

            while (true)
            {
                if (currentUserTipo == "funcionario") await MenuAdmin();
                else if (currentUserTipo == "cliente")  await MenuCliente();
                else                                     await MenuPrincipal();
            }
        }

        // ─── Menus ───────────────────────────────────────────

        static async Task MenuPrincipal()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - MOBILIDADE ELÉTRICA INTELIGENTE            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\n>>> MENU PRINCIPAL <<<");
            Console.WriteLine("================================\n");
            Console.WriteLine("  1. 🚗 Ver Frota de Carros");
            Console.WriteLine("  2. 🔑 Entrar");
            Console.WriteLine("  3. 📝 Criar Conta");
            Console.WriteLine("  4. 🛡️  Área do Funcionário");
            Console.WriteLine("  5. ❌ Sair");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ExibirFrota(); break;
                case "2": await FazerLogin(); break;
                case "3": await FazerRegistro(); break;
                case "4": await LoginAdmin(); break;
                case "5": Console.WriteLine("Obrigado por usar o EletroCar!"); Environment.Exit(0); break;
                default:  Console.WriteLine("Opção inválida!"); Pausar(); break;
            }
        }

        static async Task MenuCliente()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - MOBILIDADE ELÉTRICA INTELIGENTE            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\n>>> BEM-VINDO, {currentUserNome?.ToUpper()}! <<<");
            Console.WriteLine("================================\n");
            Console.WriteLine("  1. 🚗 Ver Frota de Carros");
            Console.WriteLine("  2. 📅 Fazer Reserva");
            Console.WriteLine("  3. 📋 Minhas Reservas");
            Console.WriteLine("  4. 🔧 Finalizar Uso (Devolução)");
            Console.WriteLine("  5. 👤 Minha Conta");
            Console.WriteLine("  6. 🚪 Sair da Conta");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ExibirFrota(); break;
                case "2": await FazerReserva(); break;
                case "3": await ExibirMinhasReservas(); break;
                case "4": await DevolverCarro(); break;
                case "5": ExibirMinhaConta(); break;
                case "6": Logout(); break;
                default:  Console.WriteLine("Opção inválida!"); Pausar(); break;
            }
        }

        static async Task MenuAdmin()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - PAINEL ADMINISTRATIVO                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\n>>> PAINEL ADMINISTRATIVO — {currentUserNome?.ToUpper()} <<<");
            Console.WriteLine("================================\n");
            Console.WriteLine("  1. 🚗 Frota de Carros");
            Console.WriteLine("  2. 🔍 Vistorias Pendentes");
            Console.WriteLine("  3. 🚪 Sair do Painel");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ExibirFrota(); break;
                case "2": await ExibirVistoriasPendentes(); break;
                case "3": Logout(); break;
                default:  Console.WriteLine("Opção inválida!"); Pausar(); break;
            }
        }

        // ─── AUTH ────────────────────────────────────────────

        static async Task FazerLogin()
        {
            LimparTela();
            Console.WriteLine(">>> LOGIN <<<");
            Console.WriteLine("=============\n");
            Console.Write("E-mail: ");
            string email = Console.ReadLine()?.Trim();
            Console.Write("Senha:  ");
            string senha = LerSenha();

            try
            {
                var resp = await ApiService.Post<LoginResponse>("auth/login", new { email, senha });
                if (resp.Success)
                {
                    currentUserEmail = resp.Email;
                    currentUserNome  = resp.Nome;
                    currentUserTipo  = resp.Type; // "cliente" ou "funcionario"
                    Console.WriteLine($"\n✅ Bem-vindo(a), {resp.Nome}!" +
                        (resp.Type == "funcionario" ? $" | Nível: {resp.Nivel}" : ""));
                }
                else
                {
                    Console.WriteLine($"\n❌ {resp.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro ao conectar à API: {ex.Message}");
            }
            Pausar();
        }

        static async Task LoginAdmin()
        {
            // Funcionários também usam o endpoint /auth/login
            await FazerLogin();
            if (currentUserTipo != "funcionario")
            {
                // Desfaz se não for funcionário
                currentUserTipo = null;
                currentUserEmail = null;
                currentUserNome  = null;
                Console.WriteLine("❌ Acesso restrito a funcionários.");
                Pausar();
            }
        }

        static async Task FazerRegistro()
        {
            LimparTela();
            Console.WriteLine(">>> CRIAR CONTA <<<");
            Console.WriteLine("===================\n");

            Console.Write("Nome completo: ");
            string nome = Console.ReadLine()?.Trim();

            Console.Write("E-mail: ");
            string email = Console.ReadLine()?.Trim();

            string cpf      = LerCampoValidado("CPF (11 números): ", ValidarCPF);
            string telefone = LerCampoValidado("Telefone (DDD+9 dígitos, ex: 11999990000): ", ValidarTelefone);
            string cnh      = LerCampoValidado("CNH (11 números): ", ValidarCNH);

            Console.Write("Senha (mín. 6 caracteres): ");
            string senha = LerSenha();
            Console.Write("Confirmar senha: ");
            string senha2 = LerSenha();

            if (senha != senha2)      { Console.WriteLine("\n❌ As senhas não coincidem!"); Pausar(); return; }
            if (senha.Length < 6)     { Console.WriteLine("\n❌ Senha muito curta!");      Pausar(); return; }

            try
            {
                var resp = await ApiService.Post<GenericResponse>("auth/registrar", new
                {
                    nome, cpf, email, telefone, cnh, senha
                });
                Console.WriteLine(resp.Success ? $"\n✅ {resp.Message}" : $"\n❌ {resp.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
            }
            Pausar();
        }

        static void Logout()
        {
            currentUserEmail    = null;
            currentUserNome     = null;
            currentUserTipo     = null;
            activeRentalToken   = null;
            activeRentalCarId   = 0;
            activeRentalCarNome = null;
            Console.WriteLine("Você saiu da sua conta.");
            Pausar();
        }

        // ─── VEÍCULOS ────────────────────────────────────────

        static async Task ExibirFrota()
        {
            LimparTela();
            Console.WriteLine(">>> FROTA DE CARROS ELÉTRICOS <<<");
            Console.WriteLine("==================================\n");
            try
            {
                var lista = await ApiService.Get<List<VeiculoDto>>("veiculo");
                if (lista == null || lista.Count == 0)
                {
                    Console.WriteLine("Nenhum veículo cadastrado.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-4} {"Modelo",-22} {"Marca",-12} {"Autonomia",-10} {"Bateria",-10} {"Status",-18} {"Localização",-15}");
                    Console.WriteLine(new string('-', 100));
                    foreach (var v in lista)
                    {
                        string st = v.Status switch
                        {
                            "disponível" => "✅ Disponível",
                            "alugado"    => "🔴 Alugado",
                            "vistoria"   => "🔍 Em Vistoria",
                            _            => "🔧 Manutenção"
                        };
                        Console.WriteLine($"{v.Id,-4} {v.Nome,-22} {v.Marca,-12} {v.Autonomia}km      {v.Bateria,-10} {st,-22} {v.Localizacao,-15}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao buscar frota: {ex.Message}");
            }
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        // ─── RESERVAS ────────────────────────────────────────

        static async Task FazerReserva()
        {
            LimparTela();
            Console.WriteLine(">>> FAZER RESERVA <<<");
            Console.WriteLine("=====================\n");

            List<VeiculoDto> disponiveis;
            try
            {
                var todos = await ApiService.Get<List<VeiculoDto>>("veiculo");
                disponiveis = todos?.Where(v => v.Status == "disponível").ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao buscar veículos: {ex.Message}"); Pausar(); return;
            }

            if (disponiveis.Count == 0)
            {
                Console.WriteLine("❌ No momento não há carros disponíveis."); Pausar(); return;
            }

            Console.WriteLine("🚗 CARROS DISPONÍVEIS:");
            Console.WriteLine(new string('-', 70));
            foreach (var v in disponiveis)
                Console.WriteLine($"ID: {v.Id,-4} | {v.Nome,-22} | Autonomia: {v.Autonomia}km | 📍 {v.Localizacao}");

            Console.Write("\nDigite o ID do carro: ");
            if (!int.TryParse(Console.ReadLine(), out int carroId))
            {
                Console.WriteLine("ID inválido!"); Pausar(); return;
            }

            var carro = disponiveis.FirstOrDefault(v => v.Id == carroId);
            if (carro == null)
            {
                Console.WriteLine("Carro não encontrado ou indisponível!"); Pausar(); return;
            }

            Console.WriteLine($"\n📌 Veículo: {carro.Nome} | 🔋 Bateria: {carro.Bateria} | 📍 {carro.Localizacao}");

            DateTime retirada = LerData("Data de Retirada (dd/MM/yyyy): ");
            DateTime devolucao;
            do
            {
                devolucao = LerData("Data de Devolução (dd/MM/yyyy): ");
                if (devolucao <= retirada)
                    Console.WriteLine("⚠️  A devolução deve ser após a retirada!");
            } while (devolucao <= retirada);

            int dias = Math.Max(1, (devolucao - retirada).Days);
            // Preço base: buscamos da lista local (a API não expõe preço — ajuste se necessário)
            decimal precoBase = 250m;
            decimal subtotal  = dias * precoBase;
            decimal taxa      = subtotal * 0.1m;
            decimal total     = subtotal + taxa;

            Console.WriteLine($"\n📋 RESUMO DA RESERVA:");
            Console.WriteLine($"   📅 Período: {retirada:dd/MM/yyyy} → {devolucao:dd/MM/yyyy}");
            Console.WriteLine($"   📆 Diárias: {dias}");
            Console.WriteLine($"   💵 Subtotal: R$ {subtotal:N2}");
            Console.WriteLine($"   📝 Taxa (10%): R$ {taxa:N2}");
            Console.WriteLine($"   🎯 TOTAL: R$ {total:N2}");
            Console.WriteLine($"   🔒 Seguro: Incluso");

            Console.Write("\nConfirmar reserva? (S/N): ");
            if (Console.ReadLine()?.Trim().ToUpper() != "S")
            {
                Console.WriteLine("Reserva cancelada."); Pausar(); return;
            }

            try
            {
                var resp = await ApiService.Post<ReservaResponse>("reserva/criar", new
                {
                    carroId,
                    clienteEmail  = currentUserEmail,
                    dataRetirada  = retirada.ToString("yyyy-MM-dd"),
                    dataDevolucao = devolucao.ToString("yyyy-MM-dd"),
                    valorTotal    = total
                });

                if (resp.Success)
                {
                    activeRentalToken   = resp.Token;
                    activeRentalCarId   = carroId;
                    activeRentalCarNome = carro.Nome;
                    Console.WriteLine($"\n✅ RESERVA CONFIRMADA!");
                    Console.WriteLine($"🔑 TOKEN DIGITAL: {resp.Token}");
                    Console.WriteLine($"📌 Guarde este token para desbloquear o veículo.");
                    Console.WriteLine($"📧 Confirmação enviada para {currentUserEmail}");
                }
                else
                {
                    Console.WriteLine($"\n❌ {resp.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
            }
            Pausar();
        }

        static async Task ExibirMinhasReservas()
        {
            LimparTela();
            Console.WriteLine(">>> MINHAS RESERVAS <<<");
            Console.WriteLine("=======================\n");
            try
            {
                // Necessário encodar o email para URL (@ → %40)
                string emailEnc = Uri.EscapeDataString(currentUserEmail);
                var lista = await ApiService.Get<List<ReservaDto>>($"reserva/cliente/{emailEnc}");

                if (lista == null || lista.Count == 0)
                {
                    Console.WriteLine("Você não possui reservas.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-6} {"Veículo",-22} {"Retirada",-12} {"Devolução",-12} {"Total",-14} {"Status",-24} {"Token",-22}");
                    Console.WriteLine(new string('-', 120));
                    foreach (var r in lista)
                    {
                        string st = r.Status switch
                        {
                            "Confirmado"         => "🔵 Confirmado",
                            "Concluído"          => "✅ Concluído",
                            "Aguardando Vistoria"=> "🔍 Ag. Vistoria",
                            _                    => r.Status
                        };
                        string modelo = r.Veiculo?.Nome ?? $"Veículo #{r.VeiculoId}";
                        Console.WriteLine($"{r.Id,-6} {modelo,-22} {r.DataRetirada:dd/MM/yyyy}   {r.DataDevolucao:dd/MM/yyyy}   R$ {r.ValorTotal,-10:N2} {st,-26} {r.Token ?? "-",-22}");
                    }

                    // Permite ativar token de uma reserva confirmada
                    var ativas = lista.Where(r => r.Status == "Confirmado" && r.Token != null).ToList();
                    if (ativas.Any() && activeRentalToken == null)
                    {
                        Console.Write("\n🔑 Deseja ativar o token de uma reserva? Digite o ID (ou ENTER para sair): ");
                        string input = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int rid))
                        {
                            var r = ativas.FirstOrDefault(x => x.Id == rid);
                            if (r != null)
                            {
                                activeRentalToken   = r.Token;
                                activeRentalCarId   = r.VeiculoId;
                                activeRentalCarNome = r.Veiculo?.Nome ?? $"Veículo #{r.VeiculoId}";
                                Console.WriteLine($"✅ Token ativo: {activeRentalToken}");
                            }
                            else Console.WriteLine("Reserva não encontrada.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
            }
            Pausar();
        }

        // ─── DEVOLUÇÃO / VISTORIA ────────────────────────────

        static async Task DevolverCarro()
        {
            LimparTela();
            Console.WriteLine(">>> FINALIZAR USO / DEVOLUÇÃO <<<");
            Console.WriteLine("=================================\n");

            if (activeRentalToken == null)
            {
                Console.WriteLine("❌ Você não possui nenhum aluguel ativo.");
                Console.WriteLine("💡 Vá em 'Minhas Reservas' para ativar um token.");
                Pausar(); return;
            }

            Console.WriteLine($"🚗 Veículo: {activeRentalCarNome}");
            Console.WriteLine($"🔑 Token:   {activeRentalToken}\n");

            Console.WriteLine("📋 CHECKLIST DE DEVOLUÇÃO:");
            Console.WriteLine(new string('-', 40));
            Console.Write("1. Sem danos na lataria? (S/N): ");
            bool semDanos  = Console.ReadLine()?.Trim().ToUpper() == "S";
            Console.Write("2. Sem riscos aparentes? (S/N): ");
            bool semRiscos = Console.ReadLine()?.Trim().ToUpper() == "S";
            Console.Write("3. Limpeza interna OK? (S/N):  ");
            bool limpezaOk = Console.ReadLine()?.Trim().ToUpper() == "S";
            Console.Write("Observações adicionais: ");
            string obs = Console.ReadLine() ?? "";

            // Monta JSON de checklist
            var checklistObj = new
            {
                semDanos,
                semRiscos,
                limpezaOk
            };
            string checklistJson = JsonSerializer.Serialize(checklistObj);

            Console.Write("\n📸 Adicionar fotos? (S/N): ");
            var imagens = new List<string>();
            if (Console.ReadLine()?.Trim().ToUpper() == "S")
            {
                imagens.Add("foto_simulada_1.jpg");
                Console.WriteLine("📷 Foto registrada (simulação).");
            }

            try
            {
                var resp = await ApiService.Post<GenericResponse>("vistoria/solicitar", new
                {
                    reservaToken = activeRentalToken,
                    checklist    = checklistJson,
                    observacoes  = obs,
                    imagens
                });

                if (resp.Success)
                {
                    Console.WriteLine($"\n✅ {resp.Message}");
                    Console.WriteLine("🔍 Um funcionário realizará a vistoria em breve.");
                    Console.WriteLine("📧 Você será notificado(a) quando aprovada.");
                    activeRentalToken   = null;
                    activeRentalCarId   = 0;
                    activeRentalCarNome = null;
                }
                else
                {
                    Console.WriteLine($"\n❌ {resp.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
            }
            Pausar();
        }

        // ─── VISTORIAS (PAINEL ADMIN) ────────────────────────

        static async Task ExibirVistoriasPendentes()
        {
            LimparTela();
            Console.WriteLine(">>> VISTORIAS PENDENTES <<<");
            Console.WriteLine("===========================\n");
            try
            {
                var lista = await ApiService.Get<List<VistoriaDto>>("vistoria/pendentes");

                if (lista == null || lista.Count == 0)
                {
                    Console.WriteLine("✅ Não há vistorias pendentes.");
                    Pausar(); return;
                }

                Console.WriteLine($"{"ID",-6} {"Reserva",-10} {"Veículo",-22} {"Cliente",-22} {"Data Solicitação",-20} {"Status",-12}");
                Console.WriteLine(new string('-', 100));
                foreach (var v in lista)
                {
                    string modelo  = v.Reserva?.Veiculo?.Nome ?? $"Veículo #{v.Reserva?.VeiculoId}";
                    string cliente = v.Reserva?.Cliente?.Nome ?? $"Cliente #{v.Reserva?.ClienteId}";
                    Console.WriteLine($"{v.Id,-6} #{v.ReservaId,-9} {modelo,-22} {cliente,-22} {v.DataSolicitacao:dd/MM/yyyy HH:mm}   ⏳ {v.Status}");
                }

                Console.Write("\nDigite o ID da vistoria para analisar (0=sair): ");
                if (!int.TryParse(Console.ReadLine(), out int vid) || vid == 0) { Pausar(); return; }

                var vistoria = lista.FirstOrDefault(v => v.Id == vid);
                if (vistoria == null) { Console.WriteLine("Vistoria não encontrada."); Pausar(); return; }

                Console.WriteLine($"\n📋 DETALHES DA VISTORIA #{vistoria.Id}");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine($"   Veículo:     {vistoria.Reserva?.Veiculo?.Nome ?? "-"}");
                Console.WriteLine($"   Token:       {vistoria.Reserva?.Token ?? "-"}");
                Console.WriteLine($"   Observações: {vistoria.Observacoes ?? "Nenhuma"}");
                Console.WriteLine($"   Checklist:   {vistoria.Checklist}");

                Console.Write("\nAprovar vistoria? (S=Aprovar / N=Reprovar / ENTER=Cancelar): ");
                string dec = Console.ReadLine()?.Trim().ToUpper();

                if (dec == "S")
                {
                    var r = await ApiService.Post<GenericResponse>($"vistoria/aprovar/{vid}", null);
                    Console.WriteLine(r.Success ? $"\n✅ {r.Message}" : $"\n❌ {r.Message}");
                }
                else if (dec == "N")
                {
                    Console.Write("Motivo da reprovação: ");
                    string motivo = Console.ReadLine() ?? "Não informado";
                    var r = await ApiService.Post<GenericResponse>($"vistoria/reprovar/{vid}", motivo);
                    Console.WriteLine(r.Success ? $"\n✅ {r.Message}" : $"\n❌ {r.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
            }
            Pausar();
        }

        // ─── CONTA DO CLIENTE ────────────────────────────────

        static void ExibirMinhaConta()
        {
            LimparTela();
            Console.WriteLine(">>> MINHA CONTA <<<");
            Console.WriteLine("===================\n");
            Console.WriteLine($"   👤 Nome:   {currentUserNome}");
            Console.WriteLine($"   📧 E-mail: {currentUserEmail}");
            if (activeRentalToken != null)
                Console.WriteLine($"   🔑 Token ativo: {activeRentalToken} ({activeRentalCarNome})");
            Pausar();
        }

        // ─── UTILITÁRIOS ─────────────────────────────────────

        static void LimparTela()
        {
            try { Console.Clear(); }
            catch { for (int i = 0; i < 50; i++) Console.WriteLine(); }
        }

        static void Pausar()
        {
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static string LerSenha()
        {
            string senha = "";
            ConsoleKeyInfo tecla;
            do
            {
                tecla = Console.ReadKey(true);
                if (tecla.Key != ConsoleKey.Backspace && tecla.Key != ConsoleKey.Enter)
                {
                    senha += tecla.KeyChar;
                    Console.Write("*");
                }
                else if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0)
                {
                    senha = senha[..^1];
                    Console.Write("\b \b");
                }
            } while (tecla.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return senha;
        }

        static DateTime LerData(string prompt)
        {
            DateTime dt;
            do
            {
                Console.Write(prompt);
            }
            while (!DateTime.TryParseExact(
                Console.ReadLine()?.Trim(),
                "dd/MM/yyyy",
                null,
                System.Globalization.DateTimeStyles.None,
                out dt
            ));
            return dt;
        }

        static string LerCampoValidado(string prompt, Func<string, bool> validador)
        {
            string valor = "";
            bool ok = false;
            while (!ok)
            {
                Console.Write(prompt);
                valor = Console.ReadLine()?.Trim() ?? "";
                ok = validador(valor);
            }
            return valor;
        }

        // ─── VALIDAÇÕES ───────────────────────────────────────

        static bool ValidarCPF(string cpf)
        {
            cpf = Regex.Replace(cpf, "[^0-9]", "");
            if (cpf.Length != 11) { Console.WriteLine($"❌ CPF deve ter 11 dígitos (você digitou {cpf.Length})."); return false; }
            if (cpf.All(c => c == cpf[0])) { Console.WriteLine("❌ CPF inválido."); return false; }

            int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tmp = cpf[..9];
            int soma = 0;
            for (int i = 0; i < 9; i++) soma += int.Parse(tmp[i].ToString()) * m1[i];
            int r1 = soma % 11; r1 = r1 < 2 ? 0 : 11 - r1;
            tmp += r1;
            soma = 0;
            for (int i = 0; i < 10; i++) soma += int.Parse(tmp[i].ToString()) * m2[i];
            int r2 = soma % 11; r2 = r2 < 2 ? 0 : 11 - r2;
            bool valido = cpf.EndsWith($"{r1}{r2}");
            if (!valido) Console.WriteLine("❌ CPF inválido (dígitos verificadores).");
            return valido;
        }

        static bool ValidarTelefone(string tel)
        {
            tel = Regex.Replace(tel, "[^0-9]", "");
            if (tel.Length != 11) { Console.WriteLine($"❌ Telefone deve ter 11 dígitos (você digitou {tel.Length})."); return false; }
            int ddd = int.Parse(tel[..2]);
            if (ddd < 11 || ddd > 99) { Console.WriteLine("❌ DDD inválido."); return false; }
            if (tel[2] != '9') { Console.WriteLine("❌ Celular deve começar com 9."); return false; }
            return true;
        }

        static bool ValidarCNH(string cnh)
        {
            cnh = Regex.Replace(cnh, "[^0-9]", "");
            if (cnh.Length != 11) { Console.WriteLine($"❌ CNH deve ter 11 dígitos (você digitou {cnh.Length})."); return false; }
            if (cnh.All(c => c == cnh[0])) { Console.WriteLine("❌ CNH inválida."); return false; }
            return true;
        }
    }
}
