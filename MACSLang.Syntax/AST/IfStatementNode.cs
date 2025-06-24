// MACSLang.Syntax/AST/IfStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa a estrutura condicional 'if-else'
public class IfStatementNode : StatementNode
{
    public ExpressionNode Condition { get; } // A condição do 'if'
    public BlockStatementNode TrueBlock { get; } // O bloco de código se a condição for verdadeira
    public BlockStatementNode ElseBlock { get; } // Opcional: o bloco de código se a condição for falsa

    public IfStatementNode(ExpressionNode condition, BlockStatementNode trueBlock, BlockStatementNode elseBlock = null,
        Token ifToken = null)
        : base(ifToken) // Usa o token 'if' como referência
    {
        Condition = condition;
        TrueBlock = trueBlock;
        ElseBlock = elseBlock;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}