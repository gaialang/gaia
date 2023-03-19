using System.Text;
using Gaia.AST;

namespace Gaia.Compiler;

public class Emitter : Visitor<string, object?> {
    private Writer writer;
    private int indent = 0;

    public Emitter(Writer writer) {
        this.writer = writer;
    }

    public string Visit(PackageStmt pkg, object? ctx = null) {
        writer.WriteLine($"pkg is {pkg.Name};\n");
        foreach (var expr in pkg.VarOrFuncStatements) {
            expr.Accept(this, ctx);
        }
        return "";
    }

    public string Visit(Identifier id, object? ctx = null) {
        return id.Name;
    }

    public string Visit(VarStmt s, object? ctx = null) {
        if (s.Id.IdType == IdType.Int) {
            var name = s.Id.Accept(this, ctx);
            if (s.Expr is null) {
                writer.WriteLine($"int {name};");
            } else {
                var val = s.Expr.Accept(this, ctx);
                writer.WriteLine($"int {name} = {val};");
            }
        }
        return "";
    }

    public string Visit(UnaryOpExpr n, object? ctx = null) {
        var operand = n.Operand.Accept(this, ctx);
        return $"{n.Operator.Lexeme}{operand}";
    }

    public string Visit(BinaryOpExpr n, object? ctx = null) {
        var lhs = n.Lhs.Accept(this, ctx);
        var rhs = n.Rhs.Accept(this, ctx);
        return $"{lhs} {n.Operator.Lexeme} {rhs}";
    }

    public string Visit(IntLiteral i, object? ctx = null) {
        return i.Lexeme;
    }

    public string Visit(FuncStmt fn, object? ctx = null) {
        var ret = Util.ConvertType(fn.ReturnType);
        var list = new List<string>();
        foreach (var item in fn.Arguments) {
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

    public string Visit(WhileStmt n, object? ctx = null) {
        return "";
    }

    public string Visit(AssignStmt n, object? ctx = null) {
        var indentString = GetIndent();
        writer.Write(indentString);
        // TODO:
        return "";
    }

    public string Visit(StmtList s, object? ctx = null) {
        s.Head?.Accept(this, ctx);
        s.Tail?.Accept(this, ctx);

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
