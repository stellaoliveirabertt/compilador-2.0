// MACSLang.Syntax/AST/FunctionDeclarationNode.cs

using System.Collections.Generic;
using MACSLang.Lexer; // Para Token

namespace MACSLang.Syntax.AST;

// Representa a declaração de uma função (ex: func fatorial(n: int): int { ... })
public class FunctionDeclarationNode : AstNode
{
    public Token Identifier { get; } // Token do nome da função
    public List<ParameterNode> Parameters { get; }
    public TypeNode ReturnType { get; } // Tipo de retorno da função
    public BlockStatementNode Body { get; } // Bloco de código da função

    public FunctionDeclarationNode(Token identifier, List<ParameterNode> parameters, TypeNode returnType,
        BlockStatementNode body, Token token = null)
        : base(token ?? identifier) // Usa o token do identificador como referência
    {
        Identifier = identifier;
        Parameters = parameters ?? new List<ParameterNode>();
        ReturnType = returnType;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

// Nó para um parâmetro de função
public class ParameterNode : AstNode
{
    public Token Identifier { get; } // Token do nome do parâmetro
    public TypeNode Type { get; } // Tipo do parâmetro

    public ParameterNode(Token identifier, TypeNode type, Token token = null)
        : base(token ?? identifier)
    {
        Identifier = identifier;
        Type = type;
    }

    public override void Accept(IAstVisitor visitor)
    {
        // Parâmetros são visitados como parte da FunctionDeclaration, não individualmente no Visitor
        // mas mantemos o método aqui para completude se precisarmos de um visitor específico para parâmetros.
        // Para este projeto, não é estritamente necessário um Visit(ParameterNode node).
    }
}