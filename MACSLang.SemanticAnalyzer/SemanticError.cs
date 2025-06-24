// MACSLang.SemanticAnalyzer/SemanticError.cs

using System;
using MACSLang.Lexer; // Para Token

namespace MACSLang.SemanticAnalyzer;

// Exceção personalizada para erros semânticos
public class SemanticError : Exception
{
    public Token? ErrorToken { get; } // O token que causou o erro, se aplicável

    public SemanticError(string message, Token? errorToken = null)
        : base(FormatErrorMessage(message, errorToken))
    {
        ErrorToken = errorToken;
    }

    private static string FormatErrorMessage(string message, Token? errorToken)
    {
        if (errorToken != null)
            return
                $"Erro Semântico: {message} em Linha: {errorToken.Line}, Coluna: {errorToken.Column}. Token: '{errorToken.Lexeme}' ({errorToken.Type})";
        return $"Erro Semântico: {message}";
    }
}