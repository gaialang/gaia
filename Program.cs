using Gaia.Lex;
using Gaia.Parse;

Console.WriteLine("Gaia >");
var lexer = new Lexer();
var parser = new Parser(lexer);
parser.Run();
