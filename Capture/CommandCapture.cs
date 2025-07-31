using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.ApplicationSettings;
using Contextform.Models;

namespace Contextform.Capture
{
    public class CommandCapture : IDisposable
    {
        private readonly MemoryManager _memoryManager;
        private bool _isCapturing;
        private readonly HashSet<string> _supportedCommands;
        private Dictionary<Guid, RhinoObject> _beforeObjects;

        public bool IsCapturing => _isCapturing;

        public CommandCapture(MemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
            _supportedCommands = new HashSet<string>
            {
                "Box", "Sphere", "Cylinder", "Cone", "Plane",
                "Line", "Polyline", "Rectangle", "Circle", "Curve",
                "Move", "Copy", "Rotate", "Scale", "Mirror", "Array",
                "Fillet", "Chamfer", "Trim", "Split", "Join",
                "Extrude", "Loft", "Revolve",
                "BooleanUnion", "BooleanDifference", "BooleanIntersection",
                "Delete", "Group", "Ungroup"
            };
        }

        public void Start()
        {
            if (_isCapturing) return;

            _memoryManager.StartNewSession();
            _isCapturing = true;

            Rhino.Commands.Command.BeginCommand += OnCommandBeginning;
            Rhino.Commands.Command.EndCommand += OnCommandEnded;

            RhinoApp.WriteLine("Started capturing design memory...");
        }

        public void Stop()
        {
            if (!_isCapturing) return;

            _isCapturing = false;

            Rhino.Commands.Command.BeginCommand -= OnCommandBeginning;
            Rhino.Commands.Command.EndCommand -= OnCommandEnded;

            _memoryManager.SaveCurrentSession();
            RhinoApp.WriteLine("Stopped capturing design memory.");
        }

        private void OnCommandBeginning(object sender, Rhino.Commands.CommandEventArgs e)
        {
            if (!_isCapturing || !_supportedCommands.Contains(e.CommandEnglishName))
                return;

            // Capture state before command execution
            _beforeObjects = new Dictionary<Guid, RhinoObject>();
            foreach (var obj in RhinoDoc.ActiveDoc.Objects)
            {
                _beforeObjects[obj.Id] = obj;
            }
        }

        private void OnCommandEnded(object sender, Rhino.Commands.CommandEventArgs e)
        {
            if (!_isCapturing || !_supportedCommands.Contains(e.CommandEnglishName))
                return;

            try
            {
                var command = new CapturedCommand
                {
                    Command = e.CommandEnglishName,
                    Parameters = ExtractCommandParameters(e),
                    InputGeometry = GetInputGeometry(e),
                    OutputGeometry = GetOutputGeometry()
                };

                _memoryManager.AddCommand(command);

                // Track original geometry for replacement later
                foreach (var geoId in command.OutputGeometry)
                {
                    _memoryManager.AddOriginalGeometry(geoId);
                }

                RhinoApp.WriteLine($"Captured: {e.CommandEnglishName}");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error capturing command {e.CommandEnglishName}: {ex.Message}");
            }
        }

        private Dictionary<string, object> ExtractCommandParameters(Rhino.Commands.CommandEventArgs e)
        {
            var parameters = new Dictionary<string, object>();

            // Basic parameter extraction - this would need to be expanded
            // for more sophisticated command analysis
            switch (e.CommandEnglishName)
            {
                case "Box":
                    // Would extract corner points, dimensions, etc.
                    parameters["type"] = "rectangular";
                    break;
                case "Sphere":
                    // Would extract center, radius, etc.
                    parameters["type"] = "solid";
                    break;
                case "Move":
                    // Would extract translation vector
                    parameters["operation"] = "translation";
                    break;
                case "Scale":
                    // Would extract scale factor, origin
                    parameters["operation"] = "uniform_scale";
                    break;
                default:
                    parameters["command_type"] = e.CommandEnglishName.ToLower();
                    break;
            }

            return parameters;
        }

        private List<string> GetInputGeometry(Rhino.Commands.CommandEventArgs e)
        {
            // This would analyze selected objects before command execution
            // For MVP, return empty list - can be enhanced later
            return new List<string>();
        }

        private List<string> GetOutputGeometry()
        {
            var outputIds = new List<string>();

            try
            {
                // Find new objects created by the command
                var currentObjects = RhinoDoc.ActiveDoc.Objects.ToList();
                
                foreach (var obj in currentObjects)
                {
                    if (_beforeObjects == null || !_beforeObjects.ContainsKey(obj.Id))
                    {
                        outputIds.Add(obj.Id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error detecting output geometry: {ex.Message}");
            }

            return outputIds;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}