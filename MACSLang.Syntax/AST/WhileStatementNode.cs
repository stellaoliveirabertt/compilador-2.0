// MACSLang.Syntax/AST/WhileStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o laço de repetição 'while'
public class WhileStatementNode : StatementNode
{
    public ExpressionNode Condition { get; } // A condição do 'while'
    public BlockStatementNode Body { get; } // O bloco de código a ser repetido

    public WhileStatementNode(ExpressionNode condition, BlockStatementNode body, Token whileToken = null)
        : base(whileToken) // Usa o token 'while' como referência
    {
        Condition = condition;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}