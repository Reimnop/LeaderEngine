using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using LeaderEngine;

namespace LeaderEditor.Logic.LeaderCompiler
{
    public class Compiler
    {
        public Type[] Compile(string source, out EmitResult outResult)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            string assemblyName = "Assembly-CSharp.dll";

            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                outResult = result;

                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    Type[] types = assembly.GetTypes();

                    List<Type> output = new List<Type>();

                    foreach (var t in types)
                    {
                        if (t.IsSubclassOf(typeof(Component)))
                            output.Add(t);
                    }

                    return output.ToArray();
                }
            }

            return null;
        }
    }
}
