﻿namespace Gaia.AST;

public class AssignStatement : Statement {
    public Identifier Left { get; }
    public Expression Right { get; }

    public AssignStatement(Identifier l, Expression r) {
        Left = l;
        Right = r;
        Kind = SyntaxKind.AssignStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
