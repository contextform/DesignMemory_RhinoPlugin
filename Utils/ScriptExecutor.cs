using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Commands;

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

                // Execute the script directly without compilation
                ExecuteDirectScript(scriptCode);

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

        private void ExecuteDirectScript(string scriptCode)
        {
            var doc = RhinoDoc.ActiveDoc;
            
            try
            {
                // Execute Python script using Rhino's Python script engine
                RhinoApp.WriteLine("Executing Python script...");
                
                // Create a temporary Python script file
                var tempPath = System.IO.Path.GetTempFileName();
                tempPath = System.IO.Path.ChangeExtension(tempPath, ".py");
                
                try
                {
                    // Write the Python script to the temp file
                    System.IO.File.WriteAllText(tempPath, scriptCode);
                    
                    // Execute the Python script file
                    var command = $"_-RunPythonScript \"{tempPath}\"";
                    var result = RhinoApp.RunScript(command, true);
                    
                    if (!result)
                    {
                        RhinoApp.WriteLine("Python script execution failed");
                    }
                    else
                    {
                        RhinoApp.WriteLine("Python script executed successfully");
                    }
                }
                finally
                {
                    // Clean up the temp file
                    if (System.IO.File.Exists(tempPath))
                    {
                        try { System.IO.File.Delete(tempPath); } catch { }
                    }
                }
                
                // Log the script that was received
                RhinoApp.WriteLine($"Script processed: {scriptCode.Substring(0, Math.Min(100, scriptCode.Length))}...");
                
                // Redraw views to show the new geometry
                doc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in ExecuteDirectScript: {ex.Message}");
                throw;
            }
        }
    }
}