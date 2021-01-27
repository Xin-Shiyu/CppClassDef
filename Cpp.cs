using System;
using System.Collections.Generic;
using System.Text;

namespace CppClassDef
{
    static class Cpp
    {
        public static class Std
        {
            public static CppExternClass Cls(string typename) => new CppExternClass(CppNamespace.Std, typename);
        }

        public static CppNamespace Ns(string name) => new CppNamespace(CppNamespace.Global, name);
        public static CppExternClass ECls(string typename) => new CppExternClass(CppNamespace.Global, typename);
        public static CppLocalClass LCls(string typename) => new CppLocalClass(CppNamespace.Global, typename);
        public static CppLocalClass LCls(string typename, CppLocalClass @base) => new CppLocalClass(CppNamespace.Global, typename, @base);
        public static CppLocalClass LCls(string typename, CppLocalClass @base, IDictionary<string, ICppType> fields)
        {
            var cls = LCls(typename, @base);
            
            foreach (var field in fields)
            {
                cls.Fields.Add(new CppField(CppMemberAccessibility.Public, false, field.Value, field.Key));
            }

            return cls;
        }
    }
}
