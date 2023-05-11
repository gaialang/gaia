using System.Text;
using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public class Checker : Visitor<Expression?> {
    private ToplevelScope toplevelScope = new ToplevelScope();
    private Scanner scanner;

    public Checker(Scanner scanner) {
        this.scanner = scanner;
    }

    public string LineColumn(int pos) {
        return scanner.LineColumn(pos);
    }

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
        if (string.IsNullOrWhiteSpace(node.ModuleSpecifier)) {
            throw new CheckError($"{LineColumn(node.Pos)}: Module specifier is required");
        }

        return null;
    }

    public Expression? Visit(Identifier id) {
        var e = toplevelScope.Get(id.Text) ??
            throw new CheckError($"{LineColumn(id.Pos)}: Unknown identifier `{id.Text}`");
        return e.Type;
    }

    public Expression? Visit(VariableDeclaration node) {
        var name = node.Name.Text;
        if (toplevelScope.ContainsLocal(name)) {
            throw new CheckError($"{LineColumn(node.Name.Pos)}: Variable `{name}` already declared");
        }

        if (node.Initializer is not null) {
            var typ = node.Initializer.Accept(this);
            if (!Equals(node.Type, typ)) {
                throw new CheckError($"{LineColumn(node.Initializer.Pos)}: Type mismatch, expected {TokenToText[node.Type.Kind]} but got {TokenToText[typ!.Kind]}");
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
        if (kind == SyntaxKind.StringKeyword) {
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
        if (node.Operator == SyntaxKind.ExclamationToken) {
            return new KeywordLikeNode(SyntaxKind.BoolKeyword, -1, -1);
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
        // TODO: handle parameters
        // node.Body?.Accept(this);
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
        var lType = node.Left.Accept(this);
        var rType = node.Right.Accept(this);
        return null;
    }

    public Expression? Visit(Block node) {
        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }

        return null;
    }

    public Expression? Visit(LiteralLikeNode node) {
        return node.Kind switch {
            SyntaxKind.IntLiteral => new KeywordLikeNode(SyntaxKind.IntKeyword, -1, -1),
            SyntaxKind.StringLiteral => new KeywordLikeNode(SyntaxKind.StringKeyword, -1, -1),
            SyntaxKind.TrueKeyword => new KeywordLikeNode(SyntaxKind.BoolKeyword, -1, -1),
            SyntaxKind.BoolKeyword => new KeywordLikeNode(SyntaxKind.BoolKeyword, -1, -1),
            _ => null
        };
    }

    public Expression? Visit(BreakStatement node) {
        return null;
    }

    public Expression? Visit(IfStatement node) {
        /*
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
        */
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
