// MACSLang.SemanticAnalyzer/VariableSymbol.cs

using MACSLang.Lexer;

namespace MACSLang.SemanticAnalyzer;

// Representa um símbolo de variável na tabela de símbolos
public class VariableSymbol : Symbol
{
    public VariableSymbol(string name, TokenType type)
        : base(name, type)
    {
    }

    public override string ToString() // Adicionado 'override'
    {
        return $"VarSymbol(Name='{Name}', Type={Type})";
    }
}