// MACSLang.SemanticAnalyzer/SemanticAnalyzer.cs

using System;
using System.Collections.Generic;
using MACSLang.Lexer; // Para TokenType e Token
using MACSLang.Syntax.AST; // Para todos os nós da AST e IAstVisitor

namespace MACSLang.SemanticAnalyzer;

// A classe SemanticAnalyzer implementa IAstVisitor para percorrer a AST
public class SemanticAnalyzer : IAstVisitor
{
    private SymbolTable _currentScope; // O escopo atual durante a travessia da AST

    private FunctionSymbol?
        _currentFunction; // A função sendo analisada atualmente (para verificar retornos), agora anulável

    public SemanticAnalyzer()
    {
        // O escopo global é o primeiro a ser inicializado
        _currentScope = new SymbolTable(null);
        _currentFunction = null; // Nenhuma função está sendo analisada no início
    }

    // Método principal para iniciar a análise semântica
    public void Analyze(ProgramNode program)
    {
        Visit(program);
    }

    // --- Implementação dos métodos Visit de IAstVisitor ---

    public void Visit(ProgramNode node)
    {
        // Primeiro passo: Coletar todas as declarações de funções no escopo global
        // Isso permite que funções chamem umas às outras, mesmo se definidas depois.
        foreach (var funcDecl in node.Functions)
        {
            var paramTypes = new List<TokenType>();
            foreach (var param in funcDecl.Parameters) paramTypes.Add(param.Type.TypeToken);
            var funcSymbol = new FunctionSymbol(funcDecl.Identifier.Lexeme, paramTypes, funcDecl.ReturnType.TypeToken);
            _currentScope.Define(funcSymbol); // Define a função no escopo global
        }

        // Segundo passo: Visitar cada função para analisar seus corpos
        foreach (var funcDecl in node.Functions)
        {
            // Entra no escopo da função
            _currentScope = new SymbolTable(_currentScope); // Cria um novo escopo filho para a função
            _currentFunction =
                _currentScope.Resolve(funcDecl.Identifier.Lexeme) as FunctionSymbol; // Define a função atual

            // Define os parâmetros da função no novo escopo
            foreach (var param in funcDecl.Parameters)
            {
                var paramSymbol = new VariableSymbol(param.Identifier.Lexeme, param.Type.TypeToken);
                _currentScope.Define(paramSymbol); // Parâmetro se torna uma variável no escopo da função
            }

            // Visita o corpo da função
            funcDecl.Body.Accept(this);

            // Sai do escopo da função
            _currentScope = _currentScope.ParentScope!; // O escopo pai nunca será nulo aqui
            _currentFunction = null; // Reseta a função atual
        }
    }

    public void Visit(FunctionDeclarationNode node)
    {
        // Já tratado no Visit(ProgramNode) e no loop do corpo da função.
        // Este método só seria chamado se a função fosse visitada de forma independente.
    }

    // NOVO MÉTODO: Visit para BlockStatementNode
    public void Visit(BlockStatementNode node)
    {
        // Simplesmente visita cada comando dentro do bloco
        foreach (var statement in node.Statements) statement.Accept(this);
    }

    public void Visit(VariableDeclarationNode node)
    {
        // Verifica se a variável já existe no escopo atual
        if (_currentScope.Symbols.ContainsKey(node.Identifier.Lexeme))
            throw new SemanticError($"Variável '{node.Identifier.Lexeme}' já declarada neste escopo.", node.Identifier);

        // Define a variável na tabela de símbolos do escopo atual
        var varSymbol = new VariableSymbol(node.Identifier.Lexeme, node.Type.TypeToken);
        _currentScope.Define(varSymbol);

        // Se há um valor inicial, verifica a compatibilidade de tipo
        if (node.InitialValue != null)
        {
            var valueType = GetExpressionType(node.InitialValue);
            if (!IsAssignable(node.Type.TypeToken, valueType))
                throw new SemanticError(
                    $"Tipo incompatível na inicialização da variável '{node.Identifier.Lexeme}'. Esperado '{node.Type.TypeToken}', encontrado '{valueType}'.",
                    node.InitialValue.Token);
        }
    }

    public void Visit(AssignmentStatementNode node)
    {
        // Verifica se a variável de destino foi declarada
        var targetSymbol = _currentScope.Resolve(node.Identifier.Lexeme); // Agora anulável
        if (targetSymbol == null)
            throw new SemanticError($"Variável '{node.Identifier.Lexeme}' não declarada.", node.Identifier);
        if (!(targetSymbol is VariableSymbol))
            throw new SemanticError($"Não é possível atribuir a '{node.Identifier.Lexeme}'. Ele não é uma variável.",
                node.Identifier);

        // Verifica a compatibilidade de tipo na atribuição
        var targetType = (targetSymbol as VariableSymbol)!.Type; // Usar ! para indicar que não será nulo
        var valueType = GetExpressionType(node.Value);

        if (!IsAssignable(targetType, valueType))
            throw new SemanticError(
                $"Tipo incompatível na atribuição para '{node.Identifier.Lexeme}'. Esperado '{targetType}', encontrado '{valueType}'.",
                node.Value.Token);

        node.Value.Accept(this); // Visita a expressão de valor
    }

    public void Visit(PrintStatementNode node)
    {
        // Não há verificação de tipo estrita para print na MACSLang (pode printar qualquer expressão)
        node.Expression.Accept(this); // Visita a expressão a ser impressa
    }


    public void Visit(InputStatementNode node)
    {
        // Verifica se a variável de destino foi declarada e é uma variável
        var targetSymbol = _currentScope.Resolve(node.TargetIdentifier.Lexeme);
        if (targetSymbol == null)
            throw new SemanticError($"Variável '{node.TargetIdentifier.Lexeme}' não declarada para input.",
                node.TargetIdentifier);
        if (!(targetSymbol is VariableSymbol))
            throw new SemanticError(
                $"Não é possível ler input para '{node.TargetIdentifier.Lexeme}'. Ele não é uma variável.",
                node.TargetIdentifier);

        // NOVO: Anotar o tipo resolvido da variável no nó da AST
        node.ResolvedTargetType = (targetSymbol as VariableSymbol)!.Type;
    }

    public void Visit(ReturnStatementNode node)
    {
        if (_currentFunction == null) throw new SemanticError("Comando 'return' fora de uma função.", node.Token);

        var returnExprType = GetExpressionType(node.Value);
        if (!IsAssignable(_currentFunction.ReturnType, returnExprType))
            throw new SemanticError(
                $"Tipo de retorno incompatível para a função '{_currentFunction.Name}'. Esperado '{_currentFunction.ReturnType}', encontrado '{returnExprType}'.",
                node.Value.Token);
        node.Value.Accept(this); // Visita a expressão de retorno
    }

    public void Visit(IfStatementNode node)
    {
        // A condição de um IF deve ser booleana
        var conditionType = GetExpressionType(node.Condition);
        if (conditionType != TokenType.BOOL_KEYWORD)
            throw new SemanticError($"Condição do 'if' deve ser booleana. Encontrado '{conditionType}'.",
                node.Condition.Token);
        node.Condition.Accept(this); // Visita a expressão da condição

        // Entra no escopo do bloco IF
        _currentScope = new SymbolTable(_currentScope);
        node.TrueBlock.Accept(this);
        _currentScope = _currentScope.ParentScope!; // O escopo pai nunca será nulo aqui

        if (node.ElseBlock != null)
        {
            // Entra no escopo do bloco ELSE
            _currentScope = new SymbolTable(_currentScope);
            node.ElseBlock.Accept(this);
            _currentScope = _currentScope.ParentScope!; // O escopo pai nunca será nulo aqui
        }
    }

    public void Visit(WhileStatementNode node)
    {
        // A condição de um WHILE deve ser booleana
        var conditionType = GetExpressionType(node.Condition);
        if (conditionType != TokenType.BOOL_KEYWORD)
            throw new SemanticError($"Condição do 'while' deve ser booleana. Encontrado '{conditionType}'.",
                node.Condition.Token);
        node.Condition.Accept(this); // Visita a expressão da condição

        // Entra no escopo do bloco WHILE
        _currentScope = new SymbolTable(_currentScope);
        node.Body.Accept(this);
        _currentScope = _currentScope.ParentScope!; // O escopo pai nunca será nulo aqui
    }

    public void Visit(ForStatementNode node)
    {
        // Um novo escopo é criado para o loop 'for' (para inicialização)
        _currentScope = new SymbolTable(_currentScope);

        if (node.Initialization != null) node.Initialization.Accept(this); // Visita a inicialização

        if (node.Condition != null)
        {
            var conditionType = GetExpressionType(node.Condition);
            if (conditionType != TokenType.BOOL_KEYWORD)
                throw new SemanticError($"Condição do 'for' deve ser booleana. Encontrado '{conditionType}'.",
                    node.Condition.Token);
            node.Condition.Accept(this); // Visita a condição
        }

        // Visita o corpo do loop antes do incremento (semântica do for)
        node.Body.Accept(this);

        if (node.Increment != null) node.Increment.Accept(this); // Visita o incremento

        _currentScope = _currentScope.ParentScope!; // O escopo pai nunca será nulo aqui
    }

    public void Visit(ExpressionStatementNode node)
    {
        node.Expression.Accept(this); // Apenas visita a expressão
    }

    public void Visit(BinaryExpressionNode node)
    {
        node.Left.Accept(this); // Visita a expressão esquerda
        node.Right.Accept(this); // Visita a expressão direita

        // Obtenha os tipos dos operandos
        var leftType = GetExpressionType(node.Left);
        var rightType = GetExpressionType(node.Right);

        // Verifica a compatibilidade de tipos para operações binárias
        switch (node.Operator.Type)
        {
            case TokenType.PLUS:
                // Tratamento especial para o operador PLUS: pode ser adição numérica ou concatenação de string
                if (leftType == TokenType.STRING_KEYWORD || rightType == TokenType.STRING_KEYWORD)
                    // Se qualquer um dos operandos for string, assume concatenação de string.
                    // Outros tipos (numéricos, bool, char) podem ser implicitamente convertidos para string para concatenação.
                    node.ExpressionType = TokenType.STRING_KEYWORD;
                else if (IsNumeric(leftType) && IsNumeric(rightType))
                    // Se ambos forem numéricos, é adição aritmética
                    node.ExpressionType = leftType == TokenType.FLOAT_KEYWORD || rightType == TokenType.FLOAT_KEYWORD
                        ? TokenType.FLOAT_KEYWORD
                        : TokenType.INT_KEYWORD;
                else
                    // Tipos inválidos para PLUS
                    throw new SemanticError(
                        $"Operador '+' espera operandos numéricos ou um dos operandos deve ser do tipo string para concatenação. Encontrado '{leftType}' e '{rightType}'.",
                        node.Operator);
                break;

            case TokenType.MINUS:
            case TokenType.MULTIPLY:
            case TokenType.DIVIDE:
            case TokenType.MODULO:
                // Outras operações aritméticas: devem ser numéricas
                if (!IsNumeric(leftType) || !IsNumeric(rightType))
                    throw new SemanticError(
                        $"Operadores aritméticos (- * / %) esperam operandos numéricos. Encontrado '{leftType}' e '{rightType}'.",
                        node.Operator);
                node.ExpressionType = leftType == TokenType.FLOAT_KEYWORD || rightType == TokenType.FLOAT_KEYWORD
                    ? TokenType.FLOAT_KEYWORD
                    : TokenType.INT_KEYWORD;
                break;

            case TokenType.EQUALS:
            case TokenType.NOT_EQUALS:
                // Operadores de igualdade: tipos devem ser compatíveis
                if (!AreComparable(leftType, rightType))
                    throw new SemanticError(
                        $"Operadores de igualdade (== !=) esperam operandos comparáveis. Encontrado '{leftType}' e '{rightType}'.",
                        node.Operator);
                node.ExpressionType = TokenType.BOOL_KEYWORD; // Resultado é booleano
                break;

            case TokenType.LESS_THAN:
            case TokenType.GREATER_THAN:
            case TokenType.LESS_EQUAL:
            case TokenType.GREATER_EQUAL:
                // Operadores de comparação: numéricos
                if (!IsNumeric(leftType) || !IsNumeric(rightType))
                    throw new SemanticError(
                        $"Operadores de comparação (< > <= >=) esperam operandos numéricos. Encontrado '{leftType}' e '{rightType}'.",
                        node.Operator);
                node.ExpressionType = TokenType.BOOL_KEYWORD; // Resultado é booleano
                break;

            case TokenType.AND:
            case TokenType.OR:
                // Operadores lógicos: booleanos
                if (leftType != TokenType.BOOL_KEYWORD || rightType != TokenType.BOOL_KEYWORD)
                    throw new SemanticError(
                        $"Operadores lógicos (&& ||) esperam operandos booleanos. Encontrado '{leftType}' e '{rightType}'.",
                        node.Operator);
                node.ExpressionType = TokenType.BOOL_KEYWORD; // Resultado é booleano
                break;

            default:
                throw new SemanticError($"Operador binário desconhecido: {node.Operator.Lexeme}", node.Operator);
        }
    }

    public void Visit(UnaryExpressionNode node)
    {
        node.Operand.Accept(this); // Visita o operando

        var operandType = GetExpressionType(node.Operand);

        switch (node.Operator.Type)
        {
            case TokenType.MINUS: // Negação numérica
                if (!IsNumeric(operandType))
                    throw new SemanticError(
                        $"Operador unário '-' espera operando numérico. Encontrado '{operandType}'.", node.Operator);
                node.ExpressionType = operandType; // Tipo permanece o mesmo (int ou float)
                break;
            case TokenType.NOT: // Negação lógica
                if (operandType != TokenType.BOOL_KEYWORD)
                    throw new SemanticError(
                        $"Operador unário '!' espera operando booleano. Encontrado '{operandType}'.", node.Operator);
                node.ExpressionType = TokenType.BOOL_KEYWORD; // Tipo permanece booleano
                break;
            default:
                throw new SemanticError($"Operador unário desconhecido: {node.Operator.Lexeme}", node.Operator);
        }
    }

    public void Visit(LiteralExpressionNode node)
    {
        // O tipo da expressão literal é o tipo do seu token literal
        // Assumimos que o LiteralExpressionNode já inferiu o tipo correto
        // baseado no TokenType do seu ValueToken (ex: INT_LITERAL -> INT_KEYWORD)
        switch (node.ValueToken.Type)
        {
            case TokenType.INT_LITERAL:
                node.ExpressionType = TokenType.INT_KEYWORD;
                break;
            case TokenType.FLOAT_LITERAL:
                node.ExpressionType = TokenType.FLOAT_KEYWORD;
                break;
            case TokenType.STRING_LITERAL:
                node.ExpressionType = TokenType.STRING_KEYWORD;
                break;
            case TokenType.CHAR_LITERAL:
                node.ExpressionType = TokenType.CHAR_KEYWORD;
                break;
            case TokenType.TRUE:
            case TokenType.FALSE:
                node.ExpressionType = TokenType.BOOL_KEYWORD;
                break;
            default:
                throw new SemanticError($"Tipo de literal inesperado: {node.ValueToken.Type}", node.ValueToken);
        }
    }

    public void Visit(IdentifierExpressionNode node)
    {
        // Resolve o identificador na tabela de símbolos para obter seu tipo
        var symbol = _currentScope.Resolve(node.Identifier.Lexeme); // Agora anulável
        if (symbol == null)
            throw new SemanticError($"Identificador '{node.Identifier.Lexeme}' não declarado.", node.Identifier);
        if (!(symbol is VariableSymbol))
            throw new SemanticError(
                $"O identificador '{node.Identifier.Lexeme}' não é uma variável utilizável em uma expressão.",
                node.Identifier);
        node.ExpressionType = (symbol as VariableSymbol)!.Type; // Usar ! para indicar que não será nulo
    }

    public void Visit(FunctionCallExpressionNode node)
    {
        // Verifica se a função foi declarada
        var funcSymbol = _currentScope.Resolve(node.Identifier.Lexeme); // Agora anulável
        if (funcSymbol == null)
            throw new SemanticError($"Função '{node.Identifier.Lexeme}' não declarada.", node.Identifier);
        if (!(funcSymbol is FunctionSymbol))
            throw new SemanticError($"O identificador '{node.Identifier.Lexeme}' não é uma função.", node.Identifier);

        var calledFunc = funcSymbol as FunctionSymbol;

        // Verifica o número de argumentos
        if (node.Arguments.Count != calledFunc!.ParameterTypes.Count) // Usar ! para indicar que não será nulo
            throw new SemanticError(
                $"Número incorreto de argumentos para a função '{calledFunc.Name}'. Esperado {calledFunc.ParameterTypes.Count}, encontrado {node.Arguments.Count}.",
                node.Identifier);

        // Verifica os tipos dos argumentos
        for (var i = 0; i < node.Arguments.Count; i++)
        {
            node.Arguments[i].Accept(this); // Visita o argumento para inferir seu tipo
            var argType = GetExpressionType(node.Arguments[i]);
            if (!IsAssignable(calledFunc.ParameterTypes[i], argType))
                throw new SemanticError(
                    $"Tipo do argumento {i + 1} incompatível para a função '{calledFunc.Name}'. Esperado '{calledFunc.ParameterTypes[i]}', encontrado '{argType}'.",
                    node.Arguments[i].Token);
        }

        node.ExpressionType =
            calledFunc.ReturnType; // O tipo da expressão de chamada de função é o tipo de retorno da função
    }

    public void Visit(TypeNode node)
    {
        // TypeNode não tem 'ExpressionType' mas é visitado para validação.
        // A validação real é feita quando o TypeNode é usado (ex: VariableDeclarationNode)
    }

    // --- Métodos Auxiliares para Verificação de Tipos ---

    // Método para inferir o tipo de uma expressão.
    // Adiciona uma propriedade 'ExpressionType' a todos os nós de expressão na AST.
    private TokenType GetExpressionType(ExpressionNode exprNode)
    {
        // Visita a expressão para garantir que seu tipo foi inferido/validado.
        exprNode.Accept(this);

        return exprNode.ExpressionType;
    }

    // Verifica se um tipo 'source' pode ser atribuído a um tipo 'target'
    private bool IsAssignable(TokenType target, TokenType source)
    {
        if (target == source) return true;

        // Regras de conversão implícita (ex: int para float)
        if (target == TokenType.FLOAT_KEYWORD && source == TokenType.INT_KEYWORD) return true;

        // Tipos booleanos são específicos
        if (target == TokenType.BOOL_KEYWORD && (source == TokenType.TRUE || source == TokenType.FALSE)) return true;

        // Tipos de string aceitam apenas string
        if (target == TokenType.STRING_KEYWORD && source == TokenType.STRING_KEYWORD) return true;

        // Caractere aceita apenas caractere
        if (target == TokenType.CHAR_KEYWORD && source == TokenType.CHAR_KEYWORD) return true;

        return false;
    }

    // Verifica se um tipo é numérico (int ou float)
    private bool IsNumeric(TokenType type)
    {
        return type == TokenType.INT_KEYWORD || type == TokenType.FLOAT_KEYWORD;
    }

    // Verifica se dois tipos são comparáveis (para ==, !=)
    private bool AreComparable(TokenType type1, TokenType type2)
    {
        // Numéricos são comparáveis entre si
        if (IsNumeric(type1) && IsNumeric(type2)) return true;

        // Booleanos são comparáveis apenas com booleanos
        if (type1 == TokenType.BOOL_KEYWORD && type2 == TokenType.BOOL_KEYWORD) return true;

        // Strings são comparáveis apenas com strings
        if (type1 == TokenType.STRING_KEYWORD && type2 == TokenType.STRING_KEYWORD) return true;

        // Chars são comparáveis apenas com chars
        if (type1 == TokenType.CHAR_KEYWORD && type2 == TokenType.CHAR_KEYWORD) return true;

        return false;
    }
}