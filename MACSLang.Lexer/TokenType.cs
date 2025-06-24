// MACSLang.Lexer/TokenType.cs

namespace MACSLang.Lexer;

public enum TokenType
{
    // Palavras-chave
    VAR, // var
    FUNC, // func
    INT_KEYWORD, // int
    FLOAT_KEYWORD, // float
    CHAR_KEYWORD, // char
    BOOL_KEYWORD, // bool
    STRING_KEYWORD, // string
    IF, // if
    ELSE, // else
    WHILE, // while
    FOR, // for
    RETURN, // return
    PRINT, // print
    INPUT, // input
    TRUE, // true
    FALSE, // false

    // Operadores
    ASSIGN, // =
    PLUS, // +
    MINUS, // -
    MULTIPLY, // *
    DIVIDE, // /
    MODULO, // % (não explícito, mas comum em linguagens imperativas)
    EQUALS, // ==
    NOT_EQUALS, // != (não explícito, mas comum)
    LESS_THAN, // <
    GREATER_THAN, // >
    LESS_EQUAL, // <=
    GREATER_EQUAL, // >=
    AND, // && (não explícito, mas comum)
    OR, // || (não explícito, mas comum)
    NOT, // ! (não explícito, mas comum)

    // Delimitadores/Pontuação
    OPEN_PAREN, // (
    CLOSE_PAREN, // )
    OPEN_BRACE, // {
    CLOSE_BRACE, // }
    SEMICOLON, // ;
    COLON, // :
    COMMA, // ,

    // Literais
    IDENTIFIER, // Nomes de variáveis, funções
    INT_LITERAL, // 123, 0, -45
    FLOAT_LITERAL, // 123.45, 0.0
    CHAR_LITERAL, // 'a', 'Z', '5'
    STRING_LITERAL, // "Olá mundo!"

    // Fim de Arquivo
    EOF, // End Of File

    // Erro
    UNKNOWN // Para caracteres ou sequências não reconhecidas
}