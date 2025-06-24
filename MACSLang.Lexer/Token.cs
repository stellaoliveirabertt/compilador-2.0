// MACSLang.Lexer/Token.cs

namespace MACSLang.Lexer;

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string lexeme, int line, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"Type: {Type}, Lexeme: '{Lexeme}', Line: {Line}, Column: {Column}";
    }
}