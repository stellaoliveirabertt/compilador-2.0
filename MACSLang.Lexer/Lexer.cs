// MACSLang.Lexer/Lexer.cs

using System.Collections.Generic;
using System.Text;

namespace MACSLang.Lexer;

public class Lexer
{
    private readonly string _source; // O código fonte a ser analisado
    private int _position; // Posição atual no código fonte
    private int _line; // Linha atual no código fonte
    private int _column; // Coluna atual na linha

    // Mapeamento de palavras-chave para seus tipos de token
    private static readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "var", TokenType.VAR },
        { "func", TokenType.FUNC },
        { "int", TokenType.INT_KEYWORD },
        { "float", TokenType.FLOAT_KEYWORD },
        { "char", TokenType.CHAR_KEYWORD },
        { "bool", TokenType.BOOL_KEYWORD },
        { "string", TokenType.STRING_KEYWORD },
        { "if", TokenType.IF },
        { "else", TokenType.ELSE },
        { "while", TokenType.WHILE },
        { "for", TokenType.FOR },
        { "return", TokenType.RETURN },
        { "print", TokenType.PRINT },
        { "input", TokenType.INPUT },
        { "true", TokenType.TRUE },
        { "false", TokenType.FALSE }
    };

    public Lexer(string source)
    {
        _source = source;
        _position = 0;
        _line = 1;
        _column = 1;
    }

    // Propriedade para pegar o caractere atual
    private char CurrentChar => Peek(0);

    // Propriedade para verificar se chegou ao fim do código fonte
    private bool IsAtEnd => _position >= _source.Length;

    // Avança um caractere na leitura
    public void Advance()
    {
        _position++;
        _column++;
    }

    // Observa o próximo caractere sem avançar a posição
    private char Peek(int offset)
    {
        if (_position + offset >= _source.Length) return '\0'; // Caractere nulo para indicar fim
        return _source[_position + offset];
    }

    // Ignora espaços em branco, quebras de linha e comentários
    private void SkipWhitespaceAndComments()
    {
        while (true)
        {
            if (IsAtEnd) break;

            var c = CurrentChar;

            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    Advance();
                    break;
                case '\n':
                    Advance();
                    _line++;
                    _column = 1; // Reseta coluna na nova linha
                    break;
                case '/': // Pode ser início de comentário
                    if (Peek(1) == '/') // Comentário de linha //
                    {
                        while (!IsAtEnd && CurrentChar != '\n') Advance();
                        // Não precisa avançar para a próxima linha aqui,
                        // pois o loop principal vai chamar SkipWhitespaceAndComments novamente
                        // e tratar o '\n' ou o EOF.
                    }
                    else if (Peek(1) == '*') // Comentário de bloco /* ... */
                    {
                        Advance(); // Consome '/'
                        Advance(); // Consome '*'
                        while (!IsAtEnd)
                        {
                            if (CurrentChar == '*' && Peek(1) == '/')
                            {
                                Advance(); // Consome '*'
                                Advance(); // Consome '/'
                                break; // Sai do comentário de bloco
                            }

                            if (CurrentChar == '\n')
                            {
                                _line++;
                                _column = 1;
                            }

                            Advance();
                        }

                        if (IsAtEnd && !(Peek(-1) == '/' &&
                                         Peek(-2) == '*')) // Verifica se o comentário de bloco foi fechado
                        {
                            // Poderíamos gerar um erro aqui para comentário de bloco não fechado
                            // Por enquanto, vamos apenas parar.
                        }
                    }
                    else // Não é comentário, é apenas o caractere '/' (divisão)
                    {
                        return; // Sai da função para que o '/' seja tratado como operador
                    }

                    break;
                default:
                    return; // Não é espaço em branco ou comentário, então sai
            }
        }
    }


    // Gera o próximo token
    public Token NextToken()
    {
        SkipWhitespaceAndComments();

        if (IsAtEnd) return new Token(TokenType.EOF, "", _line, _column);

        var c = CurrentChar;
        var startLine = _line;
        var startColumn = _column;

        // Identificar tokens de um caractere
        switch (c)
        {
            case '=':
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(TokenType.EQUALS, "==", startLine, startColumn);
                }

                return new Token(TokenType.ASSIGN, "=", startLine, startColumn);
            case '+':
                Advance();
                return new Token(TokenType.PLUS, "+", startLine, startColumn);
            case '-':
                Advance();
                return new Token(TokenType.MINUS, "-", startLine, startColumn);
            case '*':
                Advance();
                return new Token(TokenType.MULTIPLY, "*", startLine, startColumn);
            case '/':
                Advance();
                return new Token(TokenType.DIVIDE, "/", startLine,
                    startColumn); // Tratado após skipWhitespace para não confundir com comentários
            case '%':
                Advance();
                return new Token(TokenType.MODULO, "%", startLine, startColumn);
            case '<':
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(TokenType.LESS_EQUAL, "<=", startLine, startColumn);
                }

                return new Token(TokenType.LESS_THAN, "<", startLine, startColumn);
            case '>':
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(TokenType.GREATER_EQUAL, ">=", startLine, startColumn);
                }

                return new Token(TokenType.GREATER_THAN, ">", startLine, startColumn);
            case '!':
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(TokenType.NOT_EQUALS, "!=", startLine, startColumn);
                }

                return new Token(TokenType.NOT, "!", startLine, startColumn); // '!' sozinho é operador NOT
            case '&':
                Advance();
                if (CurrentChar == '&')
                {
                    Advance();
                    return new Token(TokenType.AND, "&&", startLine, startColumn);
                }

                return new Token(TokenType.UNKNOWN, "&", startLine, startColumn); // '&' sozinho não é válido
            case '|':
                Advance();
                if (CurrentChar == '|')
                {
                    Advance();
                    return new Token(TokenType.OR, "||", startLine, startColumn);
                }

                return new Token(TokenType.UNKNOWN, "|", startLine, startColumn); // '|' sozinho não é válido
            case '(':
                Advance();
                return new Token(TokenType.OPEN_PAREN, "(", startLine, startColumn);
            case ')':
                Advance();
                return new Token(TokenType.CLOSE_PAREN, ")", startLine, startColumn);
            case '{':
                Advance();
                return new Token(TokenType.OPEN_BRACE, "{", startLine, startColumn);
            case '}':
                Advance();
                return new Token(TokenType.CLOSE_BRACE, "}", startLine, startColumn);
            case ';':
                Advance();
                return new Token(TokenType.SEMICOLON, ";", startLine, startColumn);
            case ':':
                Advance();
                return new Token(TokenType.COLON, ":", startLine, startColumn);
            case ',':
                Advance();
                return new Token(TokenType.COMMA, ",", startLine, startColumn);
        }

        // Identificar strings literais (ex: "Olá mundo!")
        if (c == '"')
        {
            Advance(); // Consome o '"' inicial
            var sb = new StringBuilder();
            while (CurrentChar != '"' && !IsAtEnd)
            {
                sb.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar != '"')
                // Erro: String não terminada
                return new Token(TokenType.UNKNOWN, sb.ToString(), startLine, startColumn);
            Advance(); // Consome o '"' final
            return new Token(TokenType.STRING_LITERAL, sb.ToString(), startLine, startColumn);
        }

        // Identificar char literais (ex: 'a')
        if (c == '\'')
        {
            Advance(); // Consome o '\'' inicial
            var charValue = CurrentChar;
            Advance(); // Consome o caractere
            if (CurrentChar != '\'')
                // Erro: Char literal não terminada ou mais de um caractere
                return new Token(TokenType.UNKNOWN, charValue.ToString(), startLine, startColumn);
            Advance(); // Consome o '\'' final
            return new Token(TokenType.CHAR_LITERAL, charValue.ToString(), startLine, startColumn);
        }


        // Identificar números (inteiros ou flutuantes)
        if (char.IsDigit(c))
        {
            var sb = new StringBuilder();
            while (char.IsDigit(CurrentChar))
            {
                sb.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar == '.') // Pode ser um float
            {
                sb.Append(CurrentChar); // Adiciona o '.'
                Advance(); // Avança para o próximo caractere após o '.'
                if (!char.IsDigit(CurrentChar))
                    // Erro: Ponto decimal sem dígito após (ex: "123.")
                    // Poderíamos retornar UNKNOWN ou tratar como INT se desejarmos
                    return new Token(TokenType.UNKNOWN, sb.ToString(), startLine, startColumn);
                while (char.IsDigit(CurrentChar))
                {
                    sb.Append(CurrentChar);
                    Advance();
                }

                return new Token(TokenType.FLOAT_LITERAL, sb.ToString(), startLine, startColumn);
            }

            return new Token(TokenType.INT_LITERAL, sb.ToString(), startLine, startColumn);
        }

        // Identificar identificadores e palavras-chave
        if (char.IsLetter(c) || c == '_')
        {
            var sb = new StringBuilder();
            while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
            {
                sb.Append(CurrentChar);
                Advance();
            }

            var text = sb.ToString();

            // Verifica se é uma palavra-chave, caso contrário, é um identificador
            if (_keywords.TryGetValue(text, out var type)) return new Token(type, text, startLine, startColumn);
            return new Token(TokenType.IDENTIFIER, text, startLine, startColumn);
        }

        // Se nada mais corresponder, é um caractere desconhecido (erro)
        Advance();
        return new Token(TokenType.UNKNOWN, c.ToString(), startLine, startColumn);
    }
}