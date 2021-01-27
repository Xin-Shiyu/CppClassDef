using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CppClassDef
{
    public class CppTemplateParameter : ICppType
    {
        public CppTemplateParameter(string name)
        {
            Name = name;
        }

        public string FullName => Name;
        public string Name { get; set; }

        public bool IsCopyConstructible => true;

        public string DeclareObject(string objectName) => $"{Name} {objectName}";

        public string DeclareObjectByFullName(string objectName) => $"{FullName} {objectName}";
    }

    public class CppClassTemplate : CppLocalClass
    {
        public readonly CppTemplateParameter TemplateParameter;

        public CppClassTemplate(CppTemplateParameter templateParameter, ICppScope scope, string typename, CppLocalClass baseClass) : base(scope, typename, baseClass)
        {
            TemplateParameter = templateParameter;
        }

        public override string DeclareType()
        {
            return $"template <typename {TemplateParameter.Name}>\n{base.DeclareType()}";
        }

        public override string DefineMethods()
        {
            return Methods
                .Where(method => !(method is CppVirtualMethod virtualMethod && virtualMethod.IsAbstract))
                .Select(method => $"template <typename {TemplateParameter.Name}>\n{method.DefineFunction()}")
                .JoinWith("\n");
        }

        public string InstantiatedName(ICppType realType)
        {
            return $"{Name}<{realType.FullName}>";
        }

        public string InstantiatedFullName(ICppType realType)
        {
            return $"{FullName}<{realType.FullName}>";
        }

        public CppScopedType InstantiateWith(ICppType realType)
        {
            return new CppExternClass(this.Scope, $"{Name}<{realType.FullName}>");
        }
    }
}
