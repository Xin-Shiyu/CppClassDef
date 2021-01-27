using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace CppClassDef
{
    class CppModule
    {
        public readonly string FileName;
        public readonly IList<CppScopedType> Types = new List<CppScopedType>();
        public readonly IList<string> StantardHeaders = new List<string>();
        public readonly IList<string> UserHeaders = new List<string>();
        public readonly string GuardMacro;

        public CppModule(string fileName, string guardMacro)
        {
            FileName = fileName;
            GuardMacro = guardMacro;
        }

        public void GenerateFile()
        {
            File.WriteAllText($"{FileName}.h", CreateHeader());
            File.WriteAllText($"{FileName}.cpp", CreateImpl());
        }

        public string CreateHeader()
        {
            var namespaces = new Dictionary<CppNamespace, string>();

            foreach (var type in Types)
            {
                if (type.Scope is CppNamespace ns)
                {
                    if (namespaces.ContainsKey(ns))
                    {
                        namespaces[ns] += type.DeclareType() + "\n";
                    }
                    else
                    {
                        namespaces.Add(ns, type.DeclareType() + "\n");
                    }
                }
            }

            var headerContent = "";

            foreach (var entry in namespaces)
            {
                var ns = entry.Key;
                var content = entry.Value;
                var it = ns;
                var part = content;
                while (it != CppNamespace.Global)
                {
                    part = $"namespace {ns.Name}\n{{\n{part}\n}}\n";
                    it = it.Scope;
                }

                headerContent += part;
            }

            var res = $@"#pragma once
#ifndef {GuardMacro}
#define {GuardMacro}

{StantardHeaders.Select(header => $"#include <{header}>").ToLines()}
{UserHeaders.Select(header => $"#include \"{header}\"").ToLines()}

{headerContent}

#endif";

            return res;
        }

        public string CreateImpl()
        {
            return 
                $"#include \"{FileName}.h\"\n" +
                $"{Types.OfType<CppLocalClass>().Select(type => type.DefineMethods()).JoinWith("\n")}";
        }
    }
}
