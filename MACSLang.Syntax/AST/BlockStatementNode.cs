// MACSLang.Syntax/AST/BlockStatementNode.cs

using System.Collections.Generic;
using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa um bloco de código delimitado por chaves { ... }
public class BlockStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; }

    public BlockStatementNode(List<StatementNode> statements, Token openBraceToken = null)
        : base(openBraceToken) // Opcional, para associar ao '{'
    {
        Statements = statements ?? new List<StatementNode>();
    }

    public override void Accept(IAstVisitor visitor)
    {
        // O visitor deve percorrer a lista de statements dentro deste bloco.
        // O método Visit(BlockStatementNode) no visitor será responsável por isso.
        visitor.Visit(this);
    }
}