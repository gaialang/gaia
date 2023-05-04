using System.Text;
using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public record TypeName(string Prefix, string Suffix = "");

public class Emitter : Visitor<string, object?> {
    private Writer writer;
    private int indent = 0;

    public Emitter(Writer writer) {
        this.writer = writer;
    }

    private string Indent() {
        var sb = new StringBuilder();
        for (var i = 0; i < indent; i++) {
            sb.Append("    ");
        }
        return sb.ToString();
    }

    public string Visit(PackageDeclaration pkg, object? ctx = null) {
        // include headers.
        writer.WriteLine("#include <stdio.h>");
        writer.WriteLine("#include <stdlib.h>");
        writer.WriteLine("#include <string.h>");
        writer.WriteLine("#include <stdbool.h>");

        // TODO: optimize indentations.
        var kinds = new HashSet<SyntaxKind>() {
            SyntaxKind.FunctionDeclaration, SyntaxKind.VariableDeclaration,
            SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            };
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

    public string Visit(VariableDeclaration node, object? ctx = null) {
        var typeName = CType(node.Type);
        var name = node.Name.Accept(this, ctx);
        if (node.Initializer is null) {
            writer.WriteLine($"{typeName.Prefix} {name}{typeName.Suffix};");
        } else {
            var val = node.Initializer.Accept(this, ctx);
            writer.WriteLine($"{typeName.Prefix} {name}{typeName.Suffix} = {val};");
        }
        return "";
    }

    private static TypeName CType(Expression node) {
        if (node is ArrayType arr) {
            return CArray(arr);
        }
        if (node is IndexedAccessType acc) {
            return CIndexedAccess(acc);
        }

        var prefix = CPrimitive(node.Kind);
        return new TypeName(prefix);
    }

    private static string CPrimitive(SyntaxKind kind) {
        if (kind is SyntaxKind.StringKeyword) {
            return "char*";
        }
        return TokenToText[kind];
    }

    private static TypeName CArray(ArrayType node) {
        var sb = new StringBuilder();
        sb.Insert(0, "[]");
        var typ = node.ElementType;
        while (typ is ArrayType arr) {
            sb.Insert(0, "[]");
            typ = arr.ElementType;
        }

        var prefix = CPrimitive(typ.Kind);
        var suffix = string.Join("", sb);
        return new TypeName(prefix, suffix);
    }

    private static TypeName CIndexedAccess(IndexedAccessType node) {
        var sb = new StringBuilder();
        sb.Insert(0, $"[{node.IndexType}]");
        var typ = node.ObjectType;
        while (typ is IndexedAccessType arr) {
            sb.Insert(0, $"[{arr.IndexType}]");
            typ = arr.ObjectType;
        }

        var prefix = CPrimitive(typ.Kind);
        var suffix = string.Join("", sb);
        return new TypeName(prefix, suffix);
    }

    public string Visit(UnaryExpression n, object? ctx = null) {
        var op = TokenToText[n.Operator];
        var operand = n.Operand.Accept(this, ctx);
        return $"{op}{operand}";
    }

    public string Visit(KeywordLikeNode n, object? ctx = null) {
        return TokenToText[n.Kind];
    }

    public string Visit(BinaryExpression n, object? ctx = null) {
        var lhs = n.Left.Accept(this, ctx);
        var rhs = n.Right.Accept(this, ctx);
        var op = TokenToText[n.OperatorToken];
        return $"{lhs} {op} {rhs}";
    }

    public string Visit(FunctionDeclaration node, object? ctx = null) {
        var list = new List<string>();
        foreach (var item in node.Parameters) {
            var paramName = item.Name.Accept(this, ctx);
            var paramTypeName = CType(item.Type);
            list.Add($"{paramTypeName.Prefix} {paramName}{paramTypeName.Suffix}");
        }
        var paramsText = string.Join(", ", list);

        var returnTypeName = CType(node.Type);
        var name = node.Name.Accept(this, ctx);
        writer.WriteLine($"{returnTypeName.Prefix}{returnTypeName.Suffix} {name}({paramsText}) {{");
        indent++;
        node.Body?.Accept(this, ctx);
        writer.WriteLine("}");
        indent--;
        return "";
    }

    public string Visit(Parameter node, object? ctx = null) {
        var name = node.Name.Accept(this, ctx);
        var typ = node.Type.Accept(this, ctx);
        return $"{name}: {typ}";
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
        return node.Text;
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

    public string Visit(ArrayType node, object? ctx = null) {
        var elem = node.ElementType.Accept(this, ctx);
        return $"{elem}[]";
    }

    public string Visit(IndexedAccessType node, object? ctx = null) {
        var objType = node.ObjectType.Accept(this, ctx);
        return $"{objType}[{node.IndexType}]";
    }

    public string Visit(StructDeclaration node, object? ctx = null) {
        var name = node.Name.Accept(this, ctx);
        writer.WriteLine($"typedef struct {name}_type {{");
        indent++;

        foreach (var item in node.Members) {
            writer.Write(Indent());
            item.Accept(this, ctx);
        }

        writer.WriteLine($"}} {name};");
        indent--;
        return "";
    }

    public string Visit(PropertySignature node, object? ctx = null) {
        var name = node.Name.Accept(this, ctx);
        var typeName = CType(node.Type);
        writer.WriteLine($"{typeName.Prefix} {name}{typeName.Suffix};");
        return "";
    }

    public string Visit(InterfaceDeclaration node, object? ctx = null) {
        var name = node.Name.Accept(this, ctx);
        writer.WriteLine($"// interface {name} {{");
        indent++;

        foreach (var item in node.Members) {
            writer.Write("// ");
            writer.Write(Indent());
            item.Accept(this, ctx);
        }

        writer.WriteLine($"// }}");
        indent--;
        return "";
    }

    public string Visit(MethodSignature node, object? ctx = null) {
        var name = node.Name.Accept(this, ctx);

        var list = new List<string>();
        foreach (var item in node.Parameters) {
            var parameter = item.Accept(this, ctx);
            list.Add(parameter);
        }
        var paramsText = string.Join(", ", list);

        var returnType = node.Type.Accept(this, ctx);
        writer.WriteLine($"{name}({paramsText}) -> {returnType}");
        return "";
    }
}
