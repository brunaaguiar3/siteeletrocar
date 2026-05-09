using System;
using System.Collections.Generic;
using System.Linq;

namespace EletroCarConsole
{
    class Program
    {
        // DADOS
        static List<Carro> carsData;
        static List<Reserva> userReservations;
        static List<Vistoria> activeInspections;
        static List<Funcionario> funcionarios;
        static List<ClienteBD> clientesBD;
        static Cliente clienteAtual;
        static bool adminLogado = false;
        static string activeRentalToken = null;
        static string activeRentalCar = null;
        static int pendingReturnCarId;
        static int currentInspectionId;

        class Carro
        {
            public int id;
            public string name;
            public string brand;
            public string grad;
            public string image;
            public string emoji;
            public string autonomy;
            public int autonomyNum;
            public int price;
            public int seats;
            public int battery;
            public int year;
            public string accel;
            public string charge;
            public string tag;
            public string status;
            public string localizacao;
            public double lat;
            public double lng;
        }

        class Reserva
        {
            public string id;
            public string car;
            public string emoji;
            public string start;
            public string end;
            public int days;
            public int total;
            public string status;
            public string statusType;
        }

        class Vistoria
        {
            public int id;
            public int carId;
            public string carName;
            public string customerName;
            public string date;
            public List<string> images;
            public bool hasVideo;
            public string observations;
            public Checklist checklist;
        }

        class Checklist
        {
            public bool damage;
            public bool scratches;
            public bool clean;
        }

        class Cliente
        {
            public string nome;
            public string email;
            public string senha;
            public bool logado;
            public string cpf;
            public string telefone;
            public string cnh;
        }

        class Funcionario
        {
            public string nome;
            public string email;
            public string senha;
            public string nivel_acesso;
        }

        class ClienteBD
        {
            public string nome;
            public string cpf;
            public string email;
            public string telefone;
            public string cnh;
            public string senha;
        }

        static void LimparTela()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                for (int i = 0; i < 50; i++) Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            InicializarDados();
            while (true)
            {
                if (adminLogado) MenuAdmin();
                else if (clienteAtual != null && clienteAtual.logado) MenuCliente();
                else MenuPrincipal();
            }
        }

        static void InicializarDados()
        {
            // Dados dos funcionários
            funcionarios = new List<Funcionario>
            {
                new Funcionario { nome = "Roberto Lima", email = "roberto@eletrocar.com", senha = "123", nivel_acesso = "Administrador" },
                new Funcionario { nome = "Camila Rocha", email = "camila@eletrocar.com", senha = "123", nivel_acesso = "Atendente" },
                new Funcionario { nome = "Lucas Mendes", email = "lucas@eletrocar.com", senha = "123", nivel_acesso = "Gerente" },
                new Funcionario { nome = "Patricia Gomes", email = "patricia@eletrocar.com", senha = "123", nivel_acesso = "Supervisor" },
                new Funcionario { nome = "Eduardo Alves", email = "eduardo@eletrocar.com", senha = "123", nivel_acesso = "Atendente" }
            };

            // Dados dos clientes do banco
            clientesBD = new List<ClienteBD>
            {
                new ClienteBD { nome = "Ana Souza", cpf = "11111111111", email = "ana@email.com", telefone = "11999990001", cnh = "12345601", senha = "123" },
                new ClienteBD { nome = "Carlos Lima", cpf = "22222222222", email = "carlos@email.com", telefone = "11999990002", cnh = "12345602", senha = "123" },
                new ClienteBD { nome = "Juliana Alves", cpf = "33333333333", email = "juliana@email.com", telefone = "11999990003", cnh = "12345603", senha = "123" },
                new ClienteBD { nome = "Marcos Silva", cpf = "44444444444", email = "marcos@email.com", telefone = "11999990004", cnh = "12345604", senha = "123" },
                new ClienteBD { nome = "Fernanda Costa", cpf = "55555555555", email = "fernanda@email.com", telefone = "11999990005", cnh = "12345605", senha = "123" }
            };

            // Dados dos veículos
            carsData = new List<Carro>
            {
                new Carro { id = 1, name = "Tesla Model 3", brand = "Tesla", grad = "grad-tesla-3", image = "images/tesla-model3.jpg", emoji = "🚗", autonomy = "450km", autonomyNum = 450, price = 350, seats = 5, battery = 90, year = 2024, accel = "3.1s", charge = "30 min", tag = "Long Range AWD", status = "available", localizacao = "São Paulo", lat = -23.5615, lng = -46.6556 },
                new Carro { id = 2, name = "Tesla Model Y", brand = "Tesla", grad = "grad-tesla-y", image = "images/tesla-model-y.jpg", emoji = "🚙", autonomy = "500km", autonomyNum = 500, price = 420, seats = 7, battery = 85, year = 2024, accel = "3.7s", charge = "35 min", tag = "AWD Premium", status = "available", localizacao = "São Paulo", lat = -23.5652, lng = -46.6971 },
                new Carro { id = 3, name = "BYD Dolphin", brand = "BYD", grad = "grad-byd-dolphin", image = "images/byd-dolphin-standard.jpg", emoji = "🚗", autonomy = "405km", autonomyNum = 405, price = 180, seats = 5, battery = 95, year = 2024, accel = "7.0s", charge = "40 min", tag = "Standard Range", status = "available", localizacao = "Campinas", lat = -23.6012, lng = -46.6881 },
                new Carro { id = 4, name = "BYD Seal", brand = "BYD", grad = "grad-byd-seal", image = "images/byd-seal.jpg", emoji = "🚗", autonomy = "520km", autonomyNum = 520, price = 280, seats = 5, battery = 80, year = 2024, accel = "3.8s", charge = "32 min", tag = "Performance AWD", status = "unavailable", localizacao = "São Paulo", lat = -23.5632, lng = -46.6930 },
                new Carro { id = 5, name = "Toyota bZ4X", brand = "Toyota", grad = "grad-toyota", image = "images/toyota-bz4x.jpg", emoji = "🚙", autonomy = "460km", autonomyNum = 460, price = 310, seats = 5, battery = 78, year = 2024, accel = "6.5s", charge = "30 min", tag = "AWD Luxury", status = "available", localizacao = "São Bernardo", lat = -23.5831, lng = -46.6361 },
                new Carro { id = 6, name = "Volvo EX30", brand = "Volvo", grad = "grad-volvo", image = "images/volvo-ex30.jpg", emoji = "🚗", autonomy = "476km", autonomyNum = 476, price = 390, seats = 5, battery = 88, year = 2024, accel = "3.6s", charge = "26 min", tag = "Twin Motor", status = "available", localizacao = "Santos", lat = -23.6020, lng = -46.6583 },
                new Carro { id = 7, name = "Nissan Leaf", brand = "Nissan", grad = "grad-nissan", image = "images/nissan-leaf.jpg", emoji = "🚗", autonomy = "270km", autonomyNum = 270, price = 200, seats = 5, battery = 70, year = 2023, accel = "7.9s", charge = "40 min", tag = "Standard 40kWh", status = "maintenance", localizacao = "Osasco", lat = -23.6341, lng = -46.6729 },
                new Carro { id = 8, name = "Renault Kwid E-Tech", brand = "Renault", grad = "grad-renault", image = "images/renault-kwid-tech.jpg", emoji = "🚗", autonomy = "185km", autonomyNum = 185, price = 150, seats = 5, battery = 92, year = 2024, accel = "9.8s", charge = "60 min", tag = "Urban Edition", status = "available", localizacao = "Guarulhos", lat = -23.5366, lng = -46.6697 }
            };

            userReservations = new List<Reserva>
            {
                new Reserva { id = "#R0089", car = "Tesla Model 3", emoji = "🚗", start = "10/01/2025", end = "13/01/2025", days = 3, total = 1050, status = "Concluído", statusType = "success" },
                new Reserva { id = "#R0073", car = "BYD Dolphin", emoji = "🚙", start = "20/12/2024", end = "23/12/2024", days = 3, total = 540, status = "Concluído", statusType = "success" },
                new Reserva { id = "#R0112", car = "Tesla Model Y", emoji = "🚙", start = "28/01/2025", end = "02/02/2025", days = 5, total = 2100, status = "Confirmado", statusType = "info" }
            };

            activeInspections = new List<Vistoria>();
            clienteAtual = null;
            adminLogado = false;
        }

        static void MenuPrincipal()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - MOBILIDADE ELÉTRICA INTELIGENTE          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine(">>> MENU PRINCIPAL <<<");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("  1. Ver Frota de Carros");
            Console.WriteLine("  2. Entrar");
            Console.WriteLine("  3. Criar Conta");
            Console.WriteLine("  4. Área do Funcionário");
            Console.WriteLine("  5. Sair");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            string opcao = Console.ReadLine();
            switch (opcao)
            {
                case "1": ExibirFrota(); break;
                case "2": FazerLogin(); break;
                case "3": FazerRegistro(); break;
                case "4": LoginAdmin(); break;
                case "5": Environment.Exit(0); break;
                default: Console.WriteLine("Opção inválida!"); Console.ReadLine(); break;
            }
        }

        static void MenuCliente()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - MOBILIDADE ELÉTRICA INTELIGENTE          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($">>> BEM-VINDO, {clienteAtual.nome.ToUpper()}! <<<");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("  1. Ver Frota de Carros");
            Console.WriteLine("  2. Fazer Reserva");
            Console.WriteLine("  3. Minhas Reservas");
            Console.WriteLine("  4. Minha Conta");
            Console.WriteLine("  5. Sair da Conta");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            string opcao = Console.ReadLine();
            switch (opcao)
            {
                case "1": ExibirFrota(); break;
                case "2": FazerReserva(); break;
                case "3": ExibirMinhasReservas(); break;
                case "4": ExibirMinhaConta(); break;
                case "5": clienteAtual.logado = false; clienteAtual = null; Console.WriteLine("Você saiu da sua conta."); Console.ReadLine(); break;
                default: Console.WriteLine("Opção inválida!"); Console.ReadLine(); break;
            }
        }

        static void MenuAdmin()
        {
            LimparTela();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ELETROCAR - MOBILIDADE ELÉTRICA INTELIGENTE          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine(">>> PAINEL ADMINISTRATIVO <<<");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("  1. Dashboard");
            Console.WriteLine("  2. Solicitações de Reserva");
            Console.WriteLine("  3. Frota de Carros");
            Console.WriteLine("  4. Vistorias Pendentes");
            Console.WriteLine("  5. Listar Funcionários");
            Console.WriteLine("  6. Listar Clientes");
            Console.WriteLine("  7. Sair do Painel");
            Console.WriteLine();
            Console.Write("Digite sua opção: ");
            string opcao = Console.ReadLine();
            switch (opcao)
            {
                case "1": ExibirDashboard(); break;
                case "2": ExibirSolicitacoesReserva(); break;
                case "3": ExibirFrotaAdmin(); break;
                case "4": ExibirVistoriasPendentes(); break;
                case "5": ListarFuncionarios(); break;
                case "6": ListarClientesBD(); break;
                case "7": adminLogado = false; Console.WriteLine("Você saiu do painel administrativo."); Console.ReadLine(); break;
                default: Console.WriteLine("Opção inválida!"); Console.ReadLine(); break;
            }
        }

        static void ExibirFrota()
        {
            LimparTela();
            Console.WriteLine(">>> NOSSA FROTA DE CARROS ELÉTRICOS <<<");
            Console.WriteLine("=========================================");
            Console.WriteLine();
            Console.WriteLine($"{"ID",-4} {"Modelo",-22} {"Marca",-12} {"Autonomia",-10} {"Preço",-12} {"Bateria",-10} {"Status",-12} {"Localização",-15}");
            Console.WriteLine(new string('-', 105));
            foreach (var carro in carsData)
            {
                string statusTexto = carro.status == "available" ? "Disponível" : (carro.status == "unavailable" ? "Alugado" : (carro.status == "maintenance" ? "Manutenção" : "Aguardando vistoria"));
                Console.WriteLine($"{carro.id,-4} {carro.name,-22} {carro.brand,-12} {carro.autonomy,-10} R$ {carro.price,-9} {carro.battery}%      {statusTexto,-16} {carro.localizacao,-15}");
            }
            Console.WriteLine("\nLegenda: Disponível | Alugado | Manutenção | Aguardando vistoria\n");
            Console.WriteLine("Pressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ExibirFrotaAdmin()
        {
            LimparTela();
            Console.WriteLine(">>> FROTA DE CARROS - ADMIN <<<");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine($"{"ID",-4} {"Modelo",-22} {"Status",-20} {"Bateria",-10} {"Preço",-10} {"Localização",-15}");
            Console.WriteLine(new string('-', 90));
            foreach (var carro in carsData)
            {
                string statusTexto = carro.status == "available" ? "Disponível" : (carro.status == "unavailable" ? "Alugado" : (carro.status == "maintenance" ? "Manutenção" : "Aguardando vistoria"));
                Console.WriteLine($"{carro.id,-4} {carro.name,-22} {statusTexto,-20} {carro.battery}%      R$ {carro.price,-8} {carro.localizacao,-15}");
            }
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void FazerLogin()
        {
            LimparTela();
            Console.WriteLine(">>> LOGIN <<<");
            Console.WriteLine("=============");
            Console.WriteLine();
            Console.Write("E-mail: ");
            string email = Console.ReadLine();
            Console.Write("Senha: ");
            string senha = Console.ReadLine();
            
            // Verifica se é funcionário
            var funcionario = funcionarios.FirstOrDefault(f => f.email == email && f.senha == senha);
            if (funcionario != null)
            {
                adminLogado = true;
                Console.WriteLine($"\nBem-vindo, {funcionario.nome}! Nível: {funcionario.nivel_acesso}");
                Console.ReadLine();
                return;
            }
            
            // Verifica se é cliente do banco
            var clienteBD = clientesBD.FirstOrDefault(c => c.email == email && c.senha == senha);
            if (clienteBD != null)
            {
                clienteAtual = new Cliente { nome = clienteBD.nome, email = clienteBD.email, senha = clienteBD.senha, logado = true, cpf = clienteBD.cpf, telefone = clienteBD.telefone, cnh = clienteBD.cnh };
                Console.WriteLine($"\nBem-vindo(a), {clienteBD.nome}!");
                Console.ReadLine();
                return;
            }
            
            // Cliente padrão para demo
            if (email == "cliente@email.com" && senha == "123456")
            {
                clienteAtual = new Cliente { nome = "João da Silva", email = email, senha = senha, logado = true };
                Console.WriteLine("\nLogin realizado com sucesso!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\nE-mail ou senha incorretos!");
                Console.ReadLine();
            }
        }

        static void FazerRegistro()
        {
            LimparTela();
            Console.WriteLine(">>> CRIAR CONTA <<<");
            Console.WriteLine("===================");
            Console.WriteLine();
            Console.Write("Nome: "); string nome = Console.ReadLine();
            Console.Write("Sobrenome: "); string sobrenome = Console.ReadLine();
            Console.Write("E-mail: "); string email = Console.ReadLine();
            Console.Write("CPF: "); string cpf = Console.ReadLine();
            Console.Write("Telefone: "); string telefone = Console.ReadLine();
            Console.Write("Número da CNH: "); string cnh = Console.ReadLine();
            Console.Write("Senha: "); string senha = Console.ReadLine();
            Console.Write("Confirmar Senha: "); string senha2 = Console.ReadLine();
            
            // Adiciona à lista de clientes
            clientesBD.Add(new ClienteBD { nome = nome + " " + sobrenome, cpf = cpf, email = email, telefone = telefone, cnh = cnh, senha = senha });
            
            Console.WriteLine("\nConta criada com sucesso! Bem-vindo à EletroCar!");
            Console.ReadLine();
        }

        static void LoginAdmin()
        {
            LimparTela();
            Console.WriteLine(">>> ÁREA DO FUNCIONÁRIO <<<");
            Console.WriteLine("===========================");
            Console.WriteLine();
            Console.Write("E-mail corporativo: ");
            string email = Console.ReadLine();
            Console.Write("Senha: ");
            string senha = Console.ReadLine();
            
            var funcionario = funcionarios.FirstOrDefault(f => f.email == email && f.senha == senha);
            if (funcionario != null)
            {
                adminLogado = true;
                Console.WriteLine($"\nAcesso liberado! Bem-vindo, {funcionario.nome} ({funcionario.nivel_acesso})");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\nCredenciais inválidas!");
                Console.ReadLine();
            }
        }

        static void FazerReserva()
        {
            LimparTela();
            Console.WriteLine(">>> FAZER RESERVA <<<");
            Console.WriteLine("=====================");
            Console.WriteLine();
            var disponiveis = carsData.Where(c => c.status == "available").ToList();
            if (disponiveis.Count == 0) { Console.WriteLine("No momento não há carros disponíveis."); Console.ReadLine(); return; }
            Console.WriteLine("Carros disponíveis:");
            foreach (var c in disponiveis) Console.WriteLine($"ID: {c.id} - {c.name} - R$ {c.price}/dia - Local: {c.localizacao}");
            Console.Write("\nDigite o ID do carro: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("ID inválido!"); Console.ReadLine(); return; }
            var carro = disponiveis.FirstOrDefault(c => c.id == id);
            if (carro == null) { Console.WriteLine("Carro não encontrado!"); Console.ReadLine(); return; }
            Console.Write("\nData de Retirada (dd/MM/yyyy): ");
            string start = Console.ReadLine();
            Console.Write("Data de Devolução (dd/MM/yyyy): ");
            string end = Console.ReadLine();
            DateTime startDate = DateTime.ParseExact(start, "dd/MM/yyyy", null);
            DateTime endDate = DateTime.ParseExact(end, "dd/MM/yyyy", null);
            int days = Math.Max(1, (endDate - startDate).Days);
            int sub = days * carro.price;
            int fee = (int)(sub * 0.1);
            int total = sub + fee;
            Console.WriteLine($"\nRESUMO:\n  {days} diária(s) × R$ {carro.price} = R$ {sub}\n  Taxa: R$ {fee}\n  TOTAL: R$ {total}");
            Console.Write("\nConfirmar? (S/N): ");
            if (Console.ReadLine().ToUpper() != "S") { Console.WriteLine("Cancelado."); Console.ReadLine(); return; }
            carro.status = "unavailable";
            string token = "EC-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            activeRentalToken = token;
            activeRentalCar = carro.name;
            userReservations.Insert(0, new Reserva { id = "#R" + new Random().Next(1000, 9999), car = carro.name, emoji = carro.emoji, start = start, end = end, days = days, total = total, status = "Confirmado", statusType = "info" });
            Console.WriteLine($"\nReserva confirmada!\nTOKEN: {token}");
            Console.ReadLine();
        }

        static void ExibirMinhasReservas()
        {
            LimparTela();
            Console.WriteLine(">>> MINHAS RESERVAS <<<");
            Console.WriteLine("=======================");
            Console.WriteLine();
            if (userReservations.Count == 0) Console.WriteLine("Você não possui reservas.");
            else
            {
                Console.WriteLine($"{"ID",-10} {"Carro",-22} {"Retirada",-12} {"Devolução",-12} {"Dias",-6} {"Total",-12} {"Status",-12}");
                Console.WriteLine(new string('-', 90));
                foreach (var r in userReservations)
                    Console.WriteLine($"{r.id,-10} {r.car,-22} {r.start,-12} {r.end,-12} {r.days,-6} R$ {r.total,-10} {r.status}");
            }
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ExibirMinhaConta()
        {
            LimparTela();
            Console.WriteLine(">>> MINHA CONTA <<<");
            Console.WriteLine("===================");
            Console.WriteLine();
            Console.WriteLine($"Nome: {clienteAtual.nome}");
            Console.WriteLine($"E-mail: {clienteAtual.email}");
            Console.WriteLine($"CPF: {clienteAtual.cpf ?? "Não informado"}");
            Console.WriteLine($"Telefone: {clienteAtual.telefone ?? "Não informado"}");
            Console.WriteLine($"CNH: {clienteAtual.cnh ?? "Não informado"}");
            int totalReservas = userReservations.Count;
            int totalGasto = userReservations.Sum(r => r.total);
            Console.WriteLine($"\nReservas realizadas: {totalReservas}");
            Console.WriteLine($"Total gasto: R$ {totalGasto}");
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ExibirDashboard()
        {
            LimparTela();
            Console.WriteLine(">>> DASHBOARD ADMIN <<<");
            Console.WriteLine("=======================");
            Console.WriteLine();
            int disponiveis = carsData.Count(c => c.status == "available");
            int alugados = carsData.Count(c => c.status == "unavailable");
            int manutencao = carsData.Count(c => c.status == "maintenance");
            int vistoria = carsData.Count(c => c.status == "inspection");
            Console.WriteLine($"Carros Disponíveis: {disponiveis}");
            Console.WriteLine($"Carros Alugados: {alugados}");
            Console.WriteLine($"Em Manutenção: {manutencao}");
            Console.WriteLine($"Aguardando Vistoria: {vistoria}");
            Console.WriteLine($"Reservas Hoje: {userReservations.Count}");
            Console.WriteLine($"Clientes Cadastrados: {clientesBD.Count}");
            Console.WriteLine($"Funcionários: {funcionarios.Count}");
            Console.WriteLine($"Receita do Dia: R$ 4.820");
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ExibirSolicitacoesReserva()
        {
            LimparTela();
            Console.WriteLine(">>> SOLICITAÇÕES DE RESERVA <<<");
            Console.WriteLine("================================");
            Console.WriteLine();
            var pendentes = userReservations.Where(r => r.status == "Confirmado").ToList();
            if (pendentes.Count == 0) Console.WriteLine("Não há solicitações pendentes.");
            else
            {
                Console.WriteLine($"{"ID",-10} {"Carro",-22} {"Retirada",-12} {"Devolução",-12} {"Valor",-12}");
                Console.WriteLine(new string('-', 70));
                foreach (var r in pendentes) Console.WriteLine($"{r.id,-10} {r.car,-22} {r.start,-12} {r.end,-12} R$ {r.total,-10}");
                Console.Write("\nDigite o ID para aprovar/reprovar (0=sair): ");
                string id = Console.ReadLine();
                if (id != "0")
                {
                    var reserva = pendentes.FirstOrDefault(r => r.id == id);
                    if (reserva != null)
                    {
                        Console.Write("Aprovar? (S/N): ");
                        if (Console.ReadLine().ToUpper() == "S") Console.WriteLine("Reserva aprovada!");
                        else Console.WriteLine("Reserva recusada.");
                    }
                }
            }
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ExibirVistoriasPendentes()
        {
            LimparTela();
            Console.WriteLine(">>> VISTORIAS PENDENTES <<<");
            Console.WriteLine("===========================");
            Console.WriteLine();
            if (activeInspections.Count == 0) Console.WriteLine("Não há vistorias pendentes.");
            else
            {
                Console.WriteLine($"{"ID",-5} {"Veículo",-22} {"Cliente",-22} {"Data",-12} {"Status",-12}");
                Console.WriteLine(new string('-', 80));
                foreach (var v in activeInspections)
                    Console.WriteLine($"{v.id,-5} {v.carName,-22} {v.customerName,-22} {v.date,-12} Pendente");
            }
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ListarFuncionarios()
        {
            LimparTela();
            Console.WriteLine(">>> LISTA DE FUNCIONÁRIOS <<<");
            Console.WriteLine("=============================");
            Console.WriteLine();
            Console.WriteLine($"{"Nome",-25} {"E-mail",-30} {"Nível de Acesso",-20}");
            Console.WriteLine(new string('-', 75));
            foreach (var f in funcionarios)
                Console.WriteLine($"{f.nome,-25} {f.email,-30} {f.nivel_acesso,-20}");
            Console.WriteLine($"\nTotal de funcionários: {funcionarios.Count}");
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }

        static void ListarClientesBD()
        {
            LimparTela();
            Console.WriteLine(">>> LISTA DE CLIENTES <<<");
            Console.WriteLine("========================");
            Console.WriteLine();
            Console.WriteLine($"{"Nome",-25} {"CPF",-15} {"E-mail",-30} {"Telefone",-15}");
            Console.WriteLine(new string('-', 90));
            foreach (var c in clientesBD)
                Console.WriteLine($"{c.nome,-25} {c.cpf,-15} {c.email,-30} {c.telefone,-15}");
            Console.WriteLine($"\nTotal de clientes: {clientesBD.Count}");
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}