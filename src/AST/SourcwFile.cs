using Gaia.Domain;

namespace Gaia.AST;

public sealed class SourceFile {
    public string FileName { get; }
    public List<int>? LineMap { get; set; }
    public string Text { get; }

    public SourceFile() {
        FileName = Path.Combine(AppContext.BaseDirectory, "tests/test.ga");
        using (var sr = new StreamReader(FileName)) {
            Text = sr.ReadToEnd();
        }
        Kind = SyntaxKind.SourceFile;
    }

    public SyntaxKind Kind { get; }

    // public TResult Accept<TResult>(Visitor<TResult> visitor) {
    //     return visitor.Visit(this);
    // }
}
