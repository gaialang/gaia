namespace Gaia.Compiler;

public class Scope {
    private readonly Dictionary<string, Entity> identifiers = new();
    private Scope? parent = null;

    public Scope() {
    }

    public Scope(Scope scope) {
        parent = scope;
    }

    public void Add(string s, Entity e) {
        identifiers.Add(s, e);
    }

    public Entity? Get(string s) {
        for (var env = this; env is not null; env = env.parent) {
            if (env.identifiers.TryGetValue(s, out var value)) {
                return value;
            }
        }

        return null;
    }

    public Entity? GetLocal(string s) {
        if (identifiers.TryGetValue(s, out var value)) {
            return value;
        }

        return null;
    }
}
