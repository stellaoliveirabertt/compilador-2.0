# Compilador MACSLang

Este é um projeto de desenvolvimento de um compilador para a linguagem de programação MACSLang, conforme as especificações do Professor Marco Antônio. O compilador é implementado em C# e abrange as principais fases de um processo de compilação: Análise Léxica, Análise Sintática, Análise Semântica e Geração de Código.

## Sumário

1.  [Pré-requisitos](#pré-requisitos)
2.  [Como Iniciar o Projeto](#como-iniciar-o-projeto)
3.  [Estrutura do Projeto](#estrutura-do-projeto)
4.  [Fases do Compilador MACSLang](#fases-do-compilador-macslang)
    * [Analisador Léxico (Lexer)](#analisador-léxico-lexer)
    * [Analisador Sintático (Parser)](#analisador-sintático-parser)
    * [Analisador Semântico](#analisador-semântico)
    * [Gerador de Código](#gerador-de-código)
5.  [Exemplos de Código MACSLang](#exemplos-de-código-macslang)
6.  [Próximos Passos e Melhorias](#próximos-passos-e-melhorias)

---

## Pré-requisitos

Para executar este projeto, você precisará ter o [.NET SDK](https://dotnet.microsoft.com/download) instalado em sua máquina. A versão utilizada e testada no projeto gerado é **.NET 6.0**, mas você pode ter uma versão mais recente do SDK que suporte a compilação para `net6.0`.

* [.NET SDK 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) ou superior.

## Como Iniciar o Projeto

Siga os passos abaixo para compilar e executar o compilador MACSLang:

1.  **Navegue até o diretório raiz do projeto:**
    Abra seu terminal ou prompt de comando e vá para a pasta `MACSLangCompiler` (onde o arquivo `MACSLangCompiler.sln` está localizado).

    ```bash
    cd /caminho/para/seu/projeto/MACSLangCompiler
    ```

2.  **Compile a solução:**
    Este comando irá compilar todos os projetos da solução (`MACSLang.Lexer`, `MACSLang.Syntax`, `MACSLang.Parser`, `MACSLang.SemanticAnalyzer`, `MACSLang.CodeGenerator`, e `MACSLang.Compiler`).

    ```bash
    dotnet build
    ```

3.  **Execute o compilador:**
    Este comando executará o projeto `MACSLang.Compiler`, que é a nossa interface de linha de comando para testar as fases.

    ```bash
    dotnet run --project MACSLang.Compiler
    ```

4.  **Interaja com o menu:**
    Após a execução, um menu será exibido no console, permitindo que você escolha qual fase do compilador deseja testar:
    ```
    Bem-vindo ao Compilador MACSLang!

    --------------------------------
    Escolha a fase do compilador para testar:
    1. Analisador Léxico (Lexer)
    2. Analisador Sintático (Parser)
    3. Analisador Semântico
    4. Gerador de Código
    0. Sair
    Sua escolha:
    ```
    * **Opção 1 (Analisador Léxico):** Exibe a sequência de tokens do código de exemplo.
    * **Opção 2 (Analisador Sintático):** Permite escolher entre um código sintaticamente correto ou um com erro para verificar a construção da AST.
    * **Opção 3 (Analisador Semântico):** Permite escolher entre um código semanticamente correto ou um com erros para verificar a validação de tipos e escopo.
    * **Opção 4 (Gerador de Código):** Gera o código C# a partir do código MACSLang (apenas para o código correto), salva-o em um arquivo temporário, tenta compilá-lo e executá-lo. Esta é a demonstração completa do compilador.

---

## Estrutura do Projeto

O projeto é organizado em módulos (bibliotecas de classes) para cada fase principal do compilador, mantendo o código limpo e modular.

```text

    MACSLangCompiler/
    ├── MACSLangCompiler.sln         \# Arquivo de solução principal
    ├── MACSLang.Compiler/           \# Projeto de Console (interface principal)
    │   ├── Program.cs               \# Lógica do menu e orquestração das fases
    │   ├── MACSLang.Compiler.csproj
    │   └── ...
    ├── MACSLang.Lexer/              \# Módulo do Analisador Léxico
    │   ├── Lexer.cs                 \# Implementação do scanner
    │   ├── Token.cs                 \# Representação de um token
    │   ├── TokenType.cs             \# Enumeração dos tipos de tokens
    │   └── MACSLang.Lexer.csproj
    ├── MACSLang.Syntax/             \# Módulo da Árvore Sintática Abstrata (AST)
    │   ├── AST/                     \# Pasta para os nós da AST
    │   │   ├── AstNode.cs           \# Classe base abstrata para nós da AST
    │   │   ├── IAstVisitor.cs       \# Interface para o padrão Visitor
    │   │   ├── ProgramNode.cs       \# Nó raiz da AST
    │   │   ├── FunctionDeclarationNode.cs \# Declaração de função e parâmetros
    │   │   ├── TypeNode.cs          \# Nós para tipos de dados
    │   │   ├── StatementNode.cs     \# Classe base para comandos
    │   │   ├── BlockStatementNode.cs \# Bloco de código ({...})
    │   │   ├── VariableDeclarationNode.cs \# Declaração de variável
    │   │   ├── AssignmentStatementNode.cs \# Atribuição
    │   │   ├── PrintStatementNode.cs \# Comando print
    │   │   ├── InputStatementNode.cs \# Comando input§
    │   │   ├── ReturnStatementNode.cs \# Comando return
    │   │   ├── IfStatementNode.cs   \# Comando if-else
    │   │   ├── WhileStatementNode.cs \# Laço while
    │   │   ├── ForStatementNode.cs  \# Laço for
    │   │   ├── ExpressionStatementNode.cs \# Expressão como comando
    │   │   ├── ExpressionNode.cs    \# Classe base para expressões
    │   │   ├── LiteralExpressionNode.cs \# Literais (números, strings, etc.)
    │   │   ├── IdentifierExpressionNode.cs \# Identificadores
    │   │   ├── BinaryExpressionNode.cs \# Expressões binárias (+, -, ==, etc.)
    │   │   ├── UnaryExpressionNode.cs \# Expressões unárias (\!, -)
    │   │   └── FunctionCallExpressionNode.cs \# Chamada de função
    │   └── MACSLang.Syntax.csproj
    ├── MACSLang.Parser/             \# Módulo do Analisador Sintático
    │   ├── Parser.cs                \# Lógica de parsing para construir a AST
    │   └── MACSLang.Parser.csproj
    ├── MACSLang.SemanticAnalyzer/   \# Módulo do Analisador Semântico
    │   ├── SemanticAnalyzer.cs      \# Implementação do visitor para análise semântica
    │   ├── Symbol.cs                \# Classe base para símbolos
    │   ├── VariableSymbol.cs        \# Símbolo de variável
    │   ├── FunctionSymbol.cs        \# Símbolo de função
    │   ├── SymbolTable.cs           \# Gerenciador de escopo e símbolos
    │   ├── SemanticError.cs         \# Exceção para erros semânticos
    │   └── MACSLang.SemanticAnalyzer.csproj
    └── MACSLang.CodeGenerator/      \# Módulo do Gerador de Código
    ├── CodeGenerator.cs         \# Implementação do visitor para gerar C\#
    └── MACSLang.CodeGenerator.csproj
```
## Fases do Compilador MACSLang

Construímos o compilador em fases modulares, cada uma com sua responsabilidade específica:

### Analisador Léxico (Lexer)

* **Função:** O Lexer é a primeira fase. Ele lê o código fonte da MACSLang caractere por caractere e o agrupa em pequenas unidades significativas chamadas **tokens**. Ignora espaços em branco e comentários.
* **Implementação:** Classe `Lexer.cs`. Utiliza expressões regulares (ou lógica manual de máquina de estados) para identificar palavras-chave, identificadores, literais, operadores e delimitadores.
* **Saída:** Uma sequência (lista) de objetos `Token`, cada um contendo o tipo do token (ex: `INT_KEYWORD`, `IDENTIFIER`) e seu valor (lexema, ex: "int", "numero"), além da linha e coluna no código fonte para facilitar a depuração.

### Analisador Sintático (Parser)

* **Função:** O Parser recebe a sequência de tokens do Lexer e verifica se eles formam uma estrutura gramaticalmente válida de acordo com as regras da MACSLang. Se a sintaxe estiver correta, ele constrói uma representação hierárquica do programa chamada **Árvore Sintática Abstrata (AST)**.
* **Implementação:** Classe `Parser.cs`. Utiliza a técnica de "Parser Recursivo Descendente", onde cada regra gramatical da MACSLang (ex: declaração de função, if-else, laços) é implementada como um método recursivo.
* **Saída:** Uma instância de `ProgramNode`, que é o nó raiz da AST. A AST representa a estrutura do código MACSLang de forma organizada e fácil de percorrer. Erros sintáticos são reportados como `ParseException`.

### Analisador Semântico

* **Função:** O Analisador Semântico é a fase que verifica o "significado" do programa. Ele garante que o código não só está gramaticalmente correto, mas também faz sentido. Isso inclui verificação de tipos, declaração de variáveis/funções, uso de escopos e consistência de chamadas de função.
* **Implementação:** Classe `SemanticAnalyzer.cs`. Ela implementa a interface `IAstVisitor`, que permite percorrer todos os nós da AST. Utiliza uma `SymbolTable` para rastrear os identificadores (variáveis, funções) e seus tipos e escopos.
* **Saída:** Uma AST "anotada" com informações de tipo (a propriedade `ExpressionType` em nós de expressão) e a garantia de que o programa é semanticamente válido. Erros semânticos são reportados como `SemanticError`.

### Gerador de Código

* **Função:** A fase final. Ela pega a AST (agora validada léxica, sintática e semanticamente) e a traduz para um código em outra linguagem ou para código de máquina. Neste projeto, o Gerador de Código emite **código C# equivalente** ao código MACSLang original.
* **Implementação:** Classe `CodeGenerator.cs`. Também implementa a interface `IAstVisitor`, percorrendo a AST e emitindo as linhas de código C# correspondentes para cada nó. Ele lida com a tradução de tipos, operadores e estruturas de controle.
* **Saída:** Uma `string` contendo o código C# completo que pode ser compilado e executado por um compilador .NET padrão. O projeto inclui lógica para criar um projeto .NET temporário, compilar esse código C# e executá-lo, demonstrando o resultado final do compilador MACSLang.

---

## Exemplos de Código MACSLang

O arquivo `MACSLang.Compiler/Program.cs` contém exemplos de código MACSLang para teste:

* `_correctSourceCode`: Um exemplo de programa MACSLang com a função `fatorial` e uma função `main` que a utiliza, projetado para passar por todas as fases do compilador sem erros.
* `_syntaxErrorSourceCode`: Um exemplo com um erro sintático proposital (`var x: int = ;`) para testar a detecção de erros do Analisador Sintático.
* `_semanticErrorSourceCode`: Um exemplo com vários erros semânticos (variável não declarada, tipos incompatíveis, função duplicada, etc.) para testar a detecção de erros do Analisador Semântico.

Você pode interagir com o menu do `Program.cs` para testar cada um desses cenários.

---

## Próximos Passos e Melhorias

Este compilador é uma base sólida e funcional para a MACSLang. No entanto, compiladores reais são muito mais complexos e podem incluir diversas melhorias:

* **Mensagens de Erro Aprimoradas:** Fornecer mensagens de erro mais detalhadas e amigáveis, com sugestões de correção.
* **Otimização de Código:** Adicionar fases para otimizar a AST ou o código gerado para melhorar performance.
* **Tratamento de Erros em Tempo de Execução (Runtime):** O código C# gerado atualmente não trata erros de entrada como `FormatException` (ao digitar texto em vez de número). Implementar `try-catch` ou loops de validação no código C# gerado para tornar os programas MACSLang mais robustos.
* **Bibliotecas Padrão:** Expandir as funcionalidades de I/O e adicionar outras funções úteis.
* **Mais Tipos e Estruturas:** Adicionar suporte a arrays, estruturas de dados personalizadas (structs/classes), enums, etc.
* **Geração de Assembly Direta:** Para um compilador de produção, a geração de Assembly x86 ou outro código de máquina diretamente seria o próximo passo (mais complexo).
* **Testes Automatizados:** Implementar testes unitários e de integração para todas as fases do compilador.