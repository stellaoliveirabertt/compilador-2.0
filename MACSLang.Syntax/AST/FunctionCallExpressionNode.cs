// MACSLang.Syntax/AST/FunctionCallExpressionNode.cs

using System.Collections.Generic;
using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa uma chamada de função (ex: fatorial(numero))
public class FunctionCallExpressionNode : ExpressionNode
{
    public Token Identifier { get; } // O token do nome da função
    public List<ExpressionNode> Arguments { get; } // A lista de argumentos passados para a função

    public FunctionCallExpressionNode(Token identifier, List<ExpressionNode> arguments, Token token = null)
        : base(token ?? identifier) // Usa o token do identificador como referência
    {
        Identifier = identifier;
        Arguments = arguments ?? new List<ExpressionNode>();
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}