// MACSLang.SemanticAnalyzer/SymbolTable.cs

using System.Collections.Generic;
using MACSLang.Lexer; // Para TokenType

namespace MACSLang.SemanticAnalyzer;

// Gerencia o escopo e os símbolos (variáveis, funções)
public class SymbolTable
{
    public Dictionary<string, Symbol> Symbols { get; } // Símbolos no escopo atual
    public SymbolTable ParentScope { get; } // Escopo pai (para encadeamento de escopos)
    public int ScopeLevel { get; } // Nível do escopo (0 para global, 1 para função, etc.)

    public SymbolTable(SymbolTable parentScope = null)
    {
        Symbols = new Dictionary<string, Symbol>();
        ParentScope = parentScope;
        ScopeLevel = (parentScope?.ScopeLevel ?? -1) + 1; // Nível do escopo
    }

    // Adiciona um símbolo ao escopo atual
    public void Define(Symbol symbol)
    {
        if (Symbols.ContainsKey(symbol.Name))
            // Este erro deve ser pego na verificação semântica
            throw new SemanticError($"Símbolo '{symbol.Name}' já definido neste escopo.");
        Symbols[symbol.Name] = symbol;
    }

    // Busca um símbolo, começando no escopo atual e subindo para os pais
    public Symbol Resolve(string name)
    {
        if (Symbols.TryGetValue(name, out var symbol)) return symbol;

        // Se não encontrou no escopo atual, tenta no escopo pai
        if (ParentScope != null) return ParentScope.Resolve(name);

        // Símbolo não encontrado em nenhum escopo
        return null; // Retorna null se não encontrado
    }
}