// MACSLang.Syntax/AST/ReturnStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o comando 'return' em uma função
public class ReturnStatementNode : StatementNode
{
    public ExpressionNode Value { get; } // A expressão cujo valor será retornado

    public ReturnStatementNode(ExpressionNode value, Token returnToken = null)
        : base(returnToken) // Usa o token 'return' como referência
    {
        Value = value;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}