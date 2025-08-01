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
                // Validate script before execution
                var validationError = ValidateScript(scriptCode);
                if (!string.IsNullOrEmpty(validationError))
                {
                    RhinoApp.WriteLine($"❌ Script validation failed: {validationError}");
                    RhinoApp.WriteLine($"Generated script:\n{scriptCode}");
                    return;
                }
                
                // Execute Python script using Rhino's Python script engine
                RhinoApp.WriteLine("✅ Script validation passed - executing Python script...");
                
                // Create a temporary Python script file
                var tempPath = System.IO.Path.GetTempFileName();
                tempPath = System.IO.Path.ChangeExtension(tempPath, ".py");
                
                try
                {
                    // Write the Python script to the temp file
                    System.IO.File.WriteAllText(tempPath, scriptCode);
                    RhinoApp.WriteLine($"📄 Script written to: {tempPath}");
                    
                    // Execute the Python script file
                    var command = $"_-RunPythonScript \"{tempPath}\"";
                    var result = RhinoApp.RunScript(command, true);
                    
                    if (!result)
                    {
                        RhinoApp.WriteLine("❌ Python script execution failed");
                        RhinoApp.WriteLine($"Generated script:\n{scriptCode}");
                        RhinoApp.WriteLine($"Temp file preserved at: {tempPath}");
                        return; // Don't delete temp file for debugging
                    }
                    else
                    {
                        RhinoApp.WriteLine("✅ Python script executed successfully");
                    }
                }
                finally
                {
                    // Clean up the temp file only if execution succeeded
                    if (System.IO.File.Exists(tempPath))
                    {
                        try { System.IO.File.Delete(tempPath); } catch { }
                    }
                }
                
                // Redraw views to show the new geometry
                doc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"❌ Error in ExecuteDirectScript: {ex.Message}");
                RhinoApp.WriteLine($"Generated script:\n{scriptCode}");
                throw;
            }
        }

        private string ValidateScript(string scriptCode)
        {
            // Check for common invalid RhinoScriptSyntax functions
            var invalidFunctions = new[]
            {
                "rs.RotatePoint(", "rs.TransformPoint(", "rs.MovePoint(", 
                "rs.ScalePoint(", "rs.CopyPoint(", "rs.MirrorPoint("
            };
            
            foreach (var invalidFunc in invalidFunctions)
            {
                if (scriptCode.Contains(invalidFunc))
                {
                    return $"Invalid RhinoScriptSyntax function: {invalidFunc.TrimEnd('(')} does not exist. Use rs.PointTransform() with transformation matrix or object transformation functions instead.";
                }
            }
            
            // Check for basic Python import
            if (!scriptCode.Contains("import rhinoscriptsyntax") && !scriptCode.Contains("rhinoscriptsyntax as rs"))
            {
                return "Script must import rhinoscriptsyntax (import rhinoscriptsyntax as rs)";
            }
            
            // Check for common syntax issues
            if (scriptCode.Contains("rs.") && !scriptCode.Contains("import rhinoscriptsyntax as rs"))
            {
                return "Script uses 'rs.' but doesn't import 'rhinoscriptsyntax as rs'";
            }
            
            return null; // No validation errors
        }
    }
}