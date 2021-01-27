using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppClassDef
{
    public class CppParameter
    {
        public readonly ICppType Type;
        public readonly string Name;

        public CppParameter(ICppType type, string name)
        {
            Type = type;
            Name = name;
        }

        public string DeclareParameter() => Type.DeclareObjectByFullName(Name);
    }

    public abstract class CppFunction
    {
        public readonly ICppScope Scope;
        public readonly string Name;
        public readonly IList<CppParameter> Parameters;
        public readonly ICppType ReturnType;
        public string Code = "";

        public string FullName => $"{Scope.FullName}::{Name}";

        public CppFunction(ICppScope scope, string name, IList<CppParameter> parameters, ICppType returnType)
        {
            Scope = scope;
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
        }

        public virtual string DeclareFunctionPrototype()
        {
            return $"{ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")});";
        }

        public virtual string DefineFunctionHead()
        {
            return $"{ReturnType.DeclareObjectByFullName(FullName.TrimStart(':'))}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")})";
        }

        public virtual string DefineFunction()
        {
            return $"{DefineFunctionHead()}\n{{\n{Code.Indent()}\n}}\n";
        }
    }
}
