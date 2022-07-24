namespace Gaia.Symbols;

using System.Collections.Generic;
using Gaia.Inter;
using Gaia.Lex;

public class Env {
    private readonly Dictionary<Token, Id> table = new();
    protected Env? prev;

    public Env(Env? n) {
        prev = n;
    }

    public void Add(Token w, Id i) {
        table.Add(w, i);
    }

    public Id? Get(Token w) {
        for (var e = this; e is not null; e = e.prev) {
            if (e.table.TryGetValue(w, out var value)) {
                return value;
            }
        }

        return null;
    }
}
