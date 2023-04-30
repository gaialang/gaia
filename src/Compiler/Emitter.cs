using System.Text;
using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

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

        // TODO: optimize indentations.
        var kinds = new HashSet<SyntaxKind>() { SyntaxKind.FunctionDeclaration, SyntaxKind.VariableDeclaration, SyntaxKind.StructKeyword };
        foreach (var stmt in pkg.Statements) {
            if (kinds.Contains(stmt.Kind)) {
                writer.WriteLine();
            }
            stmt.Accept(this, ctx);
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
        var t = CType(s);
        if (s.Initializer is null) {
            writer.WriteLine($"{t};");
        } else {
            var val = s.Initializer.Accept(this, ctx);
            writer.WriteLine($"{t} = {val};");
        }
        return "";
    }

    private static string CType(VariableDeclaration decl) {
        var t = decl.Type;
        if (t is ArrayType) {
            return ConvertArray(decl);
        }
        if (t is IndexedAccessType) {
            return ConvertIndexedAccess(decl);
        }

        var st = TokenStrings[t.Kind];
        return $"{decl} {decl.Name.Name}";
    }

    private static string CPrimitive(SyntaxKind kind) {
        if (kind is SyntaxKind.StringKeyword) {
            return "char*";
        }
        return TokenStrings[kind];
    }

    private static string ConvertArray(VariableDeclaration decl) {
        var sb = new StringBuilder();
        var arr = decl.Type;

        do {
            var x = (ArrayType)arr;
            sb.Insert(0, "[]");
            arr = x.ElementType;
        } while (arr is ArrayType);

        var token = (BaseTokenNode)arr;
        var pre = TokenStrings[token.Kind];
        var post = string.Join("", sb);
        return $"{pre} {decl.Name.Name}{post}";
    }

    private static string ConvertIndexedAccess(VariableDeclaration decl) {
        var sb = new StringBuilder();
        var arr = decl.Type;

        do {
            var x = (IndexedAccessType)arr;
            sb.Insert(0, $"[{x.IndexType}]");
            arr = x.ObjectType;
        } while (arr is IndexedAccessType);

        var token = (BaseTokenNode)arr;
        var pre = TokenStrings[token.Kind];
        var post = string.Join("", sb);
        return $"{pre} {decl.Name.Name}{post}";
    }

    public string Visit(UnaryExpression n, object? ctx = null) {
        var op = TokenStrings[n.Operator];
        var operand = n.Operand.Accept(this, ctx);
        return $"{op}{operand}";
    }

    public string Visit(BaseTokenNode n, object? ctx = null) {
        return TokenStrings[n.Kind];
    }

    public string Visit(BinaryExpression n, object? ctx = null) {
        var lhs = n.Left.Accept(this, ctx);
        var rhs = n.Right.Accept(this, ctx);
        return $"{lhs} {n.OperatorToken} {rhs}";
    }

    public string Visit(FunctionDeclaration fn, object? ctx = null) {
        var list = new List<string>();
        foreach (var item in fn.Parameters) {
            var t = CPrimitive(item.Type);
            list.Add($"{t} {item.Name}");
        }
        var args = string.Join(", ", list);

        var ret = CType(fn.ReturnType);
        var name = fn.Name.Accept(this, ctx);
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
        var kinds = new HashSet<SyntaxKind>() { SyntaxKind.WhileStatement, SyntaxKind.DoStatement };
        foreach (var stmt in node.Statements) {
            if (kinds.Contains(stmt.Kind)) {
                writer.WriteLine();
            }
            writer.Write(Indent());
            stmt.Accept(this, ctx);
        }

        return "";
    }

    public string Visit(LiteralLikeNode node, object? ctx = null) {
        if (node.Kind == SyntaxKind.StringLiteral) {
            return $"\"{node.Text}\"";
        } else {
            return node.Text;
        }
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
            if (node.ElseStatement is IfStatement) {
                indent--;
                writer.Write(Indent());
                writer.Write("} else ");
                node.ElseStatement.Accept(this, ctx);
            } else {
                indent--;
                writer.Write(Indent());
                writer.WriteLine("} else {");
                indent++;
                node.ElseStatement.Accept(this, ctx);
                indent--;
                writer.Write(Indent());
                writer.WriteLine("}");
            }
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

        return $"{expr}({args});";
    }

    public string Visit(ExpressionStatement node, object? ctx = null) {
        var expr = node.Expression.Accept(this, ctx);
        writer.WriteLine(expr);
        return "";
    }

    public string Visit(ArrayLiteralExpression node, object? ctx = null) {
        var list = new List<string>();
        foreach (var item in node.Elements) {
            var i = item.Accept(this, ctx);
            list.Add(i);
        }
        var elems = string.Join(", ", list);
        return $"{{{elems}}}";
    }

    public string Visit(BaseNode node, object? ctx = null) {
        return "";
    }

    public string Visit(StructDeclaration node, object? ctx = null) {
        return "";
    }

    public string Visit(PropertySignature node, object? ctx = null) {
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
