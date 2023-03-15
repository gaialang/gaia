using Gaia.Compiler;

namespace Gaia.AST;

public sealed class BinaryOpNode : Expr {
    public BinaryOpNode(Token token, Expr lhs, Expr rhs) {
        OperatorToken = token;
        switch (token.Type) {
        case TokenType.Plus:
            NodeType = NodeType.Add;
            break;
        case TokenType.Minus:
            NodeType = NodeType.Subtract;
            break;
        case TokenType.Multiply:
            NodeType = NodeType.Multiply;
            break;
        case TokenType.Divide:
            NodeType = NodeType.Divide;
            break;
        case TokenType.LessThan:
            NodeType = NodeType.LessThan;
            break;
        case TokenType.EqualEqual:
            NodeType = NodeType.EqualEqual;
            break;
        case TokenType.Id:
            NodeType = NodeType.Id;
            break;
        case TokenType.IntLiteral:
            NodeType = NodeType.IntLiteral;
            break;
        default:
            throw new Exception("op " + token.Type + " is not a valid operator");
        }

        Lhs = lhs;
        Rhs = rhs;
    }

    public Expr Lhs { get; }
    public Expr Rhs { get; }
    public Token OperatorToken { get; }
    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
