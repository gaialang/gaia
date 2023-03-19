using Gaia.AST;
using Gaia.Compiler;

Console.WriteLine("Gaia 0.1.0\n>>>");

var scanner = new Scanner();
var parser = new Parser(scanner);
var expr = parser.Parse();
var e = new Emitter(new ConsoleWriter());
e.Visit((PackageStmt)expr);

Console.WriteLine(">>>");
