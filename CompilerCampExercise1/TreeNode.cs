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

        public string ReturnType { get; set; }
    }

    public abstract class Statement : TreeNode
    {

    }

    public class Assignment : Statement
    {

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
        public string Type { get; set; }
    }

    public class DeclarationAssignment : Declaration
    {
        public Expression Value { get; set; }
    }

    public abstract class Expression : TreeNode
    {

    }

    public class UnparsedExprCuzLazy : Expression
    {
        public List<KeyValuePair<string, ThingType>> Tokens { get; set; } = new List<KeyValuePair<string, ThingType>>();
    }

    public class Parameter : TreeNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }


    //public class Field : ClassMember
    //{
    //    public string Type { get; set; }
    //}

    public abstract class ClassMember : TreeNode
    {
        public AccessLevel AccessLevel { get; set; }
    }

    public class CompilationUnit : TreeNode
    {
        public string Namespace { get; set; }

        public List<Class> Classes { get; set; } = new List<Class>();
    }

    public class Class : TreeNode
    {
        public AccessLevel AccessLevel { get; set; }
        public string Name { get; set; }
        public List<Function> InstanceMethods { get; set; } = new List<Function>();
        public List<Declaration> InstanceFields { get; set; } = new List<Declaration>();
        public List<Function> StaticMethods { get; set; } = new List<Function>();
        public List<Declaration> StaticFields { get; set; } = new List<Declaration>();
    }
}
