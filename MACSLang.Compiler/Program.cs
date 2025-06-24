// MACSLang.Compiler/Program.cs

using System;
using System.IO; // Para operações de arquivo
using System.Diagnostics; // Para iniciar processos (compilar e rodar o C# gerado)
using MACSLang.Lexer;
using MACSLang.Parser;
using MACSLang.Syntax.AST;
using MACSLang.SemanticAnalyzer;
using MACSLang.CodeGenerator; // Usar o Gerador de Código

namespace MACSLang.Compiler;

internal class Program
{
    // O código fonte correto da MACSLang (agora com uma função main)
    private static readonly string _correctSourceCode = @"
            // Este é um programa de exemplo da MACSLang
            func fatorial(n: int): int {
                var resultado: int = 1;
                for (var i: int = 1; i <= n; i = i + 1) {
                    resultado = resultado * i;
                }
                return resultado;
            }

            func main(): int { // Adicionamos uma função principal 'main'
                print(""Digite um número para calcular o fatorial:"");
                var numero: int;
                input(numero);

                var fat: int = fatorial(numero);
                print(""O fatorial de "" + numero + "" é "" + fat);

                /*
                 * Este é um comentário de bloco.
                 * Ele pode ter múltiplas linhas.
                 */
                var myFloat: float = 123.45; 
                var myChar: char = 'X';
                var myBool: bool = true;
                if (myBool == false) {
                    // Faz algo
                } else {
                    var temp: int = 0; // Exemplo dentro do else
                }
                while (myBool) { // Exemplo de while
                    myBool = false;
                }
                // Testando expressões diversas
                var exprResult: bool = (5 + 3 * 2) == 11 && !false || true;
                return 0; // Função main retorna int
            }
        ";

    // Código fonte com um erro sintático proposital
    private static readonly string _syntaxErrorSourceCode = @"
            // Este é um programa com erro sintático
            func erroDeSintaxe(): int {
                var x: int = ; // ERRO: Faltando valor após o '='
                print(""Esta linha não deve ser alcançada."");
                return 0;
            }
        ";

    // Código fonte com erros semânticos propositais
    private static readonly string _semanticErrorSourceCode = @"
            // Este é um programa com erros semânticos
            func testSemantico(): int {
                var a: int = 10;
                var b: float = 5.5; 
                var c: bool = true;
                
                // Erro 1: Variável não declarada
                undeclaredVar = 20;

                // Erro 2: Atribuição de tipo incompatível
                a = b; // float para int (conversão implícita não permitida MACSLang, apenas int para float)

                // Erro 3: Operação aritmética com booleanos
                var result: int = a + c; 

                // Erro 4: Chamada de função com número incorreto de argumentos
                fatorial(a, b); // fatorial espera 1 int, recebendo 2

                // Erro 5: Chamada de função com tipo de argumento incompatível
                fatorial(c); // fatorial espera int, recebendo bool

                // Erro 6: Return de tipo incorreto
                return b; // função testSemantico retorna int, mas tentando retornar float
            }

            // CORRIGIDO: Adicionado tipo de retorno para ser sintaticamente válido.
            func outraFuncao(): int { 
                return 0;
            }

            func duplicada(): int { return 1; }
            func duplicada(): int { return 2; } // Erro 8: Função duplicada
        ";


    private static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo ao Compilador MACSLang!");
        var running = true;

        while (running)
        {
            Console.WriteLine("\n--------------------------------");
            Console.WriteLine("Escolha a fase do compilador para testar:");
            Console.WriteLine("1. Analisador Léxico (Lexer)");
            Console.WriteLine("2. Analisador Sintático (Parser)");
            Console.WriteLine("3. Analisador Semântico");
            Console.WriteLine("4. Gerador de Código"); // Atualizado
            Console.WriteLine("0. Sair");
            Console.Write("Sua escolha: ");

            var choice = Console.ReadLine() ?? "";
            Console.WriteLine("\n--------------------------------");

            switch (choice)
            {
                case "1":
                    RunLexerTest(_correctSourceCode);
                    break;
                case "2":
                    Console.WriteLine("Escolha o código para testar o Parser:");
                    Console.WriteLine("1. Código Correto");
                    Console.WriteLine("2. Código com Erro Sintático");
                    Console.Write("Sua escolha (1 ou 2): ");
                    var parserChoice = Console.ReadLine() ?? "";
                    if (parserChoice == "1")
                        RunParserTest(_correctSourceCode);
                    else if (parserChoice == "2")
                        RunParserTest(_syntaxErrorSourceCode);
                    else
                        Console.WriteLine("Opção inválida para teste do Parser.");
                    break;
                case "3":
                    Console.WriteLine("Escolha o código para testar o Analisador Semântico:");
                    Console.WriteLine("1. Código Correto");
                    Console.WriteLine("2. Código com Erros Semânticos");
                    Console.Write("Sua escolha (1 ou 2): ");
                    var semanticChoice = Console.ReadLine() ?? "";
                    if (semanticChoice == "1")
                        RunSemanticAnalyzerTest(_correctSourceCode);
                    else if (semanticChoice == "2")
                        RunSemanticAnalyzerTest(_semanticErrorSourceCode);
                    else
                        Console.WriteLine("Opção inválida para teste do Analisador Semântico.");
                    break;
                case "4":
                    RunCodeGeneratorTest(_correctSourceCode);
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("Saindo do compilador. Até mais!");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Por favor, digite um número de 0 a 4.");
                    break;
            }
        }
    }

    private static void RunLexerTest(string source)
    {
        Console.WriteLine("--- Teste do Analisador Léxico ---");
        try
        {
            var lexer = new MACSLang.Lexer.Lexer(source);
            Token token;
            Console.WriteLine("Tokens encontrados:");
            do
            {
                token = lexer.NextToken();
                Console.WriteLine(token.ToString());

                if (token.Type == TokenType.UNKNOWN)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"!!! ERRO LÉXICO: Caractere ou sequência desconhecida em Linha: {token.Line}, Coluna: {token.Column}");
                    Console.ResetColor();
                }
            } while (token.Type != TokenType.EOF);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nAnálise Léxica Concluída com SUCESSO!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! OCORREU UM ERRO INESPERADO DURANTE A ANÁLISE LÉXICA !!!");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    private static void RunParserTest(string source)
    {
        Console.WriteLine("--- Teste do Analisador Sintático ---");
        try
        {
            var lexer = new MACSLang.Lexer.Lexer(source);
            var parser = new MACSLang.Parser.Parser(lexer);

            Console.WriteLine("Iniciando análise sintática...");
            var ast = parser.ParseProgram();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nAnálise Sintática Concluída com SUCESSO!");
            Console.WriteLine("Árvore Sintática Abstrata (AST) construída.");
            Console.ResetColor();
        }
        catch (ParseException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ERRO DE SINTAXE !!!");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! OCORREU UM ERRO INESPERADO DURANTE A ANÁLISE SINTÁTICA !!!");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    private static void RunSemanticAnalyzerTest(string source)
    {
        Console.WriteLine("--- Teste do Analisador Semântico ---");
        try
        {
            // Primeiro, executa a análise léxica
            var lexer = new MACSLang.Lexer.Lexer(source);

            // Segundo, executa a análise sintática para construir a AST
            var parser = new MACSLang.Parser.Parser(lexer);
            Console.WriteLine("Construindo Árvore Sintática Abstrata (AST)...");
            var ast = parser.ParseProgram();

            // Terceiro, executa a análise semântica na AST
            Console.WriteLine("Iniciando análise semântica...");
            var semanticAnalyzer = new MACSLang.SemanticAnalyzer.SemanticAnalyzer();
            semanticAnalyzer.Analyze(ast);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nAnálise Semântica Concluída com SUCESSO! Código é semanticamente válido.");
            Console.ResetColor();
        }
        catch (ParseException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ERRO DE SINTAXE !!! (Análise Semântica não pode prosseguir)");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (SemanticError ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ERRO SEMÂNTICO !!!");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! OCORREU UM ERRO INESPERADO !!!");
            Console.WriteLine(ex.ToString());
            Console.ResetColor();
        }
    }

    // NOVO MÉTODO: Teste do Gerador de Código
    private static void RunCodeGeneratorTest(string source)
    {
        Console.WriteLine("--- Teste do Gerador de Código ---");
        try
        {
            // 1. Análise Léxica
            var lexer = new MACSLang.Lexer.Lexer(source);
            Console.WriteLine("Análise Léxica concluída.");

            // 2. Análise Sintática para construir a AST
            var parser = new MACSLang.Parser.Parser(lexer);
            Console.WriteLine("Construindo Árvore Sintática Abstrata (AST)...");
            var ast = parser.ParseProgram();
            Console.WriteLine("Análise Sintática concluída.");

            // 3. Análise Semântica (crucial para tipagem e escopo antes da geração de código)
            Console.WriteLine("Iniciando análise semântica...");
            var semanticAnalyzer = new MACSLang.SemanticAnalyzer.SemanticAnalyzer();
            semanticAnalyzer.Analyze(ast);
            Console.WriteLine("Análise Semântica concluída.");

            // 4. Geração de Código
            Console.WriteLine("Iniciando geração de código C#...");
            var codeGenerator = new MACSLang.CodeGenerator.CodeGenerator();
            var generatedCSharpCode = codeGenerator.Generate(ast);
            Console.WriteLine("Geração de código C# concluída.");

            // Salva o código C# gerado em um arquivo temporário no diretório do projeto temporário
            var tempProjectName = "MACSLangGeneratedApp";
            var tempProjectDir = Path.Combine(Directory.GetCurrentDirectory(), tempProjectName);
            var generatedCsFileName = "GeneratedProgram.cs";
            var generatedCsFilePath = Path.Combine(tempProjectDir, generatedCsFileName);
            var csharpProjectFilePath = Path.Combine(tempProjectDir, $"{tempProjectName}.csproj");

            // Limpa o diretório temporário antes de criar
            if (Directory.Exists(tempProjectDir)) Directory.Delete(tempProjectDir, true);
            Directory.CreateDirectory(tempProjectDir);

            // Escreve o código C# gerado
            File.WriteAllText(generatedCsFilePath, generatedCSharpCode);
            Console.WriteLine($"Código C# gerado e salvo em: {generatedCsFilePath}");
            Console.WriteLine("\n--- Código C# Gerado ---\n");
            Console.WriteLine(generatedCSharpCode);
            Console.WriteLine("\n------------------------\n");

            // Cria um arquivo .csproj mínimo para o código gerado
            // Corrigido: Removida a indentação à esquerda no string literal para evitar nós de texto inválidos
            var csharpProjectContent =
                @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""GeneratedProgram.cs"" />
  </ItemGroup>
</Project>"; // <- Ajustado aqui
            File.WriteAllText(csharpProjectFilePath, csharpProjectContent);
            Console.WriteLine($"Arquivo de projeto C# temporário criado em: {csharpProjectFilePath}");

            // 5. Compila e executa o código C# gerado
            Console.WriteLine("Tentando compilar e executar o código C# gerado...");

            // Comando para compilar o projeto C# temporário
            var psiBuild = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{tempProjectDir}\" -c Debug --nologo", // Compila o projeto temporário
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = tempProjectDir // Executa o dotnet build no diretório do projeto temporário
            };

            using (var process = Process.Start(psiBuild)!)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Compilação do código C# gerado com SUCESSO!");
                    Console.ResetColor();

                    // Determinar o caminho do executável gerado
                    var targetFrameworkForRun = "net6.0";
                    var executablePath = Path.Combine(tempProjectDir, "bin", "Debug", targetFrameworkForRun,
                        $"{tempProjectName}.dll");

                    Console.WriteLine("\n--- Saída do Programa C# Gerado ---");
                    // Comando para executar o assembly .NET gerado
                    var psiRun = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"exec \"{executablePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = tempProjectDir
                    };

                    using (var runProcess = Process.Start(psiRun)!)
                    {
                        var runOutput = runProcess.StandardOutput.ReadToEnd();
                        var runError = runProcess.StandardError.ReadToEnd();
                        runProcess.WaitForExit();
                        Console.WriteLine(runOutput);
                        if (!string.IsNullOrEmpty(runError))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Erro durante a execução: \n{runError}");
                            Console.ResetColor();
                        }
                    }

                    Console.WriteLine("----------------------------------\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FALHA na compilação do código C# gerado!");
                    Console.WriteLine("Saída do compilador .NET:\n" + output);
                    Console.WriteLine("Erros do compilador .NET:\n" + error);
                    Console.ResetColor();
                }
            }
        }
        catch (ParseException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ERRO DE SINTAXE !!! (Geração de Código não pode prosseguir)");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (SemanticError ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ERRO SEMÂNTICO !!! (Geração de Código não pode prosseguir)");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! OCORREU UM ERRO INESPERADO DURANTE A GERAÇÃO/EXECUÇÃO DO CÓDIGO !!!");
            Console.WriteLine(ex.ToString());
            Console.ResetColor();
        }
    }
}