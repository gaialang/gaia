using System.Text;
using Gaia.AST;

namespace Gaia.Compiler;

public class Emitter : Visitor<string, object?> {
    private Writer writer;
    private int indent = 0;

    public Emitter(Writer writer) {
        this.writer = writer;
    }

    public string Visit(PackageDeclaration pkg, object? ctx = null) {
        foreach (var expr in pkg.Statements) {
            expr.Accept(this, ctx);
        }
        return "";
    }

    public string Visit(ImportDeclaration node, object? ctx = null) {
        writer.WriteLine($"#include <{node.ModuleSpecifier}.h>\n");
        return "";
    }

    public string Visit(Identifier id, object? ctx = null) {
        return id.Name;
    }

    public string Visit(VariableDeclaration s, object? ctx = null) {
        var t = Util.ConvertType(s.Id.IdType);
        var name = s.Id.Accept(this, ctx);
        if (s.Expr is null) {
            writer.WriteLine($"{t} {name};");
        } else {
            var val = s.Expr.Accept(this, ctx);
            writer.WriteLine($"{t} {name} = {val};");
        }
        return "";
    }

    public string Visit(UnaryExpression n, object? ctx = null) {
        var operand = n.Operand.Accept(this, ctx);
        return $"{n.Operator.Lexeme}{operand}";
    }

    public string Visit(BinaryExpression n, object? ctx = null) {
        var lhs = n.Left.Accept(this, ctx);
        var rhs = n.Right.Accept(this, ctx);
        return $"{lhs} {n.OperatorToken.Lexeme} {rhs}";
    }

    public string Visit(IntLiteral i, object? ctx = null) {
        return i.Lexeme;
    }

    public string Visit(FunctionDeclaration fn, object? ctx = null) {
        var ret = Util.ConvertType(fn.ReturnType);
        var list = new List<string>();
        foreach (var item in fn.Parameters) {
            var t = Util.ConvertType(item.IdType);
            list.Add($"{t} {item.Name}");
        }
        var args = string.Join(", ", list);

        writer.WriteLine();
        writer.WriteLine($"{ret} {fn.Name}({args}) {{");
        indent++;
        fn.Body?.Accept(this, ctx);
        indent--;
        writer.WriteLine("}");
        return "";
    }

    public string Visit(WhileStatement node, object? ctx = null) {
        return "";
    }

    public string Visit(ReturnStatement node, object? ctx = null) {
        var s = node.Expression?.Accept(this, ctx);
        if (s is null) {
            writer.WriteLine($"return;");
        } else {
            writer.WriteLine($"return {s};");
        }
        return "";
    }

    public string Visit(AssignStatement node, object? ctx = null) {
        var name = node.LValue.Accept(this, ctx);
        var val = node.RValue.Accept(this, ctx);
        writer.WriteLine($"{name} = {val};");
        return "";
    }

    public string Visit(Block node, object? ctx = null) {
        foreach (var stmt in node.Statements) {
            var indentString = GetIndent();
            writer.Write(indentString);
            stmt.Accept(this, ctx);
        }

        return "";
    }

    private string GetIndent() {
        var sb = new StringBuilder();
        for (var i = 0; i < indent; i++) {
            sb.Append("    ");
        }
        return sb.ToString();
    }
}
