// MACSLang.Syntax/AST/AstNode.cs

using MACSLang.Lexer; // Para acessar a classe Token

namespace MACSLang.Syntax.AST;

// Classe base abstrata para todos os nós da Árvore Sintática Abstrata (AST)
public abstract class AstNode
{
    // Propriedade opcional para armazenar o token que originou este nó,
    // útil para reportar erros com linha/coluna.
    public Token Token { get; }

    protected AstNode(Token token = null)
    {
        Token = token;
    }

    // Método abstrato para permitir que um "Visitor" percorra a AST.
    // Será importante para o Analisador Semântico e o Gerador de Código.
    public abstract void Accept(IAstVisitor visitor);
}