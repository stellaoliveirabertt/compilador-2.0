// MACSLang.Parser/Parser.cs

using System;
using System.Collections.Generic;
using MACSLang.Lexer;
using MACSLang.Syntax.AST;

namespace MACSLang.Parser;

public class Parser
{
    private readonly MACSLang.Lexer.Lexer _lexer; // Nosso analisador léxico
    private readonly Queue<Token> _tokenBuffer; // Buffer para tokens lidos (para lookahead)

    // Construtor
    public Parser(MACSLang.Lexer.Lexer lexer)
    {
        _lexer = lexer;
        _tokenBuffer = new Queue<Token>();
        Advance(); // Pré-carrega o primeiro token
    }

    // Propriedade para o token atual (o primeiro no buffer)
    private Token CurrentToken => _tokenBuffer.Peek();

    // Avança para o próximo token, preenchendo o buffer se necessário
    private void Advance()
    {
        if (_tokenBuffer.Count > 0) _tokenBuffer.Dequeue(); // Remove o token atual

        // Se o buffer estiver vazio ou se precisamos de mais tokens para lookahead, pegue mais do lexer
        while (_tokenBuffer.Count < 1) // Mantemos pelo menos 1 token no buffer (o CurrentToken)
            _tokenBuffer.Enqueue(_lexer.NextToken());
    }

    // Olha para o token 'k' posições à frente do token atual sem consumi-lo
    private Token Peek(int k = 1) // Padrão é olhar 1 token à frente
    {
        // Garante que temos tokens suficientes no buffer
        while (_tokenBuffer.Count <= k) _tokenBuffer.Enqueue(_lexer.NextToken());
        return _tokenBuffer.ToArray()[k]; // Convertemos para array para acessar por índice
    }


    // Verifica se o tipo do token atual corresponde ao tipo esperado
    private bool Match(TokenType type)
    {
        return CurrentToken.Type == type;
    }

    // Consome o token atual se ele corresponder ao tipo esperado,
    // e avança para o próximo. Caso contrário, lança um erro.
    private Token Consume(TokenType type, string errorMessage)
    {
        if (Match(type))
        {
            var consumedToken = CurrentToken;
            Advance(); // Avança o token no buffer
            return consumedToken;
        }

        throw new ParseException(errorMessage, CurrentToken);
    }

    // --- Métodos de Parsing para a Gramática da MACSLang ---

    // Ponto de entrada principal para o parsing do programa completo
    public ProgramNode ParseProgram()
    {
        List<FunctionDeclarationNode> functions = new();

        while (!Match(TokenType.EOF)) functions.Add(ParseFunctionDeclaration());

        return new ProgramNode(functions);
    }

    // Regra: func <nome>(<parametros>): <tipo_retorno> { <body> }
    private FunctionDeclarationNode ParseFunctionDeclaration()
    {
        var funcToken = Consume(TokenType.FUNC, "Esperado palavra-chave 'func'.");
        var identifier = Consume(TokenType.IDENTIFIER, "Esperado nome da função.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após o nome da função.");

        List<ParameterNode> parameters = new();
        if (!Match(TokenType.CLOSE_PAREN))
        {
            parameters.Add(ParseParameter());
            while (Match(TokenType.COMMA))
            {
                Advance(); // Consome a vírgula
                parameters.Add(ParseParameter());
            }
        }

        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após parâmetros da função.");

        Consume(TokenType.COLON, "Esperado ':' antes do tipo de retorno da função.");
        var returnType = ParseType();

        var body = ParseBlock();

        return new FunctionDeclarationNode(identifier, parameters, returnType, body, funcToken);
    }

    // Regra: <nome>: <tipo>
    private ParameterNode ParseParameter()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Esperado nome do parâmetro.");
        Consume(TokenType.COLON, "Esperado ':' antes do tipo do parâmetro.");
        var type = ParseType();
        return new ParameterNode(identifier, type);
    }

    // Regra: int | float | char | bool | string
    private TypeNode ParseType()
    {
        var typeToken = CurrentToken;
        if (Match(TokenType.INT_KEYWORD) || Match(TokenType.FLOAT_KEYWORD) ||
            Match(TokenType.CHAR_KEYWORD) || Match(TokenType.BOOL_KEYWORD) ||
            Match(TokenType.STRING_KEYWORD))
        {
            Advance(); // Consome o token de tipo
            return new TypeNode(typeToken);
        }

        throw new ParseException("Esperado um tipo de dado (int, float, char, bool, string).", CurrentToken);
    }

    // Regra: { <statements> }
    private BlockStatementNode ParseBlock()
    {
        var openBraceToken = Consume(TokenType.OPEN_BRACE, "Esperado '{' para iniciar o bloco de código.");
        List<StatementNode> statements = new();
        while (!Match(TokenType.CLOSE_BRACE) && !Match(TokenType.EOF)) statements.Add(ParseStatement());
        Consume(TokenType.CLOSE_BRACE, "Esperado '}' para fechar o bloco de código.");
        return new BlockStatementNode(statements, openBraceToken);
    }

    // Regra para analisar um comando (statement)
    private StatementNode ParseStatement()
    {
        if (Match(TokenType.VAR))
        {
            return ParseVariableDeclaration();
        }
        else if (Match(TokenType.RETURN))
        {
            return ParseReturnStatement();
        }
        else if (Match(TokenType.PRINT))
        {
            return ParsePrintStatement();
        }
        else if (Match(TokenType.INPUT))
        {
            return ParseInputStatement();
        }
        else if (Match(TokenType.IF))
        {
            return ParseIfStatement();
        }
        else if (Match(TokenType.WHILE))
        {
            return ParseWhileStatement();
        }
        else if (Match(TokenType.FOR))
        {
            return ParseForStatement();
        }
        // Verifica se é uma atribuição ou chamada de função que é um statement
        // Usamos Peek(1) para olhar o próximo token sem consumi-lo.
        else if (Match(TokenType.IDENTIFIER) &&
                 (Peek(1).Type == TokenType.ASSIGN || Peek(1).Type == TokenType.OPEN_PAREN))
        {
            if (Peek(1).Type == TokenType.ASSIGN)
            {
                return ParseAssignmentStatement();
            }
            else // if (Peek(1).Type == TokenType.OPEN_PAREN)
            {
                ExpressionNode expr = ParseFunctionCallExpression();
                Consume(TokenType.SEMICOLON, "Esperado ';' após a chamada de função.");
                return new ExpressionStatementNode(expr, expr.Token);
            }
        }

        throw new ParseException($"Token inesperado para início de comando: '{CurrentToken.Lexeme}'.", CurrentToken);
    }

    // Regra: var <nome>: <tipo> [= <valor>];
    private VariableDeclarationNode ParseVariableDeclaration()
    {
        var varToken = Consume(TokenType.VAR, "Esperado palavra-chave 'var'.");
        var identifier = Consume(TokenType.IDENTIFIER, "Esperado nome da variável.");
        Consume(TokenType.COLON, "Esperado ':' antes do tipo da variável.");
        var type = ParseType();

        ExpressionNode? initialValue = null;
        if (Match(TokenType.ASSIGN))
        {
            Advance(); // Consome '='
            initialValue = ParseExpression();
        }

        Consume(TokenType.SEMICOLON, "Esperado ';' após a declaração de variável.");
        return new VariableDeclarationNode(identifier, type, initialValue, varToken);
    }

    // Regra: <identificador> = <expressao>;
    private AssignmentStatementNode ParseAssignmentStatement()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Esperado identificador para atribuição.");
        var assignToken = Consume(TokenType.ASSIGN, "Esperado '=' para atribuição.");
        var value = ParseExpression();
        Consume(TokenType.SEMICOLON, "Esperado ';' após a atribuição.");
        return new AssignmentStatementNode(identifier, value, assignToken);
    }

    // Regra: return <expressao>;
    private ReturnStatementNode ParseReturnStatement()
    {
        var returnToken = Consume(TokenType.RETURN, "Esperado palavra-chave 'return'.");
        var value = ParseExpression();
        Consume(TokenType.SEMICOLON, "Esperado ';' após a expressão de retorno.");
        return new ReturnStatementNode(value, returnToken);
    }

    // Regra: print(<expressao>);
    private PrintStatementNode ParsePrintStatement()
    {
        var printToken = Consume(TokenType.PRINT, "Esperado palavra-chave 'print'.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após 'print'.");
        var expr = ParseExpression();
        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após a expressão do 'print'.");
        Consume(TokenType.SEMICOLON, "Esperado ';' após o 'print'.");
        return new PrintStatementNode(expr, printToken);
    }

    // Regra: input(<variavel>);
    private InputStatementNode ParseInputStatement()
    {
        var inputToken = Consume(TokenType.INPUT, "Esperado palavra-chave 'input'.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após 'input'.");
        var targetIdentifier = Consume(TokenType.IDENTIFIER, "Esperado identificador para 'input'.");
        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após o identificador do 'input'.");
        Consume(TokenType.SEMICOLON, "Esperado ';' após o 'input'.");
        return new InputStatementNode(targetIdentifier, inputToken);
    }

    // Regra: if (<condicao>) { <bloco> } [else { <bloco> }]
    private IfStatementNode ParseIfStatement()
    {
        var ifToken = Consume(TokenType.IF, "Esperado palavra-chave 'if'.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após 'if'.");
        var condition = ParseExpression();
        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após a condição do 'if'.");
        var trueBlock = ParseBlock();

        BlockStatementNode? elseBlock = null;
        if (Match(TokenType.ELSE))
        {
            Advance(); // Consome 'else'
            elseBlock = ParseBlock();
        }

        return new IfStatementNode(condition, trueBlock, elseBlock, ifToken);
    }

    // Regra: while (<condicao>) { <bloco> }
    private WhileStatementNode ParseWhileStatement()
    {
        var whileToken = Consume(TokenType.WHILE, "Esperado palavra-chave 'while'.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após 'while'.");
        var condition = ParseExpression();
        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após a condição do 'while'.");
        var body = ParseBlock();
        return new WhileStatementNode(condition, body, whileToken);
    }

    // Regra: for (<inicializacao>; <condicao>; <incremento>) { <bloco> }
    private ForStatementNode ParseForStatement()
    {
        var forToken = Consume(TokenType.FOR, "Esperado palavra-chave 'for'.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após 'for'.");

        StatementNode? initialization = null;
        if (!Match(TokenType.SEMICOLON))
        {
            if (Match(TokenType.VAR))
            {
                initialization = ParseVariableDeclaration(); // já consome o ';'
            }
            else if (Match(TokenType.IDENTIFIER) && Peek(1).Type == TokenType.ASSIGN) // Usando Peek(1)
            {
                initialization = ParseAssignmentStatement(); // já consome o ';'
            }
            else
            {
                // Caso a inicialização seja uma expressão simples seguida de ';'
                var initExpr = ParseExpression();
                Consume(TokenType.SEMICOLON, "Esperado ';' após a inicialização do 'for'.");
                initialization = new ExpressionStatementNode(initExpr, initExpr.Token);
            }
        }
        else
        {
            Advance(); // Consome o ';' se a inicialização for vazia
        }


        ExpressionNode? condition = null;
        if (!Match(TokenType.SEMICOLON)) condition = ParseExpression();
        Consume(TokenType.SEMICOLON, "Esperado ';' após a condição do 'for'.");

        StatementNode? increment = null;
        if (!Match(TokenType.CLOSE_PAREN))
        {
            if (Match(TokenType.IDENTIFIER) && Peek(1).Type == TokenType.ASSIGN) // Usando Peek(1)
            {
                increment = ParseAssignmentStatementWithoutSemicolon(); // Não consome o ';' final
            }
            else if (Match(TokenType.IDENTIFIER) && Peek(1).Type == TokenType.OPEN_PAREN) // Usando Peek(1)
            {
                ExpressionNode incrementExpr = ParseFunctionCallExpression();
                increment = new ExpressionStatementNode(incrementExpr, incrementExpr.Token);
            }
            else
            {
                // Caso seja uma expressão simples como 'i + 1' sem atribuição,
                // que não é comum como "incremento", mas possível.
                var incrementExpr = ParseExpression();
                increment = new ExpressionStatementNode(incrementExpr, incrementExpr.Token);
            }
        }

        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após o incremento do 'for'.");

        var body = ParseBlock();
        return new ForStatementNode(initialization, condition, increment, body, forToken);
    }

    // Auxiliar para atribuição no for loop que não espera ';'
    private AssignmentStatementNode ParseAssignmentStatementWithoutSemicolon()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Esperado identificador para atribuição no for.");
        var assignToken = Consume(TokenType.ASSIGN, "Esperado '=' para atribuição no for.");
        var value = ParseExpression();
        return new AssignmentStatementNode(identifier, value, assignToken);
    }


    // Regra para analisar uma expressão (prioridade de operadores)
    private ExpressionNode ParseExpression()
    {
        return ParseLogicalOrExpression();
    }

    // Or (||)
    private ExpressionNode ParseLogicalOrExpression()
    {
        var expr = ParseLogicalAndExpression();
        while (Match(TokenType.OR))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseLogicalAndExpression());
        }

        return expr;
    }

    // And (&&)
    private ExpressionNode ParseLogicalAndExpression()
    {
        var expr = ParseEqualityExpression();
        while (Match(TokenType.AND))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseEqualityExpression());
        }

        return expr;
    }

    // Igualdade (==, !=)
    private ExpressionNode ParseEqualityExpression()
    {
        var expr = ParseComparisonExpression();
        while (Match(TokenType.EQUALS) || Match(TokenType.NOT_EQUALS))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseComparisonExpression());
        }

        return expr;
    }

    // Comparação (<, >, <=, >=)
    private ExpressionNode ParseComparisonExpression()
    {
        var expr = ParseAdditiveExpression();
        while (Match(TokenType.LESS_THAN) || Match(TokenType.GREATER_THAN) ||
               Match(TokenType.LESS_EQUAL) || Match(TokenType.GREATER_EQUAL))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseAdditiveExpression());
        }

        return expr;
    }

    // Adição/Subtração (+, -)
    private ExpressionNode ParseAdditiveExpression()
    {
        var expr = ParseMultiplicativeExpression();
        while (Match(TokenType.PLUS) || Match(TokenType.MINUS))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseMultiplicativeExpression());
        }

        return expr;
    }

    // Multiplicação/Divisão/Módulo (*, /, %)
    private ExpressionNode ParseMultiplicativeExpression()
    {
        var expr = ParseUnaryExpression();
        while (Match(TokenType.MULTIPLY) || Match(TokenType.DIVIDE) || Match(TokenType.MODULO))
        {
            var op = CurrentToken;
            Advance();
            expr = new BinaryExpressionNode(expr, op, ParseUnaryExpression());
        }

        return expr;
    }

    // Unário (!, -)
    private ExpressionNode ParseUnaryExpression()
    {
        if (Match(TokenType.NOT) || Match(TokenType.MINUS))
        {
            var op = CurrentToken;
            Advance();
            return new UnaryExpressionNode(op, ParseUnaryExpression());
        }

        return ParsePrimaryExpression();
    }

    // Expressões primárias (literais, identificadores, chamadas de função, parênteses)
    private ExpressionNode ParsePrimaryExpression()
    {
        if (Match(TokenType.INT_LITERAL) || Match(TokenType.FLOAT_LITERAL) ||
            Match(TokenType.STRING_LITERAL) || Match(TokenType.CHAR_LITERAL) ||
            Match(TokenType.TRUE) || Match(TokenType.FALSE))
        {
            var literalToken = CurrentToken;
            Advance();
            return new LiteralExpressionNode(literalToken);
        }
        else if (Match(TokenType.IDENTIFIER))
        {
            var identifierToken = CurrentToken;
            // Se o próximo token for '(', é uma chamada de função
            if (Peek(1).Type == TokenType.OPEN_PAREN)
                // Não avançamos o identificador aqui, pois ParseFunctionCallExpression irá consumi-lo.
                return ParseFunctionCallExpression();
            // Se não for chamada de função, apenas avançamos o identificador e retornamos
            Advance(); // Consome o IDENTIFIER
            return new IdentifierExpressionNode(identifierToken);
        }
        else if (Match(TokenType.OPEN_PAREN))
        {
            Advance();
            var expr = ParseExpression();
            Consume(TokenType.CLOSE_PAREN, "Esperado ')' para fechar a expressão parentesizada.");
            return expr;
        }

        throw new ParseException($"Token inesperado para início de expressão: '{CurrentToken.Lexeme}'.", CurrentToken);
    }

    // Analisa uma chamada de função
    private FunctionCallExpressionNode ParseFunctionCallExpression()
    {
        var identifierToken = Consume(TokenType.IDENTIFIER, "Esperado nome da função para chamada.");
        Consume(TokenType.OPEN_PAREN, "Esperado '(' após o nome da função em chamada.");
        List<ExpressionNode> arguments = new();
        if (!Match(TokenType.CLOSE_PAREN))
        {
            arguments.Add(ParseExpression());
            while (Match(TokenType.COMMA))
            {
                Advance();
                arguments.Add(ParseExpression());
            }
        }

        Consume(TokenType.CLOSE_PAREN, "Esperado ')' após os argumentos da função.");
        return new FunctionCallExpressionNode(identifierToken, arguments);
    }
}

// Classe para representar erros de parsing
public class ParseException : Exception
{
    public Token ErrorToken { get; }

    public ParseException(string message, Token errorToken)
        : base(
            $"Erro de Sintaxe: {message} em Linha: {errorToken.Line}, Coluna: {errorToken.Column}. Token: '{errorToken.Lexeme}' ({errorToken.Type})")
    {
        ErrorToken = errorToken;
    }
}