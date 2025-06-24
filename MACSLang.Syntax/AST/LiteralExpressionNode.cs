// MACSLang.Syntax/AST/LiteralExpressionNode.cs

using MACSLang.Lexer;

namespace MACSLang.Syntax.AST;

// Representa um valor literal (inteiro, float, char, string, bool)
public class LiteralExpressionNode : ExpressionNode
{
    public Token ValueToken { get; } // O token que representa o literal (INT_LITERAL, STRING_LITERAL, etc.)
    public object Value { get; } // O valor parseado do literal

    public LiteralExpressionNode(Token valueToken) : base(valueToken)
    {
        ValueToken = valueToken;
        // Tenta converter o lexema para o tipo de valor apropriado
        switch (valueToken.Type)
        {
            case TokenType.INT_LITERAL:
                if (int.TryParse(valueToken.Lexeme, out var intValue)) Value = intValue;
                else throw new ArgumentException($"Valor inteiro inválido: {valueToken.Lexeme}");
                break;
            case TokenType.FLOAT_LITERAL:
                // Use InvariantCulture para garantir que o ponto decimal seja sempre '.'
                if (double.TryParse(valueToken.Lexeme, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var floatValue)) Value = floatValue;
                else throw new ArgumentException($"Valor float inválido: {valueToken.Lexeme}");
                break;
            case TokenType.CHAR_LITERAL:
                if (valueToken.Lexeme.Length == 1) Value = valueToken.Lexeme[0];
                else throw new ArgumentException($"Valor de caractere inválido: {valueToken.Lexeme}");
                break;
            case TokenType.STRING_LITERAL:
                Value = valueToken.Lexeme; // O lexema já é a string
                break;
            case TokenType.TRUE:
                Value = true;
                break;
            case TokenType.FALSE:
                Value = false;
                break;
            default:
                throw new ArgumentException($"Tipo de token não é um literal: {valueToken.Type}");
        }
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}