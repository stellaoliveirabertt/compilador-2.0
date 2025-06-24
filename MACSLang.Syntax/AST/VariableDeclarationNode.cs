// MACSLang.Syntax/AST/VariableDeclarationNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa a declaração de uma variável (ex: var nome: tipo = valor;)
public class VariableDeclarationNode : StatementNode
{
    public Token Identifier { get; } // Token do nome da variável
    public TypeNode Type { get; } // Tipo da variável
    public ExpressionNode InitialValue { get; } // Valor inicial opcional

    public VariableDeclarationNode(Token identifier, TypeNode type, ExpressionNode initialValue = null,
        Token token = null)
        : base(token ?? identifier) // Usa o token do identificador como referência
    {
        Identifier = identifier;
        Type = type;
        InitialValue = initialValue;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}