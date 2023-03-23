using Gaia.AST;
using Gaia.Compiler;

Console.WriteLine("Gaia 0.1.0 >>>");

var scanner = new Scanner();
var parser = new Parser(scanner);
var expr = parser.Parse();
var f = new ConsoleWriter();
var e = new Emitter(f);
e.Visit((PackageDeclaration)expr);
// f.Close();

Console.WriteLine(">>> OK.");
