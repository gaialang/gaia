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
        // include headers.
        writer.WriteLine("#include <stdio.h>");
        writer.WriteLine("#include <stdbool.h>");
        writer.WriteLine();

        foreach (var expr in pkg.Statements) {
            expr.Accept(this, ctx);
        }
        return "";
    }

    public string Visit(ImportDeclaration node, object? ctx = null) {
        writer.WriteLine($"#include <{node.ModuleSpecifier}.h>");
        return "";
    }

    public string Visit(Identifier id, object? ctx = null) {
        return id.Name;
    }

    public string Visit(VariableDeclaration s, object? ctx = null) {
        var t = ToCType(s.Name);
        if (s.Initializer is null) {
            writer.WriteLine($"{t};");
        } else {
            var val = s.Initializer.Accept(this, ctx);
            writer.WriteLine($"{t} = {val};");
        }
        return "";
    }

    private static string ToCType(Identifier id) {
        var t = id.TypeInfo;
        if (t is ArrayType) {
            return ConvertArray(id);
        }
        if (t is IndexedAccessType) {
            return ConvertIndexedAccess(id);
        }

        var s = ConvertPrimitive(t);
        return $"{s} {id.Name}";
    }

    private static string ConvertArray(Identifier id) {
        var sb = new StringBuilder();
        var arr = id.TypeInfo;

        do {
            var x = (ArrayType)arr;
            sb.Insert(0, "[]");
            arr = x.ElementType;
        } while (arr is ArrayType);

        var primitive = arr;

        var pre = ConvertPrimitive(primitive);
        var post = string.Join("", sb);
        return $"{pre} {id.Name}{post}";
    }

    private static string ConvertIndexedAccess(Identifier id) {
        var sb = new StringBuilder();
        var arr = id.TypeInfo;

        do {
            var x = (IndexedAccessType)arr;
            sb.Insert(0, $"[{x.IndexType}]");
            arr = x.ObjectType;
        } while (arr is IndexedAccessType);

        var obj = arr;
        var pre = ConvertPrimitive(obj);
        var post = string.Join("", sb);
        return $"{pre} {id.Name}{post}";
    }

    private static string ConvertPrimitive(TypeInfo t) {
        var map = new Dictionary<TypeKind, string> {
            {TypeKind.Int, "int"}
        };
        return map[t.Kind];
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

    public string Visit(IntLiteral node, object? ctx = null) {
        return node.Text;
    }

    public string Visit(FloatLiteral node, object? ctx = null) {
        return node.Text;
    }

    public string Visit(FunctionDeclaration fn, object? ctx = null) {
        var list = new List<string>();
        foreach (var item in fn.Parameters) {
            var t = ConvertPrimitive(item.TypeInfo);
            list.Add($"{t} {item.Name}");
        }
        var args = string.Join(", ", list);

        var ret = ConvertPrimitive(fn.ReturnType);
        var name = fn.Name.Accept(this, ctx);
        writer.WriteLine();
        writer.WriteLine($"{ret} {name}({args}) {{");
        indent++;
        fn.Body?.Accept(this, ctx);
        writer.WriteLine("}");
        indent--;
        return "";
    }

    public string Visit(WhileStatement node, object? ctx = null) {
        var expr = node.Expression.Accept(this, ctx);
        writer.WriteLine($"while ({expr}) {{");
        indent++;
        node.Body.Accept(this, ctx);
        indent--;
        writer.Write(Indent());
        writer.WriteLine("}");
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
        var name = node.Left.Accept(this, ctx);
        var val = node.Right.Accept(this, ctx);
        writer.WriteLine($"{name} = {val};");
        return "";
    }

    public string Visit(Block node, object? ctx = null) {
        foreach (var stmt in node.Statements) {
            writer.Write(Indent());
            stmt.Accept(this, ctx);
        }

        return "";
    }

    public string Visit(BoolLiteral node, object? ctx = null) {
        return node.Text;
    }

    public string Visit(StringLiteral node, object? ctx = null) {
        return $"\"{node.Text}\"";
    }

    public string Visit(BreakStatement node, object? ctx = null) {
        writer.WriteLine("break;");
        return "";
    }

    public string Visit(IfStatement node, object? ctx = null) {
        var expr = node.Expression.Accept(this, ctx);
        writer.WriteLine($"if ({expr}) {{");
        indent++;
        node.ThenStatement.Accept(this, ctx);
        if (node.ElseStatement is null) {
            indent--;
            writer.Write(Indent());
            writer.WriteLine("}");
        } else {
            indent--;
            writer.Write(Indent());
            writer.Write("} ");
            node.ElseStatement.Accept(this, ctx);
        }
        return "";
    }

    public string Visit(ElementAssignStatement node, object? ctx = null) {
        var l = node.Left.Accept(this, ctx);
        var r = node.Right.Accept(this, ctx);
        writer.WriteLine($"{l} = {r};");
        return "";
    }

    public string Visit(ElementAccessExpression expr, object? ctx = null) {
        var l = expr.Expression.Accept(this, ctx);
        var index = expr.ArgumentExpression.Accept(this, ctx);
        return $"{l}[{index}]";
    }

    public string Visit(DoStatement node, object? ctx = null) {
        writer.WriteLine("do {");
        indent++;
        node.Body.Accept(this, ctx);
        indent--;
        writer.Write(Indent());
        var expr = node.Expression.Accept(this, ctx);
        writer.WriteLine($"}} while ({expr});");
        return "";
    }

    public string Visit(CallExpression node, object? ctx = null) {
        var expr = node.Expression.Accept(this, ctx);

        var list = new List<string>();
        foreach (var item in node.Arguments) {
            var s = item.Accept(this, ctx);
            list.Add(s);
        }
        var args = string.Join(", ", list);

        writer.WriteLine($"{expr}({args});");
        return "";
    }

    private string Indent() {
        var sb = new StringBuilder();
        for (var i = 0; i < indent; i++) {
            sb.Append("    ");
        }
        return sb.ToString();
    }
}
