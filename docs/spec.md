# context-free grammar

VarDecl -> var Id: Type;

FuncDecl -> func Id(ArgList) ReturnType { Statement* }

ArgList -> Id: Type ArgRest* | ε

ArgRest -> , Id: Type

Type -> int | string | bool | Id

ReturnType -> -> Type | ε

Statement -> { Statement* }
    -> Id = Expr;

ExprList -> Expr ExprRest* | ε

ExprRest -> , Expr
