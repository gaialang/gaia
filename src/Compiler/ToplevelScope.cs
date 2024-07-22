using Gaia.AST;
using Gaia.Domain;

namespace Gaia.Compiler;

public class ToplevelScope {
    private Scope currentScope = new Scope();
    private Scope? savedScope = null;

    public ToplevelScope() {
        // built in
        currentScope.Add("printf", new FunctionEntity(
            new KeywordLikeNode(SyntaxKind.VoidKeyword, -1, -1),
            new List<Parameter>() {
                new Parameter(new Identifier("format",-1,-1), new KeywordLikeNode(SyntaxKind.StringKeyword,-1,-1))
            })
        );
    }

    public void Add(string s, Entity e) {
        currentScope.Add(s, e);
    }

    public Entity? Get(string s) {
        return currentScope.Get(s);
    }

    // Get current level of environment only.
    public Entity? GetLocal(string s) {
        return currentScope.GetLocal(s);
    }

    public bool Contains(string s) {
        return currentScope.Get(s) is not null;
    }

    public bool ContainsLocal(string s) {
        return currentScope.GetLocal(s) is not null;
    }

    public void EnterScope() {
        savedScope = currentScope;
        currentScope = new Scope(currentScope);
    }

    public void ExitScope() {
        currentScope = savedScope ?? throw new CheckError("Scope not saved");
        savedScope = null;
    }
}
