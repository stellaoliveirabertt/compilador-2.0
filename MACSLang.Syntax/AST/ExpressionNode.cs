// MACSLang.Syntax/AST/ExpressionNode.cs

using MACSLang.Lexer; // Para TokenType e Token

namespace MACSLang.Syntax.AST;

// Classe base abstrata para todas as expressões
public abstract class ExpressionNode : AstNode
{
    // NOVO: Propriedade para armazenar o tipo semântico da expressão
    public TokenType ExpressionType { get; set; } // <- Adicione esta linha

    protected ExpressionNode(Token? token = null) : base(token)
    {
    }
}