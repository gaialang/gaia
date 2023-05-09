using System.Text;
using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public class Checker : Visitor<Expression?> {
    private ToplevelScope toplevelScope = new ToplevelScope();

    public Expression? Visit(PackageDeclaration pkg) {
        // Hoist functions to scope
        foreach (var stmt in pkg.Statements) {
            if (stmt is FunctionDeclaration func) {
                var name = func.Name.Text;
                toplevelScope.Add(name, new FunctionEntity(func.Type, func.Parameters));
            }
        }

        foreach (var stmt in pkg.Statements) {
            stmt.Accept(this);
        }
        return null;
    }

    public Expression? Visit(ImportDeclaration node) {
        return null;
    }

    public Expression? Visit(Identifier id) {
        return null;
    }

    public Expression? Visit(VariableDeclaration node) {
        var name = node.Name.Text;
        if (node.Initializer is not null) {
            var typ = node.Initializer.Accept(this);
            if (!Equals(node.Type, typ)) {
                throw new CheckError($"Type mismatch {node.Type} != {typ}");
            }
        }
        toplevelScope.Add(name, new VariableEntity(node.Type));
        return null;
    }

    private bool Equals(Expression? a, Expression? b) {
        if (a is null && b is null) {
            return true;
        }
        if (a is null || b is null) {
            return false;
        }
        if (a is ArrayType && b is ArrayType) {

        }
        if (a is IndexedAccessType && b is IndexedAccessType) {

        }

        return a.Kind == b.Kind;
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

    public Expression? Visit(UnaryExpression node) {
        if (node.Operator is SyntaxKind.ExclamationToken) {
            return new KeywordLikeNode(SyntaxKind.BoolKeyword);
        } else {
            return node.Operand.Accept(this);
        }
    }

    public Expression? Visit(KeywordLikeNode node) {
        return node;
    }

    public Expression? Visit(BinaryExpression node) {
        var accept = new HashSet<SyntaxKind>() { SyntaxKind.IntKeyword, SyntaxKind.StringKeyword };

        var lhs = node.Left.Accept(this);
        var rhs = node.Right.Accept(this);

        switch (node.OperatorToken) {
        case SyntaxKind.PlusToken:
            if (!accept.Contains(lhs!.Kind) || !accept.Contains(rhs!.Kind)) {
                throw new CheckError("Type mismatch");
            }
            if (!Equals(lhs, rhs)) {
                throw new CheckError("Type mismatch for +, lhs and rhs must be same type");
            }
            break;
        default:
            break;
        }
        return null;
    }

    public Expression? Visit(FunctionDeclaration node) {
        var list = new List<string>();
        foreach (var item in node.Parameters) {
            var paramName = item.Name.Accept(this);
            var paramTypeName = CType(item.Type);
            list.Add($"{paramTypeName.Prefix} {paramName}{paramTypeName.Suffix}");
        }
        var paramsText = string.Join(", ", list);

        var returnTypeName = CType(node.Type);
        var name = node.Name.Accept(this);
        node.Body?.Accept(this);
        return null;
    }

    public Expression? Visit(Parameter node) {
        var name = node.Name.Accept(this);
        var typ = node.Type.Accept(this);
        return null;
    }

    public Expression? Visit(WhileStatement node) {
        var expr = node.Expression.Accept(this);
        node.Body.Accept(this);
        return null;
    }

    public Expression? Visit(ReturnStatement node) {
        var s = node.Expression?.Accept(this);
        if (s is null) {
        } else {
        }
        return null;
    }

    public Expression? Visit(AssignStatement node) {
        var name = node.Left.Accept(this);
        var val = node.Right.Accept(this);
        return null;
    }

    public Expression? Visit(Block node) {
        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }

        return null;
    }

    public Expression? Visit(LiteralLikeNode node) {
        return node;
    }

    public Expression? Visit(BreakStatement node) {
        return null;
    }

    public Expression? Visit(IfStatement node) {
        var expr = node.Expression.Accept(this);
        node.ThenStatement.Accept(this);
        if (node.ElseStatement is null) {
        } else {
            if (node.ElseStatement is IfStatement) {
                node.ElseStatement.Accept(this);
            } else {
                node.ElseStatement.Accept(this);
            }
        }
        return null;
    }

    public Expression? Visit(ElementAssignStatement node) {
        return null;


    }

    public Expression? Visit(ElementAccessExpression expr) {
        return null;

    }

    public Expression? Visit(DoStatement node) {
        return null;

    }

    public Expression? Visit(CallExpression node) {
        return null;
    }

    public Expression? Visit(ExpressionStatement node) {
        return null;

    }

    public Expression? Visit(ArrayLiteralExpression node) {
        return null;

    }

    public Expression? Visit(ArrayType node) {
        return null;

    }

    public Expression? Visit(IndexedAccessType node) {
        return null;

    }

    public Expression? Visit(StructDeclaration node) {
        return null;

    }

    public Expression? Visit(PropertySignature node) {
        return null;

    }

    public Expression? Visit(InterfaceDeclaration node) {
        return null;
    }

    public Expression? Visit(MethodSignature node) {
        return null;
    }

    public Expression? Visit(EnumDeclaration node) {
        return null;
    }

    public Expression? Visit(EnumMember node) {
        return null;
    }
}
