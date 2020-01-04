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
using System.IO;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;

#endregion
namespace Amaterasu
{
    /// <summary>
    /// Allows for on-the-fly compilation and execution of C# code.
    /// </summary>
    public class AssemblyCompiler
    {
        #region Initialization

        /// <summary>
        /// Allows for on-the-fly compilation and execution of C# code.
        /// </summary>
        public AssemblyCompiler() { }

        #endregion
        #region Methods

        /// <summary>
        /// Create a managed assembly from code using in-memory techniques.
        /// </summary>
        /// <param name="code">The code to compile into a managed assembly.</param>
        /// <param name="imports">The required references that are being used by the provided code.</param>
        public Assembly Compile(string code, string[] imports = null)//, string outputDirectory = null)
        {
            string[] data = new string[] { code };
            return Compile(data, imports);//, outputDirectory);
        }

        /// <summary>
        /// Create a managed assembly from code using in-memory techniques.
        /// </summary>
        /// <param name="code">The code to compile into a managed assembly.</param>
        /// <param name="imports">The required references that are being used by the provided code.</param>
        public Assembly Compile(string[] code, string[] imports = null)//, string outputDirectory = null)
        {
            CompilerParameters CompilerParams = new CompilerParameters();

            CompilerParams.GenerateInMemory = true;
            CompilerParams.TreatWarningsAsErrors = false;
            CompilerParams.GenerateExecutable = false;
            CompilerParams.CompilerOptions = "/optimize";

            string[] references = (imports == null) ? new string[]{ "System.dll" } : imports;
            CompilerParams.ReferencedAssemblies.AddRange(references);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, code);

            if (compile.Errors.HasErrors)
            {
                string text = "Compile error: ";
                foreach (CompilerError ce in compile.Errors)
                    text += "rn" + ce.ToString();
                throw new Exception(text);
            }

            return compile.CompiledAssembly;
        }

        /// <summary>
        /// Run a method from a managed assembly.
        /// </summary>
        /// <param name="assembly">The assembly that contains the method to run.</param>
        /// <param name="rootNamespace">The namespace that contains the method to run and all its types.</param>
        /// <param name="rootClass">The class that contains the method to run.</param>
        /// <param name="methodName">The name of the method that should be run.</param>
        /// <param name="parameters">An array of objects that should be passed to the method as parameters.</param>
        public object Run(Assembly assembly, string rootNamespace, string rootClass, string methodName, object[] parameters = null)
        {
            try
            {
                Module module = assembly.GetModules()[0];
                Type mt = null;
                MethodInfo methInfo = null;

                if (module != null)
                    mt = module.GetType(rootNamespace + "." + rootClass);
                if (mt != null)
                    methInfo = mt.GetMethod(methodName);
                if (methInfo != null)
                    return methInfo.Invoke(null, parameters); //new object[] { "Hello, World!" });
            }
            catch (Exception ex) { throw ex; }
            return null;
        }

        /// <summary>
        /// Obtains all modules, types, and methods within a managed assembly.
        /// </summary>
        /// <param name="assembly">The assembly to obtain information about.</param>
        public AssemblyInfo GetAssemblyInfo(Assembly assembly)
        {
            AssemblyInfo info = new AssemblyInfo();
            foreach (Module m in assembly.GetModules())
            {
                info.AddModule(m);
                foreach (Type t in m.GetTypes())
                {
                    info.AddType(t);
                    foreach (MethodInfo mi in t.GetMethods())
                    {
                        info.AddMethod(mi);
                    }
                }
            }
            return info;
        }

        #endregion
    }
}
