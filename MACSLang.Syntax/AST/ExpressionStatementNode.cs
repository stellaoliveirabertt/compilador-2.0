// MACSLang.Syntax/AST/ExpressionStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa uma expressão que é usada como um comando (ex: uma chamada de função seguida de ';')
public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression { get; }

    public ExpressionStatementNode(ExpressionNode expression, Token token = null)
        : base(token ?? expression.Token) // Usa o token da expressão como referência
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}