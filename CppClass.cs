using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppClassDef
{
    public enum CppClassType
    {
        Class,
        Struct
    }

    public enum CppMemberAccessibility
    {
        Public,
        Private,
        Protected
    }

    public class CppField
    {
        public CppMemberAccessibility Accessibility;
        public readonly bool IsStatic;
        public readonly ICppType Type;
        public readonly string Name;

        public CppField(CppMemberAccessibility accessibility, bool isStatic, ICppType type, string name)
        {
            Accessibility = accessibility;
            IsStatic = isStatic;
            Type = type;
            Name = name;
        }

        public string DeclareField() =>
            IsStatic
            ? $"static {Type.DeclareObjectByFullName(Name)};"
            : $"{Type.DeclareObjectByFullName(Name)};";
    }

    public abstract class CppMethod : CppFunction
    {
        public CppMemberAccessibility Accessibility;
        public virtual bool IsStatic { get; }

        protected CppMethod(CppLocalClass owner, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility) : base(owner, name, parameters, returnType)
        {
            Accessibility = accessibility;
        }
    }

    public class CppStaticMethod : CppMethod
    {
        public CppStaticMethod(CppLocalClass owner, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility) : base(owner, name, parameters, returnType, accessibility)
        {
        }

        public override string DeclareFunctionPrototype()
        {
            return $"static {ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")});";
        }

        public override bool IsStatic => true;
    }

    public class CppInstanceMethod : CppMethod
    {
        public virtual bool IsVirtual => false;
        public override bool IsStatic => false;

        protected CppLocalClass ThisType;

        public CppInstanceMethod(CppLocalClass owner, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility) : base(owner, name, parameters, returnType, accessibility)
        {
            ThisType = owner;
        }

        public override string DeclareFunctionPrototype()
        {
            return $"{ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")});";
        }
    }

    public class CppConstructor : CppInstanceMethod
    {
        public readonly IList<KeyValuePair<string, string>> Initializers = new List<KeyValuePair<string, string>>();

        public CppConstructor(CppLocalClass owner, IList<CppParameter> parameters, CppMemberAccessibility accessibility) : base(owner, owner.Name, parameters, owner, accessibility)
        {
        }

        public override string DeclareFunctionPrototype()
        {
            return $"{ThisType.Name}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")});";
        }

        public override string DefineFunctionHead()
        {
            var head = $"{ThisType.Name}::{ThisType.Name}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")})";
            if (Initializers.Count != 0)
            {
                head += "\n";
                head += $": {Initializers.Select(initializer => $"{initializer.Key}({initializer.Value})").JoinWith(", ")}".Indent();
            }
            return head;
        }
    }

    public class CppVirtualMethod : CppInstanceMethod
    {
        public CppVirtualMethod(CppLocalClass scope, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility) : base(scope, name, parameters, returnType, accessibility)
        {
        }

        public CppVirtualMethod(CppLocalClass scope, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility, bool isAbstract) : base(scope, name, parameters, returnType, accessibility)
        {
            IsAbstract = isAbstract;
        }

        public override bool IsVirtual => true;
        public bool IsAbstract = false;

        public override string DeclareFunctionPrototype()
        {
            if (IsAbstract)
            {
                return $"virtual {ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")}) = 0;";
            }

            return $"virtual {ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")});";
        }

        public override string DefineFunctionHead()
        {
            if (IsAbstract) return "";

            return base.DefineFunctionHead();
        }

        public override string DefineFunction()
        {
            if (IsAbstract) return "";

            return base.DefineFunction();
        }
    }

    public class CppOverridingMethod : CppVirtualMethod
    {
        public CppOverridingMethod(CppLocalClass scope, string name, IList<CppParameter> parameters, ICppType returnType, CppMemberAccessibility accessibility) : base(scope, name, parameters, returnType, accessibility)
        {
            bool overrides = false;

            // iterates up the hierarchy and see if this function overrides something
            var it = ThisType.BaseClass;
            while (it != null)
            {
                foreach (var method in it.Methods.OfType<CppVirtualMethod>())
                {
                    if (method.Name == Name &&
                        method.Parameters.SequenceEqual(Parameters) &&
                        method.ReturnType == ReturnType)
                    {
                        overrides = true;
                        break;
                    }
                }
                it = it.BaseClass;
            }

            if (!overrides) throw new ArgumentException("The method tries to override but none of its base classes has an overridable one");
        }

        public override string DeclareFunctionPrototype()
        {
            return $"virtual {ReturnType.DeclareObjectByFullName(Name)}({Parameters.Select(parameter => parameter.DeclareParameter()).JoinWith(", ")}) override;";
        }
    }

    public abstract class CppClass : CppScopedType, ICppScope
    {
        public CppClass(ICppScope scope, string typename) : base(scope, typename)
        {
        }

        public CppClassType ClassType = CppClassType.Class;

        public ICppScope Outer => Scope;
    }

    public class CppExternClass : CppClass
    {
        public CppExternClass(ICppScope scope, string typename) : base(scope, typename)
        {
        }

        public override string DeclareType() => ClassType == CppClassType.Class ? $"class {Name};\n" : $"struct {Name};\n";
    }

    public class CppLocalClass : CppClass
    {
        public readonly IList<CppField> Fields = new List<CppField>();
        public readonly IList<CppMethod> Methods = new List<CppMethod>();

        // public CppMemberAccessibility InheritanceAccessibility = CppMemberAccessibility.Public;
        public CppLocalClass BaseClass = null;

        public CppLocalClass(ICppScope scope, string typename) : base(scope, typename)
        {
        }

        public CppLocalClass(ICppScope scope, string typename, CppLocalClass baseClass) : base(scope, typename)
        {
            BaseClass = baseClass;
        }

        public CppExternClass AsExtern()
        {
            return new CppExternClass(this.Scope, this.Name);
        }

        public void OverrideAll()
        {
            var it = BaseClass;
            while (it != null)
            {
                foreach (var method in it.Methods.OfType<CppVirtualMethod>())
                {
                    this.Methods.Add(new CppOverridingMethod(this, method.Name, method.Parameters, method.ReturnType, method.Accessibility));
                }
                it = it.BaseClass;
            }
        }

        public void CreateConstructor()
        {
            var forFields = new List<CppParameter>();
            foreach (var field in Fields.Where(field => !field.IsStatic))
            {
                forFields.Add(new CppParameter(field.Type, field.Name));
            }
            var inits = 
                forFields
                .Select(
                    field => new KeyValuePair<string, string>(
                        field.Name,
                        field.Type.IsCopyConstructible
                        ? field.Name
                        : $"std::move({field.Name})"));
            var ctor = new CppConstructor(this, forFields, CppMemberAccessibility.Public);
            foreach (var init in inits) ctor.Initializers.Add(init);
            Methods.Add(ctor);
        }

        public virtual string DefineMethods()
        {
            return Methods.Select(method => method.DefineFunction()).JoinWith("\n");
        }

        public override string DeclareType()
        {
            var head = ClassType == CppClassType.Class ? $"class {Name}" : $"struct {Name}";
            if (BaseClass != null)
            {
                head += $": public {BaseClass.FullName}"; // currently assume all base classes are publicly inherited
            }

            var publicFields = Fields.Where(field => field.Accessibility == CppMemberAccessibility.Public);
            var privateFields = Fields.Where(field => field.Accessibility == CppMemberAccessibility.Private);
            var protectedFields = Fields.Where(field => field.Accessibility == CppMemberAccessibility.Protected);

            var publicMethods = Methods.Where(method => method.Accessibility == CppMemberAccessibility.Public);
            var privateMethods = Methods.Where(method => method.Accessibility == CppMemberAccessibility.Private);
            var protectedMethods = Methods.Where(method => method.Accessibility == CppMemberAccessibility.Protected);

            var memberDeclaration = "";
            if (publicFields.Count() != 0 || publicMethods.Count() != 0) 
                memberDeclaration += $"public:\n{publicFields.Select(field => field.DeclareField()).ToLines().Indent()}\n" +
                    $"{publicMethods.Select(method => method.DeclareFunctionPrototype()).ToLines().Indent()}\n";
            if (privateFields.Count() != 0 || privateMethods.Count() != 0) 
                memberDeclaration += $"private:\n{privateFields.Select(field => field.DeclareField()).ToLines().Indent()}\n" +
                    $"{privateMethods.Select(method => method.DeclareFunctionPrototype()).ToLines().Indent()}\n"; ;
            if (protectedFields.Count() != 0 || protectedMethods.Count() != 0) 
                memberDeclaration += $"protected:\n{protectedFields.Select(field => field.DeclareField()).ToLines().Indent()}\n" +
                    $"{protectedMethods.Select(method => method.DeclareFunctionPrototype()).ToLines().Indent()}\n"; ;

            return $"{head}\n{{\n{memberDeclaration}}};\n";
        }
    }
}
