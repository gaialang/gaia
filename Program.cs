using Gaia.AST;
using Gaia.Compiler;

Console.WriteLine("Gaia 0.1.0\n>>>");

var scanner = new Scanner();
var parser = new Parser(scanner);
var expr = parser.Parse();
var e = new Emitter();
e.Visit((PackageNode)expr);

Console.WriteLine(">>>");
