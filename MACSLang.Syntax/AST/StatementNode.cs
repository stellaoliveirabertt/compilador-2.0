// MACSLang.Syntax/AST/StatementNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Classe base abstrata para todos os comandos (statements)
public abstract class StatementNode : AstNode
{
    protected StatementNode(Token token = null) : base(token)
    {
    }
}