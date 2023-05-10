namespace Gaia.Domain;

public enum TokenFlags {
    None = 0,
    PrecedingLineBreak = 1 << 0,
    Unterminated = 1 << 2,
}
