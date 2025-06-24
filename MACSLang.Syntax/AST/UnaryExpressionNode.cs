// MACSLang.Syntax/AST/UnaryExpressionNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa uma operação unária (ex: -x, !true)
public class UnaryExpressionNode : ExpressionNode
{
    public Token Operator { get; } // O token do operador (MINUS para negação, NOT para negação lógica)
    public ExpressionNode Operand { get; } // A expressão à qual o operador se aplica

    public UnaryExpressionNode(Token op, ExpressionNode operand, Token token = null)
        : base(token ?? op) // Usa o token do operador como referência
    {
        Operator = op;
        Operand = operand;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}