using Gaia.AST;
using Gaia.Compiler;

Console.WriteLine("Gaia >>>");

var scanner = new Scanner();
var parser = new Parser(scanner);
var expr = parser.Parse();
var e = new Emitter();
e.Visit(expr as PackageNode);

Console.WriteLine("\nOK.");
