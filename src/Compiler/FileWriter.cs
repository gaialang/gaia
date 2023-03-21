namespace Gaia.Compiler;

public class FileWriter : Writer {
    private StreamWriter sw;

    public FileWriter() {
        var path = "E:/Monorepos/gaia/out";
        var name = "test.c";
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        var full = Path.Combine(path, name);
        sw = new StreamWriter(full);
    }

    public void WriteLine(string s = "") {
        sw.WriteLine(s);
    }

    public void Write(string s = "") {
        sw.Write(s);
    }

    public void Close() {
        sw.Close();
    }
}
