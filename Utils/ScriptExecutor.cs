using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using Rhino;
using Rhino.DocObjects;

namespace Contextform.Utils
{
    public class ScriptExecutor
    {
        public void ExecuteScript(string scriptCode, List<string> originalGeometryIds)
        {
            try
            {
                // First, delete the original geometry
                DeleteOriginalGeometry(originalGeometryIds);

                // Compile and execute the generated script
                var compiledAssembly = CompileScript(scriptCode);
                ExecuteCompiledScript(compiledAssembly);

                RhinoApp.WriteLine("Script executed successfully - geometry replaced!");
                RhinoDoc.ActiveDoc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error executing script: {ex.Message}");
                throw;
            }
        }

        private void DeleteOriginalGeometry(List<string> geometryIds)
        {
            var doc = RhinoDoc.ActiveDoc;
            var objectsToDelete = new List<RhinoObject>();

            foreach (var idString in geometryIds)
            {
                if (Guid.TryParse(idString, out Guid id))
                {
                    var obj = doc.Objects.FindId(id);
                    if (obj != null)
                    {
                        objectsToDelete.Add(obj);
                    }
                }
            }

            foreach (var obj in objectsToDelete)
            {
                doc.Objects.Delete(obj, true);
            }

            RhinoApp.WriteLine($"Deleted {objectsToDelete.Count} original objects");
        }

        private Assembly CompileScript(string scriptCode)
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();
            
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            
            // Add required references
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add(@"C:\Program Files\Rhino 8\System\RhinoCommon.dll");

            // Wrap the script in a class if it's not already
            var fullScript = WrapScriptInClass(scriptCode);

            var results = provider.CompileAssemblyFromSource(parameters, fullScript);

            if (results.Errors.HasErrors)
            {
                var errorMessages = results.Errors.Cast<CompilerError>()
                    .Select(error => $"Line {error.Line}: {error.ErrorText}")
                    .ToArray();
                
                throw new Exception($"Script compilation failed:\n{string.Join("\n", errorMessages)}");
            }

            return results.CompiledAssembly;
        }

        private string WrapScriptInClass(string scriptCode)
        {
            // Check if the script already contains a class definition
            if (scriptCode.Contains("class ") && scriptCode.Contains("public static void Execute"))
            {
                return AddUsingStatements(scriptCode);
            }

            // Wrap the script in a class
            var wrappedScript = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

public class GeneratedScript
{{
    public static void Execute()
    {{
        var doc = RhinoDoc.ActiveDoc;
        
{IndentCode(scriptCode, 2)}
        
        doc.Views.Redraw();
    }}
}}";

            return wrappedScript;
        }

        private string AddUsingStatements(string scriptCode)
        {
            var usingStatements = @"using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

";
            
            if (!scriptCode.Contains("using System;"))
            {
                return usingStatements + scriptCode;
            }
            
            return scriptCode;
        }

        private string IndentCode(string code, int indentLevel)
        {
            var indentString = new string(' ', indentLevel * 4);
            var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            return string.Join("\n", lines.Select(line => 
                string.IsNullOrWhiteSpace(line) ? line : indentString + line));
        }

        private void ExecuteCompiledScript(Assembly assembly)
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.GetMethod("Execute") != null);
            
            if (type == null)
            {
                throw new Exception("No Execute method found in generated script");
            }

            var executeMethod = type.GetMethod("Execute");
            
            if (executeMethod == null || !executeMethod.IsStatic)
            {
                throw new Exception("Execute method must be public and static");
            }

            executeMethod.Invoke(null, null);
        }
    }
}