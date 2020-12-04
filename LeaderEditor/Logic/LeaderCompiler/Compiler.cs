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
        //assembly name
        const string assemblyName = "InMemoryAssembly";

        public Type[] Compile(string[] sources, out EmitResult outResult)
        {
            //parse to syntax trees
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();

            foreach (var source in sources)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(source));
            }

            //get all assemblies to reference
            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            //compile
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            //load compiled il code into memory
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                outResult = result;

                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    return assembly.GetTypes();
                }
            }

            return null;
        }
    }
}
