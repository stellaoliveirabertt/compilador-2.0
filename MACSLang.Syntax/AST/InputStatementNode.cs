// MACSLang.Syntax/AST/InputStatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o comando de entrada 'input'
public class InputStatementNode : StatementNode
{
    public Token TargetIdentifier { get; } // O identificador da variável onde o valor lido será armazenado
    public TokenType ResolvedTargetType { get; set; } // NOVO: Tipo resolvido da variável de destino

    public InputStatementNode(Token targetIdentifier, Token? inputToken = null)
        : base(inputToken) // Usa o token 'input' como referência
    {
        TargetIdentifier = targetIdentifier;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}