// MACSLang.Syntax/AST/TypeNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa um tipo de dado (int, float, char, bool, string)
public class TypeNode : AstNode
{
    public TokenType TypeToken { get; } // O token que representa o tipo (e.g., INT_KEYWORD)

    public TypeNode(Token typeToken) : base(typeToken)
    {
        if (typeToken.Type != TokenType.INT_KEYWORD &&
            typeToken.Type != TokenType.FLOAT_KEYWORD &&
            typeToken.Type != TokenType.CHAR_KEYWORD &&
            typeToken.Type != TokenType.BOOL_KEYWORD &&
            typeToken.Type != TokenType.STRING_KEYWORD)
            // Isso deve ser tratado pelo parser, mas é um bom "fail-safe"
            throw new ArgumentException($"O token '{typeToken.Lexeme}' não é um tipo válido de MACSLang.");
        TypeToken = typeToken.Type;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}