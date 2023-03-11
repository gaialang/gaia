using Gaia.AST;

namespace Gaia.Compiler;

public class Env {
    private readonly Dictionary<string, IdNode> table = new();
    private Env? prev;

    public Env(Env? n) {
        prev = n;
    }

    public void Add(string s, IdNode id) {
        table.Add(s, id);
    }

    public bool TryGet(string s, out IdNode? value) {
        for (var e = this; e is not null; e = e.prev) {
            if (e.table.TryGetValue(s, out value)) {
                return true;
            }
        }

        value = null;
        return false;
    }
}
