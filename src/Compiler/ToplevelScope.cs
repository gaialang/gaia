using Gaia.AST;
using Gaia.Domain;

namespace Gaia.Compiler;

public class ToplevelScope {
    private Scope currentScope = new Scope();
    private Scope? savedScope = null;

    public ToplevelScope() {
        // built in
        currentScope.Add("printf", new FunctionEntity(
            new KeywordLikeNode(SyntaxKind.VoidKeyword),
            new List<Parameter>() {
                new Parameter(new Identifier("format"), new KeywordLikeNode(SyntaxKind.StringKeyword))
            })
        );
    }

    public void Add(string s, Entity e) {
        currentScope.Add(s, e);
    }

    public Entity? Get(string s) {
        return currentScope.Get(s);
    }

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
