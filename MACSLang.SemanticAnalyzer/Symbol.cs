// MACSLang.SemanticAnalyzer/Symbol.cs

using MACSLang.Lexer; // Para TokenType

namespace MACSLang.SemanticAnalyzer;

// Classe base abstrata para todos os símbolos na tabela de símbolos
public abstract class Symbol
{
    public string Name { get; }
    public TokenType Type { get; } // O TokenType do tipo do símbolo (ex: INT_KEYWORD)

    protected Symbol(string name, TokenType type)
    {
        Name = name;
        Type = type;
    }

    public abstract override string ToString(); // Adicionado 'override'
}