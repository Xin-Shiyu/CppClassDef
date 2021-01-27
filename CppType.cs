using System;
using System.Collections.Generic;
using System.Text;

namespace CppClassDef
{
    static class CppTypeExtensions
    {
        public static CppPointer ToPointer(this ICppType type) => new CppPointer(type);
        public static CppReference ToReference(this ICppType type) => new CppReference(type);
        public static CppConst ToConst(this ICppType type) => new CppConst(type);
        public static CppArray ToArray(this ICppType type, ulong length) => new CppArray(type, length);
        public static ICppType Decay(this ICppType type)
        {
            if (type is CppConst cppConst)
            {
                return cppConst.BaseType.Decay();
            }
            else if (type is CppReference cppReference)
            {
                return cppReference.BaseType.Decay();
            }
            else if (type is CppArray cppArray)
            {
                return cppArray.BaseType.ToPointer().Decay();
            }
            return type;
        }
    }

    interface ICppType
    {
        string FullName { get; }
        string Name { get; }
        bool IsCopyConstructible { get; }

        string DeclareObjectByFullName(string objectName);
        string DeclareObject(string objectName);
    }

    abstract class CppScopedType : ICppType
    {
        public readonly ICppScope Scope;

        public CppScopedType(ICppScope scope, string typename)
        {
            Scope = scope;
            Name = typename;
        }

        public string FullName => $"{Scope.FullName}::{Name}";
        public virtual string Name { get; private set; }

        public bool IsCopyConstructible { get; set; } = true;

        public string DeclareObject(string objectName) => $"{Name} {objectName}";

        public string DeclareObjectByFullName(string objectName) => $"{FullName} {objectName}";

        public abstract string DeclareType();
    }

    class CppPrimitiveType : ICppType
    {
        public CppPrimitiveType(string name)
        {
            Name = name;
        }

        public string FullName => Name;
        public string Name { get; set; }

        public bool IsCopyConstructible => true;

        public string DeclareObject(string objectName) => $"{Name} {objectName}";

        public string DeclareObjectByFullName(string objectName) => $"{FullName} {objectName}";

        public static CppPrimitiveType Void = new CppPrimitiveType("void");
        public static CppPrimitiveType Int = new CppPrimitiveType("int");
        public static CppPrimitiveType Char = new CppPrimitiveType("char");
    }

    abstract class CppCompoundType : ICppType
    {
        public ICppType BaseType;

        protected CppCompoundType(ICppType baseType)
        {
            BaseType = baseType;
        }

        public string FullName => DeclareObjectByFullName("");
        public string Name => DeclareObject("");

        public abstract bool IsCopyConstructible { get; }

        public abstract string DeclareObject(string objectName);
        public abstract string DeclareObjectByFullName(string objectName);
    }

    class CppPointer : CppCompoundType
    {
        public CppPointer(ICppType baseType) : base(baseType)
        {
            if (baseType is CppReference) throw new ArgumentException("A pointer to a reference type is not a valid type");
        }

        public override string DeclareObject(string objectName) => BaseType.DeclareObject($"*{objectName}");

        public override string DeclareObjectByFullName(string objectName) => BaseType.DeclareObjectByFullName($"*{objectName}");

        public static CppPointer CString = CppPrimitiveType.Char.ToConst().ToPointer();

        public override bool IsCopyConstructible => true;
    }

    class CppReference : CppCompoundType
    {
        public CppReference(ICppType baseType) : base(baseType)
        {
        }

        public override bool IsCopyConstructible => BaseType.IsCopyConstructible;

        public override string DeclareObject(string objectName)
        {
            if (BaseType is CppArray)
            {
                return BaseType.DeclareObject($"(&{objectName})");
            }

            return BaseType.DeclareObject($"&{objectName}");
        }

        public override string DeclareObjectByFullName(string objectName)
        {
            if (BaseType is CppArray)
            {
                return BaseType.DeclareObjectByFullName($"(&{objectName})");
            }

            return BaseType.DeclareObjectByFullName($"&{objectName}");
        }
    }

    class CppConst : CppCompoundType
    {
        public CppConst(ICppType baseType) : base(baseType)
        {
            if (baseType is CppReference) throw new ArgumentException("A const reference does not make sence. Do you need a reference to const?");
        }

        public override bool IsCopyConstructible => BaseType.IsCopyConstructible;

        public override string DeclareObject(string objectName) => BaseType.DeclareObject($"const {objectName}");

        public override string DeclareObjectByFullName(string objectName) => BaseType.DeclareObjectByFullName($"const {objectName}");
    }

    class CppArray : CppCompoundType
    {
        public ulong Length;

        public CppArray(ICppType baseType, ulong length) : base(baseType)
        {
            if (baseType is CppReference) throw new ArgumentException("An array of a reference type is not a valid type");
            Length = length;
        }

        public override bool IsCopyConstructible => true;

        public override string DeclareObject(string objectName) => BaseType.DeclareObject($"{objectName}[{Length}]");

        public override string DeclareObjectByFullName(string objectName) => BaseType.DeclareObjectByFullName($"{objectName}[{Length}]");
    }

    class CppTypeDef : CppScopedType
    {
        public ICppType OriginalType;

        public CppTypeDef(ICppScope scope, string typename, ICppType originalType) : base(scope, typename)
        {
            OriginalType = originalType;
        }

        public override string DeclareType() => $"using {Name} = {OriginalType.FullName};";
    }
}
