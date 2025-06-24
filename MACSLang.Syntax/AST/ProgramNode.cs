// MACSLang.Syntax/AST/ProgramNode.cs

using System.Collections.Generic;

namespace MACSLang.Syntax.AST;

// Representa o programa completo (nó raiz da AST)
public class ProgramNode : AstNode
{
    public List<FunctionDeclarationNode> Functions { get; }

    public ProgramNode(List<FunctionDeclarationNode> functions)
    {
        Functions = functions ?? new List<FunctionDeclarationNode>();
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}