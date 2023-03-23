namespace Gaia.AST;

public class FuncCall {
    public FuncCall(string callee, List<Expression> args) {
        Callee = callee;
        Arguments = args;
    }

    public string Callee { get; private set; }
    public List<Expression> Arguments { get; private set; }

    /*
        public override SyntaxKind Kind { get; protected set; }

        public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
            return visitor.Visit(this, ctx);
        }
        */
}
