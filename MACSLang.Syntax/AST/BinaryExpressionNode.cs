// MACSLang.Syntax/AST/BinaryExpressionNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa uma operação binária (ex: a + b, x == y)
public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; } // A expressão à esquerda do operador
    public Token Operator { get; } // O token do operador (PLUS, MINUS, EQUALS, etc.)
    public ExpressionNode Right { get; } // A expressão à direita do operador

    public BinaryExpressionNode(ExpressionNode left, Token op, ExpressionNode right, Token token = null)
        : base(token ?? op) // Usa o token do operador como referência
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}