using Gaia.Compiler;

namespace Gaia.AST;

public sealed class BinaryExpression : Expression {
    public BinaryExpression(Token token, Expression left, Expression right) {
        OperatorToken = token;
        Left = left;
        Right = right;
        Kind = SyntaxKind.BinaryExpression;
    }

    public Expression Left { get; }
    public Expression Right { get; }
    public Token OperatorToken { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
