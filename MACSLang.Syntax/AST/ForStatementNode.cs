// MACSLang.Syntax/AST/ForStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o laço de repetição 'for'
public class ForStatementNode : StatementNode
{
    public StatementNode
        Initialization { get; } // A parte de inicialização do 'for' (pode ser VariableDeclaration ou Assignment)

    public ExpressionNode Condition { get; } // A condição de continuação do 'for'

    public StatementNode
        Increment { get; } // A parte de incremento do 'for' (geralmente Assignment ou ExpressionStatement)

    public BlockStatementNode Body { get; } // O bloco de código a ser repetido

    public ForStatementNode(StatementNode initialization, ExpressionNode condition, StatementNode increment,
        BlockStatementNode body, Token forToken = null)
        : base(forToken) // Usa o token 'for' como referência
    {
        Initialization = initialization;
        Condition = condition;
        Increment = increment;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}