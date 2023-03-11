namespace Gaia.AST;

public sealed class IntLiteralNode : Node {
    public IntLiteralNode(string lexeme, int value) {
        Lexeme = lexeme;
        Value = value;
        NodeType = NodeType.IntLiteral;
    }

    public string Lexeme { get; private set; }
    public double Value { get; private set; }

    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
