// MACSLang.Syntax/AST/PrintStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o comando de saída 'print'
public class PrintStatementNode : StatementNode
{
    public ExpressionNode Expression { get; } // A expressão a ser impressa

    public PrintStatementNode(ExpressionNode expression, Token printToken = null)
        : base(printToken) // Usa o token 'print' como referência
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}