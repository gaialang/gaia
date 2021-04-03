using System;
using Gaia.Lex;
using Gaia.Parse;

var lexer = new Lexer();
var parser = new Parser(lexer);
parser.Run();
