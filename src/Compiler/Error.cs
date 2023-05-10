namespace Gaia.Compiler;

[Serializable]
public class ParseError : Exception {
    public ParseError(string message) : base(message) { }
    public ParseError(string message, Exception inner) : base(message, inner) { }
}

[Serializable]
public class CheckError : Exception {
    public CheckError(string message) : base(message) { }
    public CheckError(string message, Exception inner) : base(message, inner) { }
}
