namespace Gaia.Inter;

public class Pkg : Stmt {
    public readonly Id Id;

    public Pkg(Id id) {
        Id = id;
    }

    public override string ToString() {
        return Id.ToString();
    }
}
