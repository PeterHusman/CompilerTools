using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{
    public enum AccessLevel
    {
        Public,
        Private
    }

    public abstract class TreeNode
    {
    }


    public class Function : ClassMember
    {
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public List<Statement> Statements { get; set; } = new List<Statement>();

        public string Name { get; set; }

        public NamespacedThing ReturnType { get; set; }
    }

    public abstract class Statement : TreeNode
    {

    }

    public class Assignment : Statement
    {
        public NamespacedThing LHS { get; set; }
        public Expression RHS { get; set; }
    }

    public class GetVariableOrField : Expression
    {
        public NamespacedThing Value { get; set; }
    }

    public class Call : Statement
    {

    }

    public class Return : Statement
    {
        public Expression Value { get; set; }
    }

    public class Declaration : Statement
    {
        public string Name { get; set; }
        public NamespacedThing Type { get; set; }
    }

    public class DeclarationAssignment : Declaration
    {
        public Expression Value { get; set; }
    }

    public abstract class Expression : TreeNode
    {

    }

    public class NamespacedThing : TreeNode
    {
        public NamespacedThing Parent { get; set; }
        public string Name { get; set; }
    }

    public class UnparsedExprCuzLazy : Expression
    {
        public List<KeyValuePair<string, ThingType>> Tokens { get; set; } = new List<KeyValuePair<string, ThingType>>();
    }

    public class LazyStatement : Statement
    {
        public UnparsedExprCuzLazy LazyExpr { get; set; }
    }

    public class Parameter : TreeNode
    {
        public string Name { get; set; }
        public NamespacedThing Type { get; set; }
    }

    public abstract class ClassMember : TreeNode
    {
        public AccessLevel AccessLevel { get; set; }
    }

    public class CompilationUnit : TreeNode
    {
        public string Namespace { get; set; }

        public Function EntryPoint { get; set; } = null;

        public List<Class> Classes { get; set; } = new List<Class>();
    }

    public class Class : TreeNode
    {
        public override string ToString()
        {
            return Name;
        }


        public Function Ctor { get; set; } = null;

        public bool Static { get; set; } = false;

        public AccessLevel AccessLevel { get; set; }
        public string Name { get; set; }
        public List<Function> InstanceMethods { get; set; } = new List<Function>();
        public List<Declaration> InstanceFields { get; set; } = new List<Declaration>();
        public List<Function> StaticMethods { get; set; } = new List<Function>();
        public List<Declaration> StaticFields { get; set; } = new List<Declaration>();
    }
}
