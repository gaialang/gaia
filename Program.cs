using Gaia.AST;
using Gaia.Compiler;

Console.WriteLine("Gaia 0.1 >>>");

var scanner = new Scanner();
var parser = new Parser(scanner);
var expr = (PackageDeclaration)parser.Parse();

var checker = new Checker(scanner);
checker.Visit(expr);

var file = new FileWriter();
var emit = new Emitter(file);
emit.Visit(expr);
file.Close();

Console.WriteLine(">>> OK.");
