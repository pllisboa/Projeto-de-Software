using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

class Program
{

    // DADOS DO SISTEMA


    static string pastaDados = Path.Combine(AppContext.BaseDirectory, "dados");

    static List<Pao> paes;
    static List<Doce> doces;
    static List<Bebida> bebidas;
    static List<Venda> vendas;

    // Largura padrão (em caracteres) dos quadros da interface.
    const int Largura = 70;

    // Maior preço unitário aceito (também evita estouro nos cálculos).
    const decimal PrecoMaximo = 99999.99m;

    // Linhas dos arquivos que não puderam ser lidas ao iniciar o sistema.
    static List<string> linhasIgnoradas = new List<string>();

    
    // MÉTODO PRINCIPAL
    

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Padaria Trigo & Mel";

        // Garante moeda em reais (R$) em qualquer computador.
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

        try
        {
            Directory.CreateDirectory(pastaDados);

            // Carregamos os dados salvos anteriormente.
            paes = CarregarPaes();
            doces = CarregarDoces();
            bebidas = CarregarBebidas();
            vendas = CarregarVendas();

            GuardarLinhasIgnoradas();
        }
        catch (Exception)
        {
            // Encerra sem salvar para não sobrescrever os arquivos existentes.
            Erro(
                "Não foi possível carregar os dados. " +
                "Verifique as permissões da pasta e tente novamente."
            );
            Pausar();
            return;
        }

        if (linhasIgnoradas.Count > 0)
        {
            Mensagem(
                linhasIgnoradas.Count + " linha(s) inválida(s) ignorada(s) " +
                "e copiada(s) para linhas_ignoradas.txt.",
                ConsoleColor.Yellow
            );
            Pausar();
        }

        //  MENU PRINCIPAL

        while (true)
        {
            Console.Clear();
            Titulo(
                "PADARIA TRIGO & MEL",
                "Sistema de Gestão de Estoque e Vendas"
            );

            MostrarMenu(
                "MENU PRINCIPAL",
                "1|Cadastrar produto",
                "2|Realizar venda",
                "3|Consultar ou excluir produto",
                "4|Imprimir estoque",
                "5|Relatório de vendas",
                "0|Sair"
            );

            Console.WriteLine();
            Prompt("Escolha uma opção: ");

            string opcao = Console.ReadLine() ?? "";

            if (opcao == "1")
            {
                MenuCadastro();
            }
            else if (opcao == "2")
            {
                MenuVenda();
            }
            else if (opcao == "3")
            {
                MenuConsultaExclusao();
            }
            else if (opcao == "4")
            {
                ImprimirEstoque();
                Pausar();
            }
            else if (opcao == "5")
            {
                ImprimirRelatorio();
                Pausar();
            }
            else if (opcao == "0")
            {
                if (SalvarTudo())
                {
                    Mensagem(
                        "Dados salvos. Volte sempre à Padaria Trigo & Mel!",
                        ConsoleColor.Green
                    );
                }
                else
                {
                    Mensagem(
                        "Nem todos os dados foram salvos. " +
                        "Veja os erros acima.",
                        ConsoleColor.Yellow
                    );
                    Pausar();
                }

                break;
            }
            else
            {
                Erro("Opção inválida. Escolha uma opção do menu.");
                Pausar();
            }
        }
    }

    
    // CAMADA DE APRESENTAÇÃO: MENUS E TELAS
    

    static void MenuCadastro()
    {
        Console.Clear();
        Titulo("CADASTRAR PRODUTO");

        MostrarMenu(
            "TIPO DE PRODUTO",
            "1|Pão",
            "2|Doce/Bolo",
            "3|Bebida",
            "0|Voltar"
        );

        Console.WriteLine();
        Prompt("Tipo de produto: ");

        string tipo = Console.ReadLine() ?? "";

        if (tipo == "1")
        {
            CadastrarPao();
        }
        else if (tipo == "2")
        {
            CadastrarDoce();
        }
        else if (tipo == "3")
        {
            CadastrarBebida();
        }
        else if (tipo != "0")
        {
            Erro("Tipo de produto inválido.");
            Pausar();
        }
    }

    static void MenuVenda()
    {
        Console.Clear();
        Titulo("REALIZAR VENDA");

        MostrarMenu(
            "CATEGORIA DO PRODUTO",
            "1|Pão",
            "2|Doce/Bolo",
            "3|Bebida",
            "0|Voltar"
        );

        Console.WriteLine();
        Prompt("Categoria: ");

        string categoria = Console.ReadLine() ?? "";

        if (categoria == "0")
        {
            return;
        }

        if (categoria != "1" && categoria != "2" && categoria != "3")
        {
            Erro("Categoria inválida.");
            Pausar();
            return;
        }

        Console.WriteLine();

        string nome = LerTexto("Nome do produto: ");
        int quantidade = LerInteiroPositivo("Quantidade a vender: ");

        if (categoria == "1")
        {
            VenderPao(nome, quantidade);
        }
        else if (categoria == "2")
        {
            VenderDoce(nome, quantidade);
        }
        else
        {
            VenderBebida(nome, quantidade);
        }

        Pausar();
    }

    static void MenuConsultaExclusao()
    {
        Console.Clear();
        Titulo("CONSULTAR OU EXCLUIR PRODUTO");

        MostrarMenu(
            "CATEGORIA DO PRODUTO",
            "1|Pão",
            "2|Doce/Bolo",
            "3|Bebida",
            "0|Voltar"
        );

        Console.WriteLine();
        Prompt("Categoria: ");

        string categoria = Console.ReadLine() ?? "";

        if (categoria == "0")
        {
            return;
        }

        if (categoria != "1" && categoria != "2" && categoria != "3")
        {
            Erro("Categoria inválida.");
            Pausar();
            return;
        }

        Console.WriteLine();

        string nome = LerTexto("Nome do produto: ");
        bool encontrado = false;

        if (categoria == "1")
        {
            encontrado = ConsultarPao(nome);
        }
        else if (categoria == "2")
        {
            encontrado = ConsultarDoce(nome);
        }
        else
        {
            encontrado = ConsultarBebida(nome);
        }

        if (encontrado)
        {
            Console.WriteLine();
            Prompt(
                "Deseja excluir este produto? (S/N): ",
                ConsoleColor.Yellow
            );

            string resposta = (Console.ReadLine() ?? "")
                .Trim()
                .ToUpperInvariant();

            if (resposta == "S")
            {
                bool excluido = false;

                if (categoria == "1")
                {
                    excluido = ExcluirPao(nome);
                }
                else if (categoria == "2")
                {
                    excluido = ExcluirDoce(nome);
                }
                else
                {
                    excluido = ExcluirBebida(nome);
                }

                if (excluido)
                {
                    Mensagem(
                        "Produto excluído com sucesso.",
                        ConsoleColor.Green
                    );
                }
            }
            else
            {
                Mensagem(
                    "Exclusão cancelada.",
                    ConsoleColor.Yellow
                );
            }
        }

        Pausar();
    }

    static void ImprimirEstoque()
    {
        Console.Clear();
        Titulo("ESTOQUE ATUAL");

        //  PÃES 

        Secao("PÃES");

        if (paes.Count == 0)
        {
            Vazio("Nenhum pão cadastrado.");
        }
        else
        {
            int[] larg = { 22, 16, 7, 12 };
            bool[] dir = { false, false, true, true };

            TabelaBorda('┌', '┬', '┐', larg);
            TabelaLinha(
                new string[] { "Nome", "Tipo", "Estoque", "Preço" },
                larg, dir, ConsoleColor.Yellow
            );
            TabelaBorda('├', '┼', '┤', larg);

            foreach (Pao pao in paes)
            {
                TabelaLinha(
                    new string[]
                    {
                        pao.Nome,
                        pao.Tipo,
                        pao.Quantidade.ToString(),
                        pao.Preco.ToString("C2")
                    },
                    larg, dir, CorEstoque(pao.Quantidade)
                );
            }

            TabelaBorda('└', '┴', '┘', larg);
        }

        //  DOCES E BOLOS 

        Secao("DOCES E BOLOS");

        if (doces.Count == 0)
        {
            Vazio("Nenhum doce/bolo cadastrado.");
        }
        else
        {
            int[] larg = { 22, 16, 7, 12 };
            bool[] dir = { false, false, true, true };

            TabelaBorda('┌', '┬', '┐', larg);
            TabelaLinha(
                new string[] { "Nome", "Sabor", "Estoque", "Preço" },
                larg, dir, ConsoleColor.Yellow
            );
            TabelaBorda('├', '┼', '┤', larg);

            foreach (Doce doce in doces)
            {
                TabelaLinha(
                    new string[]
                    {
                        doce.Nome,
                        doce.Sabor,
                        doce.Quantidade.ToString(),
                        doce.Preco.ToString("C2")
                    },
                    larg, dir, CorEstoque(doce.Quantidade)
                );
            }

            TabelaBorda('└', '┴', '┘', larg);
        }

        //  BEBIDAS 

        Secao("BEBIDAS");

        if (bebidas.Count == 0)
        {
            Vazio("Nenhuma bebida cadastrada.");
        }
        else
        {
            int[] larg = { 20, 14, 8, 7, 12 };
            bool[] dir = { false, false, true, true, true };

            TabelaBorda('┌', '┬', '┐', larg);
            TabelaLinha(
                new string[]
                {
                    "Nome", "Marca", "Volume", "Estoque", "Preço"
                },
                larg, dir, ConsoleColor.Yellow
            );
            TabelaBorda('├', '┼', '┤', larg);

            foreach (Bebida bebida in bebidas)
            {
                TabelaLinha(
                    new string[]
                    {
                        bebida.Nome,
                        bebida.Marca,
                        bebida.VolumeMl + " ml",
                        bebida.Quantidade.ToString(),
                        bebida.Preco.ToString("C2")
                    },
                    larg, dir, CorEstoque(bebida.Quantidade)
                );
            }

            TabelaBorda('└', '┴', '┘', larg);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Legenda: ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("esgotado");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  |  ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("estoque baixo (até 5 un.)");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  |  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("normal");
        Console.ResetColor();
    }

    static void ImprimirRelatorio()
    {
        Console.Clear();
        Titulo("RELATÓRIO DE VENDAS");

        decimal totalPaes;
        decimal totalDoces;
        decimal totalBebidas;
        decimal totalGeral;

        CalcularTotaisDeVenda(
            out totalPaes,
            out totalDoces,
            out totalBebidas,
            out totalGeral
        );

        Secao("VENDAS REGISTRADAS");

        if (vendas.Count == 0)
        {
            Vazio("Nenhuma venda registrada.");
        }
        else
        {
            int[] larg = { 16, 9, 16, 5, 12, 15 };
            bool[] dir = { false, false, false, true, true, true };

            TabelaBorda('┌', '┬', '┐', larg);
            TabelaLinha(
                new string[]
                {
                    "Data/Hora", "Categoria", "Produto",
                    "Qtd.", "Unitário", "Total"
                },
                larg, dir, ConsoleColor.Yellow
            );
            TabelaBorda('├', '┼', '┤', larg);

            foreach (Venda venda in vendas)
            {
                TabelaLinha(
                    new string[]
                    {
                        venda.DataHora.ToString("dd/MM/yyyy HH:mm"),
                        venda.Categoria,
                        venda.NomeProduto,
                        venda.QuantidadeVendida.ToString(),
                        venda.PrecoUnitario.ToString("C2"),
                        venda.Total.ToString("C2")
                    },
                    larg, dir, ConsoleColor.White
                );
            }

            TabelaBorda('└', '┴', '┘', larg);
        }

        //  RESUMO DOS TOTAIS 

        Secao("RESUMO POR CATEGORIA");

        int interno = Largura - 2;

        Linha("┌" + new string('─', interno) + "┐", ConsoleColor.Cyan);

        LinhaTotal("Total vendido em pães:", totalPaes, ConsoleColor.White);
        LinhaTotal(
            "Total vendido em doces/bolos:",
            totalDoces,
            ConsoleColor.White
        );
        LinhaTotal(
            "Total vendido em bebidas:",
            totalBebidas,
            ConsoleColor.White
        );

        Linha("├" + new string('─', interno) + "┤", ConsoleColor.Cyan);

        LinhaTotal("TOTAL GERAL:", totalGeral, ConsoleColor.Green);

        Linha("└" + new string('─', interno) + "┘", ConsoleColor.Cyan);
    }

    
    // CAMADA DE NEGÓCIOS: CADASTRO, CONSULTA, EXCLUSÃO E VENDAS
    

    static void CadastrarPao()
    {
        Console.Clear();
        Titulo("CADASTRO DE PÃO");
        Dica("Preencha os dados abaixo. Não use ponto e vírgula (;).");

        string nome = LerTexto("Nome do pão: ");

        if (PaoExiste(nome))
        {
            Erro("Já existe um pão com esse nome.");
            Pausar();
            return;
        }

        string tipo = LerTexto(
            "Tipo (francês, integral, doce etc.): "
        );

        int quantidade = LerInteiroNaoNegativo(
            "Quantidade inicial em estoque: "
        );

        decimal preco = LerPreco(
            "Preço unitário (ex.: 1,50): "
        );

        Pao novoPao = new Pao(nome, tipo, quantidade, preco);

        paes.Add(novoPao);

        if (SalvarPaes())
        {
            Mensagem(
                "Pão cadastrado com sucesso!",
                ConsoleColor.Green
            );

            Console.WriteLine("    " + novoPao.ToString());
        }

        Pausar();
    }

    static void CadastrarDoce()
    {
        Console.Clear();
        Titulo("CADASTRO DE DOCE/BOLO");
        Dica("Preencha os dados abaixo. Não use ponto e vírgula (;).");

        string nome = LerTexto("Nome do doce/bolo: ");

        if (DoceExiste(nome))
        {
            Erro("Já existe um doce/bolo com esse nome.");
            Pausar();
            return;
        }

        string sabor = LerTexto("Sabor: ");

        int quantidade = LerInteiroNaoNegativo(
            "Quantidade inicial em estoque: "
        );

        decimal preco = LerPreco(
            "Preço unitário (ex.: 5,50): "
        );

        Doce novoDoce = new Doce(nome, sabor, quantidade, preco);

        doces.Add(novoDoce);

        if (SalvarDoces())
        {
            Mensagem(
                "Doce/bolo cadastrado com sucesso!",
                ConsoleColor.Green
            );

            Console.WriteLine("    " + novoDoce.ToString());
        }

        Pausar();
    }

    static void CadastrarBebida()
    {
        Console.Clear();
        Titulo("CADASTRO DE BEBIDA");
        Dica("Preencha os dados abaixo. Não use ponto e vírgula (;).");

        string nome = LerTexto("Nome da bebida: ");

        if (BebidaExiste(nome))
        {
            Erro("Já existe uma bebida com esse nome.");
            Pausar();
            return;
        }

        string marca = LerTexto("Marca: ");

        int volume = LerInteiroPositivo("Volume em ml: ");

        int quantidade = LerInteiroNaoNegativo(
            "Quantidade inicial em estoque: "
        );

        decimal preco = LerPreco(
            "Preço unitário (ex.: 4,50): "
        );

        Bebida novaBebida = new Bebida(nome, marca, volume, quantidade, preco);

        bebidas.Add(novaBebida);

        if (SalvarBebidas())
        {
            Mensagem(
                "Bebida cadastrada com sucesso!",
                ConsoleColor.Green
            );

            Console.WriteLine("    " + novaBebida.ToString());
        }

        Pausar();
    }

    static bool PaoExiste(string nome)
    {
        foreach (Pao pao in paes)
        {
            if (pao.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static bool DoceExiste(string nome)
    {
        foreach (Doce doce in doces)
        {
            if (doce.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static bool BebidaExiste(string nome)
    {
        foreach (Bebida bebida in bebidas)
        {
            if (bebida.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static bool ConsultarPao(string nome)
    {
        foreach (Pao pao in paes)
        {
            if (pao.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine();
                Cartao(
                    "PRODUTO ENCONTRADO",
                    new string[] { "Nome", "Tipo", "Estoque", "Preço" },
                    new string[]
                    {
                        pao.Nome,
                        pao.Tipo,
                        pao.Quantidade + " un.",
                        pao.Preco.ToString("C2")
                    }
                );

                return true;
            }
        }

        Erro("Pão não encontrado.");
        return false;
    }

    static bool ConsultarDoce(string nome)
    {
        foreach (Doce doce in doces)
        {
            if (doce.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine();
                Cartao(
                    "PRODUTO ENCONTRADO",
                    new string[] { "Nome", "Sabor", "Estoque", "Preço" },
                    new string[]
                    {
                        doce.Nome,
                        doce.Sabor,
                        doce.Quantidade + " un.",
                        doce.Preco.ToString("C2")
                    }
                );

                return true;
            }
        }

        Erro("Doce/bolo não encontrado.");
        return false;
    }

    static bool ConsultarBebida(string nome)
    {
        foreach (Bebida bebida in bebidas)
        {
            if (bebida.Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine();
                Cartao(
                    "PRODUTO ENCONTRADO",
                    new string[]
                    {
                        "Nome", "Marca", "Volume", "Estoque", "Preço"
                    },
                    new string[]
                    {
                        bebida.Nome,
                        bebida.Marca,
                        bebida.VolumeMl + " ml",
                        bebida.Quantidade + " un.",
                        bebida.Preco.ToString("C2")
                    }
                );

                return true;
            }
        }

        Erro("Bebida não encontrada.");
        return false;
    }

    static bool ExcluirPao(string nome)
    {
        for (int i = 0; i < paes.Count; i++)
        {
            if (paes[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                paes.RemoveAt(i);

                return SalvarPaes();
            }
        }

        Erro("Pão não encontrado.");
        return false;
    }

    static bool ExcluirDoce(string nome)
    {
        for (int i = 0; i < doces.Count; i++)
        {
            if (doces[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                doces.RemoveAt(i);

                return SalvarDoces();
            }
        }

        Erro("Doce/bolo não encontrado.");
        return false;
    }

    static bool ExcluirBebida(string nome)
    {
        for (int i = 0; i < bebidas.Count; i++)
        {
            if (bebidas[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                bebidas.RemoveAt(i);

                return SalvarBebidas();
            }
        }

        Erro("Bebida não encontrada.");
        return false;
    }

    static void VenderPao(string nome, int quantidade)
    {
        for (int i = 0; i < paes.Count; i++)
        {
            if (paes[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Pao pao = paes[i];

                if (quantidade > pao.Quantidade)
                {
                    Erro(
                        $"Estoque insuficiente. Disponível: {pao.Quantidade}."
                    );

                    return;
                }

                decimal total = pao.Preco * quantidade;

                pao.Quantidade -= quantidade;
                paes[i] = pao;

                RegistrarVenda(
                    pao.Nome,
                    "Pão",
                    quantidade,
                    pao.Preco,
                    total
                );

                bool salvouEstoque = SalvarPaes();
                bool salvouVendas = SalvarVendas();

                ConfirmarVenda(salvouEstoque && salvouVendas, total);

                return;
            }
        }

        Erro("Pão não encontrado.");
    }

    static void VenderDoce(string nome, int quantidade)
    {
        for (int i = 0; i < doces.Count; i++)
        {
            if (doces[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Doce doce = doces[i];

                if (quantidade > doce.Quantidade)
                {
                    Erro(
                        $"Estoque insuficiente. Disponível: {doce.Quantidade}."
                    );

                    return;
                }

                decimal total = doce.Preco * quantidade;

                doce.Quantidade -= quantidade;
                doces[i] = doce;

                RegistrarVenda(
                    doce.Nome,
                    "Doce/Bolo",
                    quantidade,
                    doce.Preco,
                    total
                );

                bool salvouEstoque = SalvarDoces();
                bool salvouVendas = SalvarVendas();

                ConfirmarVenda(salvouEstoque && salvouVendas, total);

                return;
            }
        }

        Erro("Doce/bolo não encontrado.");
    }

    static void VenderBebida(string nome, int quantidade)
    {
        for (int i = 0; i < bebidas.Count; i++)
        {
            if (bebidas[i].Nome.Equals(
                nome,
                StringComparison.OrdinalIgnoreCase))
            {
                Bebida bebida = bebidas[i];

                if (quantidade > bebida.Quantidade)
                {
                    Erro(
                        $"Estoque insuficiente. Disponível: {bebida.Quantidade}."
                    );

                    return;
                }

                decimal total = bebida.Preco * quantidade;

                bebida.Quantidade -= quantidade;
                bebidas[i] = bebida;

                RegistrarVenda(
                    bebida.Nome,
                    "Bebida",
                    quantidade,
                    bebida.Preco,
                    total
                );

                bool salvouEstoque = SalvarBebidas();
                bool salvouVendas = SalvarVendas();

                ConfirmarVenda(salvouEstoque && salvouVendas, total);

                return;
            }
        }

        Erro("Bebida não encontrada.");
    }

    static void CalcularTotaisDeVenda(
        out decimal totalPaes,
        out decimal totalDoces,
        out decimal totalBebidas,
        out decimal totalGeral)
    {
        totalPaes = 0;
        totalDoces = 0;
        totalBebidas = 0;

        foreach (Venda venda in vendas)
        {
            if (venda.Categoria == "Pão")
            {
                totalPaes += venda.Total;
            }
            else if (venda.Categoria == "Doce/Bolo")
            {
                totalDoces += venda.Total;
            }
            else if (venda.Categoria == "Bebida")
            {
                totalBebidas += venda.Total;
            }
        }

        totalGeral = totalPaes + totalDoces + totalBebidas;
    }

    static void ConfirmarVenda(bool salvou, decimal total)
    {
        if (salvou)
        {
            Mensagem(
                $"Venda realizada! Total: {total:C2}",
                ConsoleColor.Green
            );
        }
        else
        {
            Mensagem(
                $"Venda feita, mas não gravada no arquivo. Total: {total:C2}",
                ConsoleColor.Yellow
            );
        }

        Console.WriteLine("    " + vendas[vendas.Count - 1].ToString());
    }

    static void RegistrarVenda(
        string nome,
        string categoria,
        int quantidade,
        decimal preco,
        decimal total)
    {
        Venda venda = new Venda(
            nome,
            categoria,
            quantidade,
            preco,
            total,
            DateTime.Now
        );

        vendas.Add(venda);
    }

    
    // CAMADA DE PERSISTÊNCIA: ARQUIVOS TXT

    static string CaminhoArquivo(string nomeArquivo)
    {
        return Path.Combine(pastaDados, nomeArquivo);
    }

    static string LimparCampo(string texto)
    {
        return texto
            .Replace(";", ",")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    static bool GravarArquivo(string nomeArquivo, List<string> linhas)
    {
        try
        {
            File.WriteAllLines(
                CaminhoArquivo(nomeArquivo),
                linhas
            );

            return true;
        }
        catch (Exception)
        {
            Erro(
                "Não foi possível salvar " + nomeArquivo + ". " +
                "Verifique as permissões da pasta ou se o arquivo está aberto."
            );

            return false;
        }
    }

    static bool SalvarPaes()
    {
        List<string> linhas = new List<string>();

        foreach (Pao pao in paes)
        {
            string linha =
                LimparCampo(pao.Nome) + ";" +
                LimparCampo(pao.Tipo) + ";" +
                pao.Quantidade + ";" +
                pao.Preco.ToString(CultureInfo.InvariantCulture);

            linhas.Add(linha);
        }

        return GravarArquivo("paes.txt", linhas);
    }

    static bool SalvarDoces()
    {
        List<string> linhas = new List<string>();

        foreach (Doce doce in doces)
        {
            string linha =
                LimparCampo(doce.Nome) + ";" +
                LimparCampo(doce.Sabor) + ";" +
                doce.Quantidade + ";" +
                doce.Preco.ToString(CultureInfo.InvariantCulture);

            linhas.Add(linha);
        }

        return GravarArquivo("doces.txt", linhas);
    }

    static bool SalvarBebidas()
    {
        List<string> linhas = new List<string>();

        foreach (Bebida bebida in bebidas)
        {
            string linha =
                LimparCampo(bebida.Nome) + ";" +
                LimparCampo(bebida.Marca) + ";" +
                bebida.VolumeMl + ";" +
                bebida.Quantidade + ";" +
                bebida.Preco.ToString(CultureInfo.InvariantCulture);

            linhas.Add(linha);
        }

        return GravarArquivo("bebidas.txt", linhas);
    }

    static bool SalvarVendas()
    {
        List<string> linhas = new List<string>();

        foreach (Venda venda in vendas)
        {
            string linha =
                LimparCampo(venda.NomeProduto) + ";" +
                LimparCampo(venda.Categoria) + ";" +
                venda.QuantidadeVendida + ";" +
                venda.PrecoUnitario.ToString(
                    CultureInfo.InvariantCulture) + ";" +
                venda.Total.ToString(
                    CultureInfo.InvariantCulture) + ";" +
                venda.DataHora.ToString(
                    "O",
                    CultureInfo.InvariantCulture);

            linhas.Add(linha);
        }

        return GravarArquivo("vendas.txt", linhas);
    }

    static List<Pao> CarregarPaes()
    {
        List<Pao> lista = new List<Pao>();

        string arquivo = CaminhoArquivo("paes.txt");

        if (!File.Exists(arquivo))
        {
            return lista;
        }

        foreach (string linha in File.ReadAllLines(arquivo))
        {
            if (string.IsNullOrWhiteSpace(linha))
            {
                continue;
            }

            string[] campos = linha.Split(';');

            int quantidade;
            decimal preco;

            if (campos.Length == 4 &&
                int.TryParse(campos[2], out quantidade) &&
                quantidade >= 0 &&
                decimal.TryParse(
                    campos[3],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out preco) &&
                PrecoValido(preco))
            {
                lista.Add(new Pao(campos[0], campos[1], quantidade, preco));
            }
            else
            {
                linhasIgnoradas.Add("paes.txt: " + linha);
            }
        }

        return lista;
    }

    static List<Doce> CarregarDoces()
    {
        List<Doce> lista = new List<Doce>();

        string arquivo = CaminhoArquivo("doces.txt");

        if (!File.Exists(arquivo))
        {
            return lista;
        }

        foreach (string linha in File.ReadAllLines(arquivo))
        {
            if (string.IsNullOrWhiteSpace(linha))
            {
                continue;
            }

            string[] campos = linha.Split(';');

            int quantidade;
            decimal preco;

            if (campos.Length == 4 &&
                int.TryParse(campos[2], out quantidade) &&
                quantidade >= 0 &&
                decimal.TryParse(
                    campos[3],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out preco) &&
                PrecoValido(preco))
            {
                lista.Add(new Doce(campos[0], campos[1], quantidade, preco));
            }
            else
            {
                linhasIgnoradas.Add("doces.txt: " + linha);
            }
        }

        return lista;
    }

    static List<Bebida> CarregarBebidas()
    {
        List<Bebida> lista = new List<Bebida>();

        string arquivo = CaminhoArquivo("bebidas.txt");

        if (!File.Exists(arquivo))
        {
            return lista;
        }

        foreach (string linha in File.ReadAllLines(arquivo))
        {
            if (string.IsNullOrWhiteSpace(linha))
            {
                continue;
            }

            string[] campos = linha.Split(';');

            int volume;
            int quantidade;
            decimal preco;

            if (campos.Length == 5 &&
                int.TryParse(campos[2], out volume) &&
                volume > 0 &&
                int.TryParse(campos[3], out quantidade) &&
                quantidade >= 0 &&
                decimal.TryParse(
                    campos[4],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out preco) &&
                PrecoValido(preco))
            {
                lista.Add(new Bebida(
                    campos[0],
                    campos[1],
                    volume,
                    quantidade,
                    preco
                ));
            }
            else
            {
                linhasIgnoradas.Add("bebidas.txt: " + linha);
            }
        }

        return lista;
    }

    static List<Venda> CarregarVendas()
    {
        List<Venda> lista = new List<Venda>();

        string arquivo = CaminhoArquivo("vendas.txt");

        if (!File.Exists(arquivo))
        {
            return lista;
        }

        foreach (string linha in File.ReadAllLines(arquivo))
        {
            if (string.IsNullOrWhiteSpace(linha))
            {
                continue;
            }

            string[] campos = linha.Split(';');

            int quantidade;
            decimal preco;
            decimal total;
            DateTime data;

            if (campos.Length == 6 &&
                int.TryParse(campos[2], out quantidade) &&
                quantidade > 0 &&
                decimal.TryParse(
                    campos[3],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out preco) &&
                PrecoValido(preco) &&
                decimal.TryParse(
                    campos[4],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out total) &&
                total >= 0 &&
                DateTime.TryParse(
                    campos[5],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out data))
            {
                lista.Add(new Venda(
                    campos[0],
                    campos[1],
                    quantidade,
                    preco,
                    total,
                    data
                ));
            }
            else
            {
                linhasIgnoradas.Add("vendas.txt: " + linha);
            }
        }

        return lista;
    }

    static bool SalvarTudo()
    {
        bool salvouPaes = SalvarPaes();
        bool salvouDoces = SalvarDoces();
        bool salvouBebidas = SalvarBebidas();
        bool salvouVendas = SalvarVendas();

        return salvouPaes && salvouDoces && salvouBebidas && salvouVendas;
    }

    static bool PrecoValido(decimal preco)
    {
        return preco >= 0 && preco <= PrecoMaximo;
    }

    static void GuardarLinhasIgnoradas()
    {
        if (linhasIgnoradas.Count > 0)
        {
            File.AppendAllLines(
                CaminhoArquivo("linhas_ignoradas.txt"),
                linhasIgnoradas
            );
        }
    }

  
    // FUNÇÕES AUXILIARES DE ENTRADA

    static string LerTexto(string mensagem)
    {
        while (true)
        {
            Prompt(mensagem);

            string texto = (Console.ReadLine() ?? "").Trim();

            if (texto != "" && !texto.Contains(";"))
            {
                return texto;
            }

            Erro(
                "Digite um texto válido e não use ponto e vírgula (;)."
            );
        }
    }

    static int LerInteiroNaoNegativo(string mensagem)
    {
        while (true)
        {
            Prompt(mensagem);

            int valor;

            if (int.TryParse(Console.ReadLine(), out valor) &&
                valor >= 0)
            {
                return valor;
            }

            Erro(
                "Digite um número inteiro igual ou maior que zero."
            );
        }
    }

    static int LerInteiroPositivo(string mensagem)
    {
        while (true)
        {
            Prompt(mensagem);

            int valor;

            if (int.TryParse(Console.ReadLine(), out valor) &&
                valor > 0)
            {
                return valor;
            }

            Erro("Digite um número inteiro maior que zero.");
        }
    }

    static decimal LerPreco(string mensagem)
    {
        while (true)
        {
            Prompt(mensagem);

            string entrada = (Console.ReadLine() ?? "")
                .Trim()
                .Replace(',', '.');

            decimal valor;

            int ponto = entrada.IndexOf('.');
            bool casasValidas = ponto < 0 || entrada.Length - ponto - 1 <= 2;

            if (casasValidas &&
                decimal.TryParse(
                    entrada,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out valor) &&
                valor > 0 &&
                valor <= PrecoMaximo)
            {
                return valor;
            }

            Erro(
                "Digite um preço maior que zero, com até 2 casas decimais " +
                $"e no máximo {PrecoMaximo:C2}."
            );
        }
    }

    
    // FUNÇÕES AUXILIARES DE INTERFACE (CORES, BORDAS E TABELAS)

    // ---------- Texto e alinhamento ----------

    static string Centralizar(string texto, int largura)
    {
        if (texto.Length >= largura)
        {
            return texto.Substring(0, largura);
        }

        int esquerda = (largura - texto.Length) / 2;
        int direita = largura - texto.Length - esquerda;

        return new string(' ', esquerda) + texto + new string(' ', direita);
    }

    static string Ajustar(string texto, int largura, bool alinharDireita)
    {
        if (texto.Length > largura)
        {
            texto = texto.Substring(0, largura - 1) + "…";
        }

        if (alinharDireita)
        {
            return texto.PadLeft(largura);
        }

        return texto.PadRight(largura);
    }

    static void Linha(string texto, ConsoleColor cor)
    {
        Console.ForegroundColor = cor;
        Console.WriteLine(texto);
        Console.ResetColor();
    }

    static void LinhaCaixa(
        char borda,
        string conteudo,
        ConsoleColor corBorda,
        ConsoleColor corTexto)
    {
        Console.ForegroundColor = corBorda;
        Console.Write(borda);

        Console.ForegroundColor = corTexto;
        Console.Write(conteudo);

        Console.ForegroundColor = corBorda;
        Console.WriteLine(borda);

        Console.ResetColor();
    }

    // ---------- Cabeçalho e menus ----------

    static void Titulo(string texto, string subtitulo = "")
    {
        int interno = Largura - 2;

        Linha("╔" + new string('═', interno) + "╗", ConsoleColor.Yellow);

        LinhaCaixa(
            '║',
            Centralizar(texto, interno),
            ConsoleColor.Yellow,
            ConsoleColor.White
        );

        if (subtitulo != "")
        {
            LinhaCaixa(
                '║',
                Centralizar(subtitulo, interno),
                ConsoleColor.Yellow,
                ConsoleColor.Gray
            );
        }

        Linha("╚" + new string('═', interno) + "╝", ConsoleColor.Yellow);

        Console.WriteLine();
    }

   
    static void MostrarMenu(string titulo, params string[] itens)
    {
        int interno = Largura - 2;

        Linha("┌" + new string('─', interno) + "┐", ConsoleColor.DarkYellow);

        LinhaCaixa(
            '│',
            Centralizar(titulo, interno),
            ConsoleColor.DarkYellow,
            ConsoleColor.White
        );

        Linha("├" + new string('─', interno) + "┤", ConsoleColor.DarkYellow);

        foreach (string item in itens)
        {
            string[] partes = item.Split('|');

            string chave = ("[" + partes[0] + "]").PadRight(5);
            string texto = Ajustar(partes[1], interno - 9, false);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("│");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  " + chave);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  " + texto);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("│");

            Console.ResetColor();
        }

        Linha("└" + new string('─', interno) + "┘", ConsoleColor.DarkYellow);
    }

    static void Secao(string texto)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ■ " + texto);
        Console.ResetColor();
    }

    static void Vazio(string texto)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    " + texto);
        Console.ResetColor();
    }

    static void Dica(string texto)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  » " + texto);
        Console.ResetColor();
        Console.WriteLine();
    }

    // ---------- Entrada de dados ----------

    static void Prompt(string texto)
    {
        Prompt(texto, ConsoleColor.Cyan);
    }

    static void Prompt(string texto, ConsoleColor cor)
    {
        Console.ForegroundColor = cor;
        Console.Write("  ► ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(texto);

        Console.ResetColor();
    }

    // ---------- Mensagens de feedback ----------

    static void Mensagem(string texto, ConsoleColor cor)
    {
        string etiqueta = "INFO";

        if (cor == ConsoleColor.Green)
        {
            etiqueta = "SUCESSO";
        }
        else if (cor == ConsoleColor.Red)
        {
            etiqueta = "ERRO";
        }
        else if (cor == ConsoleColor.Yellow)
        {
            etiqueta = "ATENÇÃO";
        }

        Console.WriteLine();

        Console.ForegroundColor = cor;
        Console.Write("  ▌ [" + etiqueta + "] ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(texto);

        Console.ResetColor();
    }

    static void Erro(string texto)
    {
        Mensagem(texto, ConsoleColor.Red);
    }

    static void Pausar()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  ─ Pressione ENTER para continuar... ─");
        Console.ResetColor();
        Console.ReadLine();
    }

    // ---------- Tabelas ----------

    static ConsoleColor CorEstoque(int quantidade)
    {
        if (quantidade == 0)
        {
            return ConsoleColor.Red;
        }

        if (quantidade <= 5)
        {
            return ConsoleColor.Yellow;
        }

        return ConsoleColor.White;
    }

    static void TabelaBorda(
        char esquerda,
        char meio,
        char direita,
        int[] larguras)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(esquerda);

        for (int i = 0; i < larguras.Length; i++)
        {
            sb.Append(new string('─', larguras[i] + 2));

            if (i < larguras.Length - 1)
            {
                sb.Append(meio);
            }
        }

        sb.Append(direita);

        Linha("  " + sb.ToString(), ConsoleColor.DarkGray);
    }

    static void TabelaLinha(
        string[] celulas,
        int[] larguras,
        bool[] alinharDireita,
        ConsoleColor cor)
    {
        Console.Write("  ");

        for (int i = 0; i < celulas.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│");

            Console.ForegroundColor = cor;
            Console.Write(
                " " +
                Ajustar(celulas[i], larguras[i], alinharDireita[i]) +
                " "
            );
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("│");

        Console.ResetColor();
    }

    // ---------- Cartão de consulta e linha de total ----------

    static void Cartao(string titulo, string[] rotulos, string[] valores)
    {
        int interno = Largura - 2;

        Linha("┌" + new string('─', interno) + "┐", ConsoleColor.Green);

        LinhaCaixa(
            '│',
            Centralizar(titulo, interno),
            ConsoleColor.Green,
            ConsoleColor.Green
        );

        Linha("├" + new string('─', interno) + "┤", ConsoleColor.Green);

        for (int i = 0; i < rotulos.Length; i++)
        {
            string conteudo = Ajustar(
                "  " + rotulos[i].PadRight(10) + ": " + valores[i],
                interno,
                false
            );

            LinhaCaixa(
                '│',
                conteudo,
                ConsoleColor.Green,
                ConsoleColor.White
            );
        }

        Linha("└" + new string('─', interno) + "┘", ConsoleColor.Green);
    }

    static void LinhaTotal(string rotulo, decimal valor, ConsoleColor cor)
    {
        int interno = Largura - 2;

        string textoValor = valor.ToString("C2");

        string conteudo =
            " " +
            rotulo.PadRight(interno - 2 - textoValor.Length) +
            textoValor +
            " ";

        LinhaCaixa('│', conteudo, ConsoleColor.Cyan, cor);
    }

    
    // CAMADA DE DADOS: STRUCTS


    struct Pao
    {
        public string Nome;
        public string Tipo;
        public int Quantidade;
        public decimal Preco;

        public Pao(string nome, string tipo, int quantidade, decimal preco)
        {
            Nome = nome;
            Tipo = tipo;
            Quantidade = quantidade;
            Preco = preco;
        }

        public override string ToString()
        {
            return $"Nome: {Nome} | Tipo: {Tipo} | " +
                   $"Estoque: {Quantidade} | Preço: {Preco:C2}";
        }
    }

    struct Doce
    {
        public string Nome;
        public string Sabor;
        public int Quantidade;
        public decimal Preco;

        public Doce(string nome, string sabor, int quantidade, decimal preco)
        {
            Nome = nome;
            Sabor = sabor;
            Quantidade = quantidade;
            Preco = preco;
        }

        public override string ToString()
        {
            return $"Nome: {Nome} | Sabor: {Sabor} | " +
                   $"Estoque: {Quantidade} | Preço: {Preco:C2}";
        }
    }

    struct Bebida
    {
        public string Nome;
        public string Marca;
        public int VolumeMl;
        public int Quantidade;
        public decimal Preco;

        public Bebida(
            string nome,
            string marca,
            int volumeMl,
            int quantidade,
            decimal preco)
        {
            Nome = nome;
            Marca = marca;
            VolumeMl = volumeMl;
            Quantidade = quantidade;
            Preco = preco;
        }

        public override string ToString()
        {
            return $"Nome: {Nome} | Marca: {Marca} | " +
                   $"Volume: {VolumeMl} ml | Estoque: {Quantidade} | " +
                   $"Preço: {Preco:C2}";
        }
    }

    struct Venda
    {
        public string NomeProduto;
        public string Categoria;
        public int QuantidadeVendida;
        public decimal PrecoUnitario;
        public decimal Total;
        public DateTime DataHora;

        public Venda(
            string nomeProduto,
            string categoria,
            int quantidadeVendida,
            decimal precoUnitario,
            decimal total,
            DateTime dataHora)
        {
            NomeProduto = nomeProduto;
            Categoria = categoria;
            QuantidadeVendida = quantidadeVendida;
            PrecoUnitario = precoUnitario;
            Total = total;
            DataHora = dataHora;
        }

        public override string ToString()
        {
            return $"{DataHora:dd/MM/yyyy HH:mm} | {Categoria} | " +
                   $"{NomeProduto} | Qtd.: {QuantidadeVendida} | " +
                   $"Unitário: {PrecoUnitario:C2} | Total: {Total:C2}";
        }
    }
}