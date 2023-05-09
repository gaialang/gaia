using Gaia.AST;

namespace Gaia.Compiler;

public record Entity(Expression Type);

public record VariableEntity(Expression Type) : Entity(Type);

public record FunctionEntity(Expression Type, List<Parameter> Parameters) : Entity(Type);
