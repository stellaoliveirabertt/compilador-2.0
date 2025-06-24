// MACSLang.Syntax/AST/IAstVisitor.cs

namespace MACSLang.Syntax.AST;

// Interface para o padrão Visitor, permitindo percorrer a AST
public interface IAstVisitor
{
    // Métodos Visit para cada tipo concreto de nó da AST que será definido.
    // Adicionaremos mais métodos aqui conforme definirmos novos tipos de nós.
    void Visit(ProgramNode node);
    void Visit(FunctionDeclarationNode node);
    void Visit(VariableDeclarationNode node);
    void Visit(ReturnStatementNode node);
    void Visit(PrintStatementNode node);
    void Visit(InputStatementNode node);
    void Visit(IfStatementNode node);
    void Visit(WhileStatementNode node);
    void Visit(ForStatementNode node);
    void Visit(BinaryExpressionNode node);
    void Visit(UnaryExpressionNode node);
    void Visit(LiteralExpressionNode node);
    void Visit(IdentifierExpressionNode node);
    void Visit(AssignmentStatementNode node);
    void Visit(FunctionCallExpressionNode node);
    void Visit(TypeNode node); // Para tipos como 'int', 'float'
    void Visit(BlockStatementNode node);
    void Visit(ExpressionStatementNode node);
}