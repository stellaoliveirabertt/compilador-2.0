// MACSLang.Syntax/AST/AssignmentStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa uma atribuição de valor a uma variável (ex: variavel = valor;)
public class AssignmentStatementNode : StatementNode
{
    public Token Identifier { get; } // O identificador da variável que está recebendo o valor
    public ExpressionNode Value { get; } // A expressão cujo valor será atribuído

    public AssignmentStatementNode(Token identifier, ExpressionNode value, Token token = null)
        : base(token ?? identifier) // Usa o token do identificador como referência
    {
        Identifier = identifier;
        Value = value;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}