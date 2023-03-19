namespace Gaia.Compiler;

public class ConsoleWriter : Writer {
    public void WriteLine(string s = "") {
        Console.WriteLine(s);
    }

    public void Write(string s = "") {
        Console.Write(s);
    }
}
