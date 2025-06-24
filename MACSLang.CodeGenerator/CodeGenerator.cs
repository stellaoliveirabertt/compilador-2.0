// MACSLang.CodeGenerator/CodeGenerator.cs
using System;
using System.Collections.Generic;
using System.Text;
using MACSLang.Lexer; // Para TokenType
using MACSLang.Syntax.AST; // Para todos os nós da AST e IAstVisitor
using System.IO; // Adicionado para StreamWriter
using System.Globalization; // Adicionado para CultureInfo.InvariantCulture

namespace MACSLang.CodeGenerator
{
    // A classe CodeGenerator implementa IAstVisitor para percorrer a AST e gerar código C#
    public class CodeGenerator : IAstVisitor
    {
        private readonly StringBuilder _sb; // Para acumular o código gerado
        private int _indentationLevel;     // Nível de indentação atual
        private string _currentFunctionReturnType; // Para saber o tipo de retorno da função atual

        public CodeGenerator()
        {
            _sb = new StringBuilder();
            _indentationLevel = 0;
            _currentFunctionReturnType = "void"; // Valor padrão, ajustado ao entrar em func
        }

        // Método principal para iniciar a geração de código
        public string Generate(ProgramNode program)
        {
            // Começa com a estrutura básica de um programa C#
            EmitLine("using System;");
            EmitLine("using System.Linq;"); // Para algumas operações de string, se necessário
            EmitLine("using System.IO;"); // Adicionado para StreamWriter, se necessário
            EmitLine("using System.Globalization;"); // Adicionado para CultureInfo, se necessário
            EmitLine("");
            EmitLine("namespace MACSLangRuntime");
            EmitLine("{");
            Indent();
            EmitLine("public static class Program");
            EmitLine("{");
            Indent();

            // Visita o nó raiz do programa para iniciar a geração
            Visit(program);

            // Adiciona um ponto de entrada Main se não houver um explicitamente definido na MACSLang
            // Assumimos que 'main' é a função de entrada.
            // Se a MACSLang permitir código fora de funções, isso precisaria ser ajustado.
            // Por enquanto, o código gerado chama a função 'main' definida na MACSLang.
            if (!program.Functions.Exists(f => f.Identifier.Lexeme == "main"))
            {
                 // Isso pode significar que não há um ponto de entrada claro,
                 // ou que o código gerado precisa de um wrapper.
                 // Por enquanto, assumimos que 'main' existe no código MACSLang.
                 // Se não houver 'main', o compilador da MACSLang precisaria definir como executar.
                 // Para este compilador simples, vamos esperar uma função 'main'.
            }


            Dedent();
            EmitLine("}"); // Fim da classe Program
            Dedent();
            EmitLine("}"); // Fim do namespace

            return _sb.ToString();
        }

        // --- Métodos Auxiliares de Emissão de Código ---

        private void Emit(string code)
        {
            _sb.Append(code);
        }

        private void EmitLine(string code = "")
        {
            EmitIndent();
            _sb.AppendLine(code);
        }

        private void EmitIndent()
        {
            for (int i = 0; i < _indentationLevel; i++)
            {
                _sb.Append("    "); // 4 espaços por nível de indentação
            }
        }

        private void Indent()
        {
            _indentationLevel++;
        }

        private void Dedent()
        {
            _indentationLevel--;
        }

        // --- Implementação dos métodos Visit de IAstVisitor ---

        public void Visit(ProgramNode node)
        {
            foreach (var funcDecl in node.Functions)
            {
                funcDecl.Accept(this);
                EmitLine(); // Adiciona uma linha em branco entre funções para legibilidade
            }
        }

        public void Visit(FunctionDeclarationNode node)
        {
            _currentFunctionReturnType = ConvertMacsLangTypeToCSharp(node.ReturnType.TypeToken);
            string funcName = node.Identifier.Lexeme;

            // Tratamento especial para a função 'main' para que seja o ponto de entrada C#
            if (funcName == "main")
            {
                EmitLine($"public static {_currentFunctionReturnType} Main(string[] args)");
                EmitLine("{");
                Indent();
                // GARANTIA: Força o auto-flush do Console.Out e Flush explícito
                EmitLine("Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });"); 
                
                // Visita o corpo da função (dentro do Main)
                node.Body.Accept(this);

                Dedent();
                EmitLine("}");
            }
            else
            {
                EmitLine($"public static {_currentFunctionReturnType} {funcName}({FormatParameters(node.Parameters)})");
                node.Body.Accept(this); // Visita o corpo da função
            }
            
            _currentFunctionReturnType = "void"; // Reseta após sair da função
        }

        public void Visit(BlockStatementNode node)
        {
            // Se o bloco é o corpo de uma função 'main', as chaves já foram emitidas
            // na Visit(FunctionDeclarationNode)
            // A lógica de indentação aqui precisa ser mais robusta, pois está misturando o contexto do Main.
            // Para simplicidade, vamos tratar { e } explicitamente para todos os blocos,
            // e confiar na indentação para organizar.
            EmitLine("{"); // Sempre emite o '{' para um novo bloco
            Indent();
            
            foreach (var statement in node.Statements)
            {
                statement.Accept(this);
            }

            Dedent();
            EmitLine("}"); // Sempre emite o '}' para um bloco
        }


        public void Visit(VariableDeclarationNode node)
        {
            string cSharpType = ConvertMacsLangTypeToCSharp(node.Type.TypeToken);
            
            Emit($"{cSharpType} {node.Identifier.Lexeme}");

            if (node.InitialValue != null)
            {
                Emit(" = ");
                node.InitialValue.Accept(this); // Emit the initial value expression
            }
            else // Provide default value for C# if no initial value in MACSLang
            {
                switch (node.Type.TypeToken)
                {
                    case TokenType.INT_KEYWORD: Emit(" = 0"); break;
                    case TokenType.FLOAT_KEYWORD: Emit(" = 0.0f"); break;
                    case TokenType.CHAR_KEYWORD: Emit(" = '\\0'"); break;
                    case TokenType.BOOL_KEYWORD: Emit(" = false"); break;
                    case TokenType.STRING_KEYWORD: Emit(" = string.Empty"); break;
                    default: Emit(" = null"); break; // For other reference types (though MACSLang doesn't have custom classes yet)
                }
            }
            EmitLine(";");
        }

        public void Visit(AssignmentStatementNode node)
        {
            Emit($"{node.Identifier.Lexeme} = ");
            node.Value.Accept(this); // Emit the value expression
            EmitLine(";");
        }

        public void Visit(PrintStatementNode node)
        {
            Emit("Console.WriteLine(");
            node.Expression.Accept(this); // Emit the expression to be printed
            EmitLine(");");
        }

        public void Visit(InputStatementNode node)
        {
            // CORREÇÃO: Força o flush da saída ANTES de Console.ReadLine()
            EmitLine("Console.Out.Flush();"); // Garante que o prompt foi exibido

            string cSharpType = ConvertMacsLangTypeToCSharp(node.ResolvedTargetType); // Usa o tipo resolvido da AST

            // Gera o código de leitura com a conversão de tipo apropriada
            switch (node.ResolvedTargetType)
            {
                case TokenType.INT_KEYWORD:
                    Emit($"{node.TargetIdentifier.Lexeme} = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);"); // Use CultureInfo também para int.Parse
                    break;
                case TokenType.FLOAT_KEYWORD:
                    // Usa CultureInfo.InvariantCulture para garantir que o ponto decimal seja sempre '.'
                    Emit($"{node.TargetIdentifier.Lexeme} = float.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);");
                    break;
                case TokenType.BOOL_KEYWORD:
                    Emit($"{node.TargetIdentifier.Lexeme} = bool.Parse(Console.ReadLine()!);");
                    break;
                case TokenType.CHAR_KEYWORD:
                    Emit($"{node.TargetIdentifier.Lexeme} = Console.ReadLine()![0];"); // Pega o primeiro caractere da linha
                    break;
                case TokenType.STRING_KEYWORD:
                    Emit($"{node.TargetIdentifier.Lexeme} = Console.ReadLine();");
                    break;
                default:
                    // Caso para tipos não esperados ou complexos (não deve acontecer com análise semântica)
                    Emit($"{node.TargetIdentifier.Lexeme} = Console.ReadLine(); // Erro: Tipo desconhecido para input");
                    break;
            }
            EmitLine();
        }

        public void Visit(ReturnStatementNode node)
        {
            Emit("return ");
            node.Value.Accept(this); // Emite o return expression
            EmitLine(";");
        }

        public void Visit(IfStatementNode node)
        {
            Emit("if (");
            node.Condition.Accept(this); // Emite a condição
            Emit(") ");
            node.TrueBlock.Accept(this); // Emite o bloco 'if'

            if (node.ElseBlock != null)
            {
                Emit("else ");
                node.ElseBlock.Accept(this); // Emite o bloco 'else'
            }
        }

        public void Visit(WhileStatementNode node)
        {
            Emit("while (");
            node.Condition.Accept(this); // Emite a condição
            Emit(") ");
            node.Body.Accept(this); // Emite o corpo do loop
        }

        public void Visit(ForStatementNode node)
        {
            Emit("for (");
            // A inicialização pode ser uma declaração ou atribuição
            if (node.Initialization != null)
            {
                if (node.Initialization is VariableDeclarationNode varDecl)
                {
                    Emit($"{ConvertMacsLangTypeToCSharp(varDecl.Type.TypeToken)} {varDecl.Identifier.Lexeme} = ");
                    varDecl.InitialValue!.Accept(this); // Assume que InitialValue existe
                }
                else if (node.Initialization is AssignmentStatementNode assignStmt)
                {
                    Emit($"{assignStmt.Identifier.Lexeme} = ");
                    assignStmt.Value.Accept(this);
                }
                else if (node.Initialization is ExpressionStatementNode exprStmt)
                {
                    exprStmt.Expression.Accept(this);
                }
            }
            Emit("; "); // Separador da inicialização

            if (node.Condition != null)
            {
                node.Condition.Accept(this); // Emite a condição
            }
            Emit("; "); // Separador da condição

            if (node.Increment != null)
            {
                 if (node.Increment is AssignmentStatementNode assignStmt)
                {
                    Emit($"{assignStmt.Identifier.Lexeme} = ");
                    assignStmt.Value.Accept(this);
                }
                else if (node.Increment is ExpressionStatementNode exprStmt)
                {
                    exprStmt.Expression.Accept(this);
                }
            }
            EmitLine(")"); // Fecha o parêntese do for
            node.Body.Accept(this); // Emite o corpo do loop
        }

        // Implementado: Visit para ExpressionStatementNode
        public void Visit(ExpressionStatementNode node)
        {
            node.Expression.Accept(this); // Just visit the expression
            EmitLine(";"); // Expression used as a statement usually ends with a semicolon
        }


        // Expressões
        public void Visit(BinaryExpressionNode node)
        {
            node.Left.Accept(this);
            Emit($" {ConvertTokenTypeToCSharpOperator(node.Operator.Type)} ");
            node.Right.Accept(this);
        }

        public void Visit(UnaryExpressionNode node)
        {
            Emit(ConvertTokenTypeToCSharpOperator(node.Operator.Type));
            node.Operand.Accept(this);
        }

        public void Visit(LiteralExpressionNode node)
        {
            switch (node.ValueToken.Type)
            {
                case TokenType.STRING_LITERAL:
                    Emit($"\"{node.ValueToken.Lexeme}\""); // Strings precisam de aspas
                    break;
                case TokenType.CHAR_LITERAL:
                    Emit($"'{node.ValueToken.Lexeme}'"); // Chars precisam de aspas simples
                    break;
                case TokenType.TRUE:
                case TokenType.FALSE:
                    Emit(node.ValueToken.Lexeme.ToLower()); // 'True'/'False' para 'true'/'false' em C#
                    break;
                case TokenType.FLOAT_LITERAL:
                    Emit($"{node.ValueToken.Lexeme}f"); // Adicionar 'f' para literais float em C#
                    break;
                default:
                    Emit(node.ValueToken.Lexeme); // Literais int, etc.
                    break;
            }
        }

        public void Visit(IdentifierExpressionNode node)
        {
            Emit(node.Identifier.Lexeme);
        }

        public void Visit(FunctionCallExpressionNode node)
        {
            Emit($"{node.Identifier.Lexeme}(");
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                node.Arguments[i].Accept(this); // Emite cada argumento
                if (i < node.Arguments.Count - 1)
                {
                    Emit(", ");
                }
            }
            Emit(")");
        }

        public void Visit(TypeNode node)
        {
            // TypeNode é visitado para obter o tipo, mas não gera código por si só
            // A conversão é feita por ConvertMacsLangTypeToCSharp
        }

        // --- Métodos Auxiliares de Conversão ---

        private string ConvertMacsLangTypeToCSharp(TokenType macsLangType)
        {
            switch (macsLangType)
            {
                case TokenType.INT_KEYWORD: return "int";
                case TokenType.FLOAT_KEYWORD: return "float";
                case TokenType.CHAR_KEYWORD: return "char";
                case TokenType.BOOL_KEYWORD: return "bool";
                case TokenType.STRING_KEYWORD: return "string";
                case TokenType.FUNC: return "void"; // Funções em C# que não tem retorno explicitamente
                default: throw new ArgumentException($"Tipo MACSLang desconhecido: {macsLangType}");
            }
        }

        private string ConvertTokenTypeToCSharpOperator(TokenType operatorType)
        {
            switch (operatorType)
            {
                case TokenType.PLUS: return "+";
                case TokenType.MINUS: return "-";
                case TokenType.MULTIPLY: return "*";
                case TokenType.DIVIDE: return "/";
                case TokenType.MODULO: return "%";
                case TokenType.EQUALS: return "==";
                case TokenType.NOT_EQUALS: return "!=";
                case TokenType.LESS_THAN: return "<";
                case TokenType.GREATER_THAN: return ">";
                case TokenType.LESS_EQUAL: return "<=";
                case TokenType.GREATER_EQUAL: return ">=";
                case TokenType.AND: return "&&";
                case TokenType.OR: return "||";
                case TokenType.NOT: return "!";
                default: throw new ArgumentException($"Operador MACSLang desconhecido: {operatorType}");
            }
        }

        private string FormatParameters(List<ParameterNode> parameters)
        {
            var formattedParams = new List<string>();
            foreach (var param in parameters)
            {
                formattedParams.Add($"{ConvertMacsLangTypeToCSharp(param.Type.TypeToken)} {param.Identifier.Lexeme}");
            }
            return string.Join(", ", formattedParams);
        }
    }
}
