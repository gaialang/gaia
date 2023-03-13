using Gaia.AST;

namespace Gaia.Compiler;

public class Emitter : Visitor<string, object?> {
    public string Visit(PackageNode n, object? ctx = null) {
        Console.WriteLine($"pkg is {n.Name};\n");
        foreach (var expr in n.ExprList) {
            expr.Accept(this, ctx);
        }
        return "";
    }

    public string Visit(IdNode n, object? ctx = null) {
        return n.Name;
    }

    public string Visit(VarAssignNode n, object? ctx = null) {
        if (n.IdNode.IdType == IdType.Int) {
            var name = n.IdNode.Accept(this, ctx);
            if (n.Expr is null) {
                Console.WriteLine($"int {name};");
            } else {
                var val = n.Expr.Accept(this, ctx);
                Console.WriteLine($"int {name} = {val};");
            }
        }
        return "";
    }

    public string Visit(UnaryNode n, object? ctx = null) {
        var operand = n.Operand.Accept(this, ctx);
        return $"{n.Operator.Lexeme}{operand};";
    }

    public string Visit(IntLiteralNode n, object? ctx = null) {
        return n.Lexeme;
    }

    public string Visit(FuncNode n, object? ctx = null) {
        return "";
    }

    public string Visit(WhileNode n, object? ctx = null) {
        return "";
    }
}
