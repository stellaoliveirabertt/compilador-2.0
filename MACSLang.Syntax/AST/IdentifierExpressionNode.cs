// MACSLang.Syntax/AST/IdentifierExpressionNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa o uso de um identificador (nome de variável ou função) em uma expressão
public class IdentifierExpressionNode : ExpressionNode
{
    public Token Identifier { get; } // O token do identificador

    public IdentifierExpressionNode(Token identifierToken) : base(identifierToken)
    {
        if (identifierToken.Type != TokenType.IDENTIFIER)
            throw new ArgumentException($"O token '{identifierToken.Lexeme}' não é um identificador.");
        Identifier = identifierToken;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}