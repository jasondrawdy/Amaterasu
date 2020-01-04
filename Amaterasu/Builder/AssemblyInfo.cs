/*
==============================================================================
Copyright © Jason Drawdy (CloneMerge)

All rights reserved.

The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Except as contained in this notice, the name of the above copyright holder
shall not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization.
==============================================================================
*/

#region Imports

using System;
using System.Reflection;

#endregion
namespace Amaterasu
{
    public class AssemblyInfo
    {
        #region Variables

        /// <summary>
        /// A collection of modules found within a managed assembly.
        /// </summary>
        public Module[] Modules { get; private set; }
        /// <summary>
        /// A collection of types found within a managed assembly.
        /// </summary>
        public Type[] Types { get; private set; }
        /// <summary>
        /// A collection of all methods found within a managed assembly.
        /// </summary>
        public MethodInfo[] Methods { get; private set; }

        #endregion
        #region Initialization

        /// <summary>
        /// A collection of modules, types, and methods within a managed assembly.
        /// </summary>
        public AssemblyInfo() { }
        /// <summary>
        /// A collection of modules, types, and methods within a managed assembly.
        /// </summary>
        /// <param name="modules">An array of modules from an assembly.</param>
        /// <param name="types">An array of types from an assembly.</param>
        /// <param name="methods">An array of methods from an assembly.</param>
        public AssemblyInfo(Module[] modules, Type[] types, MethodInfo[] methods)
        {
            Modules = modules;
            Types = types;
            Methods = methods;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Adds a module to the current <see cref="Modules"/> collection.
        /// </summary>
        /// <param name="module">The module to add to the collection.</param>
        public bool AddModule(Module module)
        {
            try
            {
                Module[] modules = new Module[Modules.Length + 1];
                Modules.CopyTo(modules, 0);
                modules[modules.Length] = module;
                Modules = modules;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Adds a type to the current <see cref="Types"/> collection.
        /// </summary>
        /// <param name="type">The type to add to the collection.</param>
        public bool AddType(Type type)
        {
            try
            {
                Type[] types = new Type[Types.Length + 1];
                Types.CopyTo(types, 0);
                types[types.Length] = type;
                Types = types;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Adds a method to the current <see cref="Methods"/> collection.
        /// </summary>
        /// <param name="method">The method to add to the collection.</param>
        public bool AddMethod(MethodInfo method)
        {
            try
            {
                MethodInfo[] methods = new MethodInfo[Methods.Length + 1];
                Methods.CopyTo(methods, 0);
                methods[Types.Length] = method;
                Methods = methods;
                return true;
            }
            catch { return false; }
        }

        #endregion
    }
}