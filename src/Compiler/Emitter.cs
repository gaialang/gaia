using System.Text;
using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public record TypeName(string Prefix, string Suffix = "");

public class Emitter : Visitor<string> {
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

    public string Visit(PackageDeclaration pkg) {
        // include headers.
        writer.WriteLine("#include <stdio.h>");
        writer.WriteLine("#include <stdlib.h>");
        writer.WriteLine("#include <string.h>");
        writer.WriteLine("#include <stdbool.h>");
        writer.WriteLine();

        // Hoist functions
        foreach (Statement stmt in pkg.Statements) {
            if (stmt is FunctionDeclaration func) {
                var name = func.Name.Accept(this);
                if (name == "main") {
                    continue;
                }
                string proto = CFunctionPrototype(func);
                writer.WriteLine($"{proto};");
            }
        }

        // TODO: optimize indentations.
        foreach (var stmt in pkg.Statements) {
            writer.WriteLine();
            stmt.Accept(this);
        }
        return "";
    }

    public string Visit(ImportDeclaration node) {
        writer.WriteLine($"#include \"{node.ModuleSpecifier}.h\"");
        return "";
    }

    public string Visit(Identifier id) {
        return id.Text;
    }

    public string Visit(VariableDeclaration node) {
        var typeName = CType(node.Type);
        var name = node.Name.Accept(this);
        if (node.Initializer is null) {
            writer.WriteLine($"{typeName.Prefix} {name}{typeName.Suffix};");
        } else {
            var val = node.Initializer.Accept(this);
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
        return kind switch {
            SyntaxKind.StringKeyword => "char*",
            SyntaxKind.NullKeyword => "NULL",
            _ => KindToText(kind),
        };
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

    public string Visit(UnaryExpression n) {
        var op = KindToText(n.Operator);
        var operand = n.Operand.Accept(this);
        return $"{op}{operand}";
    }

    public string Visit(KeywordLikeNode n) {
        return KindToText(n.Kind);
    }

    public string Visit(BinaryExpression n) {
        var lhs = n.Left.Accept(this);
        var rhs = n.Right.Accept(this);
        var op = KindToText(n.OperatorToken);
        return $"{lhs} {op} {rhs}";
    }

    private string CFunctionPrototype(FunctionDeclaration node) {
        var list = new List<string>();
        foreach (var item in node.Parameters) {
            var paramName = item.Name.Accept(this);
            var paramTypeName = CType(item.Type);
            list.Add($"{paramTypeName.Prefix} {paramName}{paramTypeName.Suffix}");
        }
        var paramsText = string.Join(", ", list);

        var returnTypeName = CType(node.Type);
        var name = node.Name.Accept(this);
        return $"{returnTypeName.Prefix}{returnTypeName.Suffix} {name}({paramsText})";
    }

    public string Visit(FunctionDeclaration node) {
        var proto = CFunctionPrototype(node);
        writer.WriteLine($"{proto} {{");
        indent++;
        node.Body?.Accept(this);
        writer.WriteLine("}");
        indent--;
        return "";
    }

    public string Visit(Parameter node) {
        var name = node.Name.Accept(this);
        var typ = node.Type.Accept(this);
        return $"{name}: {typ}";
    }

    public string Visit(WhileStatement node) {
        var expr = node.Expression.Accept(this);
        writer.WriteLine($"while ({expr}) {{");
        indent++;
        node.Body.Accept(this);
        indent--;
        writer.Write(Indent());
        writer.WriteLine("}");
        return "";
    }

    public string Visit(ReturnStatement node) {
        var s = node.Expression?.Accept(this);
        if (s is null) {
            writer.WriteLine($"return;");
        } else {
            writer.WriteLine($"return {s};");
        }
        return "";
    }

    public string Visit(AssignStatement node) {
        var name = node.Left.Accept(this);
        var val = node.Right.Accept(this);
        writer.WriteLine($"{name} = {val};");
        return "";
    }

    public string Visit(Block node) {
        var kinds = new HashSet<SyntaxKind>() { SyntaxKind.WhileStatement, SyntaxKind.DoStatement };
        foreach (var stmt in node.Statements) {
            if (kinds.Contains(stmt.Kind)) {
                writer.WriteLine();
            }
            writer.Write(Indent());
            stmt.Accept(this);
        }

        return "";
    }

    public string Visit(LiteralLikeNode node) {
        if (node.Kind == SyntaxKind.StringLiteral) {
            return $"\"{node.Text}\"";
        } else if (node.Kind == SyntaxKind.CharacterLiteral) {
            return $"'{node.Text}'";
        }

        return node.Text;
    }

    public string Visit(BreakStatement node) {
        writer.WriteLine("break;");
        return "";
    }

    public string Visit(IfStatement node) {
        var expr = node.Expression.Accept(this);
        writer.WriteLine($"if ({expr}) {{");
        indent++;
        node.ThenStatement.Accept(this);
        if (node.ElseStatement is null) {
            indent--;
            writer.Write(Indent());
            writer.WriteLine("}");
        } else {
            if (node.ElseStatement is IfStatement) {
                indent--;
                writer.Write(Indent());
                writer.Write("} else ");
                node.ElseStatement.Accept(this);
            } else {
                indent--;
                writer.Write(Indent());
                writer.WriteLine("} else {");
                indent++;
                node.ElseStatement.Accept(this);
                indent--;
                writer.Write(Indent());
                writer.WriteLine("}");
            }
        }
        return "";
    }

    public string Visit(ElementAssignStatement node) {
        var l = node.Left.Accept(this);
        var r = node.Right.Accept(this);
        writer.WriteLine($"{l} = {r};");
        return "";
    }

    public string Visit(ElementAccessExpression expr) {
        var l = expr.Expression.Accept(this);
        var index = expr.ArgumentExpression.Accept(this);
        return $"{l}[{index}]";
    }

    public string Visit(DoStatement node) {
        writer.WriteLine("do {");
        indent++;
        node.Body.Accept(this);
        indent--;
        writer.Write(Indent());
        var expr = node.Expression.Accept(this);
        writer.WriteLine($"}} while ({expr});");
        return "";
    }

    public string Visit(CallExpression node) {
        var expr = node.Expression.Accept(this);

        var list = new List<string>();
        foreach (var item in node.Arguments) {
            var s = item.Accept(this);
            list.Add(s);
        }
        var args = string.Join(", ", list);

        return $"{expr}({args});";
    }

    public string Visit(ExpressionStatement node) {
        var expr = node.Expression.Accept(this);
        writer.WriteLine(expr);
        return "";
    }

    public string Visit(ArrayLiteralExpression node) {
        var list = new List<string>();
        foreach (var item in node.Elements) {
            var i = item.Accept(this);
            list.Add(i);
        }
        var elems = string.Join(", ", list);
        return $"{{{elems}}}";
    }

    public string Visit(ArrayType node) {
        var elem = node.ElementType.Accept(this);
        return $"{elem}[]";
    }

    public string Visit(IndexedAccessType node) {
        var objType = node.ObjectType.Accept(this);
        return $"{objType}[{node.IndexType}]";
    }

    public string Visit(StructDeclaration node) {
        var name = node.Name.Accept(this);
        writer.WriteLine($"typedef struct {name} {{");
        indent++;

        foreach (var item in node.Members) {
            writer.Write(Indent());
            item.Accept(this);
        }

        writer.WriteLine($"}} {name};");
        indent--;
        return "";
    }

    public string Visit(PropertySignature node) {
        var name = node.Name.Accept(this);
        var typeName = CType(node.Type);
        writer.WriteLine($"{typeName.Prefix} {name}{typeName.Suffix};");
        return "";
    }

    public string Visit(InterfaceDeclaration node) {
        var name = node.Name.Accept(this);
        writer.WriteLine($"// interface {name} {{");
        indent++;

        foreach (var item in node.Members) {
            writer.Write("// ");
            writer.Write(Indent());
            item.Accept(this);
        }

        writer.WriteLine($"// }}");
        indent--;
        return "";
    }

    public string Visit(MethodSignature node) {
        var name = node.Name.Accept(this);

        var list = new List<string>();
        foreach (var item in node.Parameters) {
            var parameter = item.Accept(this);
            list.Add(parameter);
        }
        var paramsText = string.Join(", ", list);

        var returnType = node.Type.Accept(this);
        writer.WriteLine($"{name}({paramsText}) -> {returnType}");
        return "";
    }

    public string Visit(EnumDeclaration node) {
        var name = node.Name.Accept(this);
        writer.WriteLine($"typedef enum {name} {{");
        indent++;

        foreach (var item in node.Members) {
            writer.Write(Indent());
            item.Accept(this);
        }

        writer.WriteLine($"}} {name};");
        indent--;
        return "";
    }

    public string Visit(EnumMember node) {
        var name = node.Name.Accept(this);
        if (node.Initializer is null) {
            writer.WriteLine($"{name},");
        } else {
            var val = node.Initializer.Accept(this);
            writer.WriteLine($"{name} = {val},");
        }
        return "";
    }

    public string Visit(HeritageClause node) {
        return "";
    }

    public string Visit(ExpressionWithTypeArguments node) {
        return "";
    }
}
