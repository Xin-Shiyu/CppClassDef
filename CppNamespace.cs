using System;
using System.Collections.Generic;
using System.Text;

namespace CppClassDef
{
    public sealed class CppNamespace : ICppScope
    {
        public static readonly CppNamespace Global = new CppNamespace(null, "");
        public static readonly CppNamespace Std = new CppNamespace(Global, "std");

        public CppNamespace Scope = Global;

        public CppNamespace(CppNamespace scope, string name)
        {
            if (scope == null ^ name == "") throw new ArgumentException();
            if (scope != null) Scope = scope;
            Name = name;
        }

        public ICppScope Outer { get => Scope; }

        public string FullName => 
            Scope == null
            ? ""
            : $"{Scope.FullName}::{Name}";

        public string Name { get; private set; }
    }
}
