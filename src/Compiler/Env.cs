using Gaia.AST;

namespace Gaia.Compiler;

public class Env {
    private readonly Dictionary<string, Identifier> table = new();
    private Env? prev;

    public Env(Env? n) {
        prev = n;
    }

    public void Add(string s, Identifier id) {
        table.Add(s, id);
    }

    public Identifier? Get(string s) {
        for (var e = this; e is not null; e = e.prev) {
            if (e.table.TryGetValue(s, out var value)) {
                return value;
            }
        }

        return null;
    }
}
