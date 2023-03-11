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

    public string Visit(VarNode n, object? ctx = null) {
        if (n.IdNode.IdType == IdType.Int) {
            var name = n.IdNode.Accept(this, ctx);
            if (n.Expr is null) {

                Console.WriteLine($"int {name};");
            } else {
                Console.Write($"int {name} = ");
                n.Expr?.Accept(this, ctx);
                Console.WriteLine(";");
            }
        }
        return "";
    }

    public string Visit(UnaryNode n, object? ctx = null) {
        var operand = n.Operand.Accept(this, ctx);
        Console.WriteLine($"{n.Operator.Lexeme}{operand};");
        return "";
    }

    public string Visit(IntLiteralNode n, object? ctx = null) {
        Console.Write(n.Lexeme);
        return "";
    }
}
