// MACSLang.SemanticAnalyzer/FunctionSymbol.cs

using System.Collections.Generic;
using MACSLang.Lexer;

namespace MACSLang.SemanticAnalyzer;

// Representa um símbolo de função na tabela de símbolos
public class FunctionSymbol : Symbol
{
    public List<TokenType> ParameterTypes { get; } // Tipos dos parâmetros
    public TokenType ReturnType { get; } // Tipo de retorno da função

    public FunctionSymbol(string name, List<TokenType> parameterTypes, TokenType returnType)
        : base(name, TokenType.FUNC) // O tipo do símbolo FunctionSymbol é FUNC
    {
        ParameterTypes = parameterTypes ?? new List<TokenType>();
        ReturnType = returnType;
    }

    public override string ToString() // Adicionado 'override'
    {
        return $"FuncSymbol(Name='{Name}', Params:[{string.Join(", ", ParameterTypes)}], Return={ReturnType})";
    }
}