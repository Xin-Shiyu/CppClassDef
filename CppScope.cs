using System;
using System.Collections.Generic;
using System.Text;

namespace CppClassDef
{
    /// <summary>
    /// Represents a named scope
    /// </summary>
    public interface ICppScope
    {
        ICppScope Outer { get; }
        string FullName { get; }
        string Name { get; }
    }
}
