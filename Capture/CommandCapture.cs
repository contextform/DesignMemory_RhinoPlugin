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
        private List<CapturedCommand> _sessionCommands;
        private int _commandSequence;
        private Dictionary<Guid, Point3d> _beforeCenters;
        private Dictionary<Guid, BoundingBox> _beforeBoundingBoxes;

        public bool IsCapturing => _isCapturing;

        public CommandCapture(MemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
            _sessionCommands = new List<CapturedCommand>();
            _commandSequence = 0;
            _supportedCommands = new HashSet<string>
            {
                // Basic Primitives
                "Box", "Sphere", "Cylinder", "Cone", "Plane", "Torus",
                
                // 2D Curves
                "Line", "Polyline", "Rectangle", "Circle", "Curve", "Arc", "Ellipse",
                "Polygon", "InterpCrv", "CurveThroughPt",
                
                // Transformations
                "Move", "Copy", "Rotate", "Scale", "Mirror", "Array", "ArrayPolar", 
                "ArrayLinear", "ArrayCrv", "Orient", "Flow",
                
                // Surface Operations
                "Extrude", "ExtrudeCrv", "ExtrudeSrf", "Loft", "Revolve", "Sweep1", "Sweep2",
                "NetworkSrf", "Patch", "PlanarSrf",
                
                // Boolean Operations
                "BooleanUnion", "BooleanDifference", "BooleanIntersection", "BooleanSplit",
                
                // Editing
                "Fillet", "Chamfer", "Trim", "Split", "Join", "Explode", "Offset", "OffsetSrf",
                "Rebuild", "Simplify", "Fair", "MatchSrf",
                
                // Organization
                "Delete", "Group", "Ungroup", "Hide", "Show", "Lock", "Unlock",
                
                // Analysis
                "Distance", "Angle", "Area", "Volume", "CurvatureAnalysis"
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
            _beforeCenters = new Dictionary<Guid, Point3d>();
            _beforeBoundingBoxes = new Dictionary<Guid, BoundingBox>();
            
            foreach (var obj in RhinoDoc.ActiveDoc.Objects)
            {
                _beforeObjects[obj.Id] = obj;
                
                // Capture geometric properties for transformation tracking
                var bbox = obj.Geometry.GetBoundingBox(true);
                if (bbox.IsValid)
                {
                    _beforeCenters[obj.Id] = bbox.Center;
                    _beforeBoundingBoxes[obj.Id] = bbox;
                }
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

                // Extract detailed geometry data for output objects
                ExtractGeometryData(command);
                
                // Extract semantic data for the command
                ExtractSemanticData(command);
                
                // Extract command relationships
                ExtractCommandRelationships(command);
                
                // Extract transformation data for transformation commands
                if (IsTransformationCommand(command.Command))
                {
                    ExtractTransformationData(command);
                }
                
                // Add to session tracking
                command.Sequence = ++_commandSequence;
                _sessionCommands.Add(command);

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

        private void ExtractGeometryData(CapturedCommand command)
        {
            var doc = RhinoDoc.ActiveDoc;
            
            foreach (var objId in command.OutputGeometry)
            {
                if (!Guid.TryParse(objId, out Guid id)) continue;
                
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj == null) continue;

                var geoData = new GeometryData
                {
                    Id = objId,
                    Type = GetGeometryType(rhinoObj)
                };

                // Extract bounding box
                var bbox = rhinoObj.Geometry.GetBoundingBox(true);
                if (bbox.IsValid)
                {
                    geoData.BoundingBox = new BoundingBoxData
                    {
                        Min = new Point3dData(bbox.Min.X, bbox.Min.Y, bbox.Min.Z),
                        Max = new Point3dData(bbox.Max.X, bbox.Max.Y, bbox.Max.Z),
                        Center = new Point3dData(bbox.Center.X, bbox.Center.Y, bbox.Center.Z),
                        Dimensions = new DimensionsData
                        {
                            Width = bbox.Max.X - bbox.Min.X,
                            Height = bbox.Max.Y - bbox.Min.Y,
                            Depth = bbox.Max.Z - bbox.Min.Z
                        }
                    };
                }

                // Extract type-specific properties
                ExtractGeometryProperties(rhinoObj, geoData);

                command.GeometryData.Add(geoData);
            }
        }

        private string GetGeometryType(RhinoObject obj)
        {
            switch (obj.ObjectType)
            {
                case ObjectType.Point:
                    return "Point";
                case ObjectType.Curve:
                    return "Curve";
                case ObjectType.Surface:
                    return "Surface";
                case ObjectType.Brep:
                    return "Brep";
                case ObjectType.Mesh:
                    return "Mesh";
                case ObjectType.Extrusion:
                    return "Extrusion";
                default:
                    return obj.ObjectType.ToString();
            }
        }

        private void ExtractGeometryProperties(RhinoObject obj, GeometryData data)
        {
            switch (obj.ObjectType)
            {
                case ObjectType.Brep:
                    var brep = obj.Geometry as Brep;
                    if (brep != null)
                    {
                        data.Properties["volume"] = brep.GetVolume();
                        data.Properties["area"] = brep.GetArea();
                        data.Properties["is_solid"] = brep.IsSolid;
                        data.Properties["face_count"] = brep.Faces.Count;
                        data.Properties["edge_count"] = brep.Edges.Count;
                        data.Properties["vertex_count"] = brep.Vertices.Count;
                        
                        // Add corner points for boxes
                        if (brep.Faces.Count == 6 && brep.Vertices.Count == 8)
                        {
                            foreach (var vertex in brep.Vertices)
                            {
                                data.Points.Add(new Point3dData(vertex.Location.X, vertex.Location.Y, vertex.Location.Z));
                            }
                        }
                    }
                    break;

                case ObjectType.Curve:
                    var curve = obj.Geometry as Curve;
                    if (curve != null)
                    {
                        data.Properties["length"] = curve.GetLength();
                        data.Properties["is_closed"] = curve.IsClosed;
                        data.Properties["degree"] = curve.Degree;
                        
                        // Add start and end points
                        var start = curve.PointAtStart;
                        var end = curve.PointAtEnd;
                        data.Points.Add(new Point3dData(start.X, start.Y, start.Z));
                        data.Points.Add(new Point3dData(end.X, end.Y, end.Z));
                    }
                    break;

                case ObjectType.Point:
                    var point = obj.Geometry as Point;
                    if (point != null)
                    {
                        data.Points.Add(new Point3dData(point.Location.X, point.Location.Y, point.Location.Z));
                    }
                    break;

                case ObjectType.Mesh:
                    var mesh = obj.Geometry as Mesh;
                    if (mesh != null)
                    {
                        data.Properties["vertex_count"] = mesh.Vertices.Count;
                        data.Properties["face_count"] = mesh.Faces.Count;
                        data.Properties["is_closed"] = mesh.IsClosed;
                    }
                    break;
            }
        }

        private void ExtractSemanticData(CapturedCommand command)
        {
            var doc = RhinoDoc.ActiveDoc;
            
            switch (command.Command)
            {
                // Basic Primitives
                case "Box":
                    ExtractBoxSemanticData(command, doc);
                    break;
                case "Sphere":
                    ExtractSphereSemanticData(command, doc);
                    break;
                case "Cylinder":
                    ExtractCylinderSemanticData(command, doc);
                    break;
                case "Cone":
                    ExtractConeSemanticData(command, doc);
                    break;
                    
                // 2D Curves
                case "Rectangle":
                    ExtractRectangleSemanticData(command, doc);
                    break;
                case "Circle":
                    ExtractCircleSemanticData(command, doc);
                    break;
                case "Line":
                    ExtractLineSemanticData(command, doc);
                    break;
                case "Arc":
                    ExtractArcSemanticData(command, doc);
                    break;
                case "Polyline":
                    ExtractPolylineSemanticData(command, doc);
                    break;
                    
                // Transformations
                case "Move":
                    ExtractMoveSemanticData(command, doc);
                    break;
                case "Copy":
                    ExtractCopySemanticData(command, doc);
                    break;
                case "Scale":
                    ExtractScaleSemanticData(command, doc);
                    break;
                case "Rotate":
                    ExtractRotateSemanticData(command, doc);
                    break;
                case "Mirror":
                    ExtractMirrorSemanticData(command, doc);
                    break;
                case "Array":
                case "ArrayLinear":
                case "ArrayPolar":
                    ExtractArraySemanticData(command, doc);
                    break;
                    
                // Surface Operations
                case "Extrude":
                case "ExtrudeCrv":
                    ExtractExtrudeSemanticData(command, doc);
                    break;
                case "Loft":
                    ExtractLoftSemanticData(command, doc);
                    break;
                case "Revolve":
                    ExtractRevolveSemanticData(command, doc);
                    break;
                    
                // Boolean Operations
                case "BooleanUnion":
                case "BooleanDifference":
                case "BooleanIntersection":
                case "BooleanSplit":
                    ExtractBooleanSemanticData(command, doc);
                    break;
                    
                // Editing
                case "Fillet":
                    ExtractFilletSemanticData(command, doc);
                    break;
                case "Trim":
                case "Split":
                    ExtractTrimSplitSemanticData(command, doc);
                    break;
                    
                // Default
                default:
                    command.SemanticData["intent"] = $"Applied {command.Command.ToLower()} operation";
                    break;
            }
        }

        private void ExtractBoxSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            // Get the first (and likely only) created brep
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Brep brep && brep.IsSolid)
                {
                    var bbox = brep.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        // Extract semantic box data
                        command.SemanticData["creation_method"] = "corner_to_corner";
                        command.SemanticData["first_corner"] = new[] { bbox.Min.X, bbox.Min.Y, bbox.Min.Z };
                        command.SemanticData["second_corner"] = new[] { bbox.Max.X, bbox.Max.Y, bbox.Min.Z }; // Base corners
                        command.SemanticData["height"] = bbox.Max.Z - bbox.Min.Z;
                        
                        command.SemanticData["dimensions"] = new Dictionary<string, double>
                        {
                            ["width"] = bbox.Max.X - bbox.Min.X,
                            ["depth"] = bbox.Max.Y - bbox.Min.Y, 
                            ["height"] = bbox.Max.Z - bbox.Min.Z
                        };
                        
                        command.SemanticData["center"] = new[] { bbox.Center.X, bbox.Center.Y, bbox.Center.Z };
                        
                        var dims = command.SemanticData["dimensions"] as Dictionary<string, double>;
                        command.SemanticData["intent"] = $"Created box from ({bbox.Min.X:F1},{bbox.Min.Y:F1},{bbox.Min.Z:F1}) " +
                                                        $"with dimensions {dims["width"]:F1} x {dims["depth"]:F1} x {dims["height"]:F1}";
                        
                        // Add corner points for precise reconstruction
                        command.SemanticData["corner_points"] = new[]
                        {
                            new[] { bbox.Min.X, bbox.Min.Y, bbox.Min.Z }, // Bottom corners
                            new[] { bbox.Max.X, bbox.Min.Y, bbox.Min.Z },
                            new[] { bbox.Max.X, bbox.Max.Y, bbox.Min.Z },
                            new[] { bbox.Min.X, bbox.Max.Y, bbox.Min.Z },
                            new[] { bbox.Min.X, bbox.Min.Y, bbox.Max.Z }, // Top corners
                            new[] { bbox.Max.X, bbox.Min.Y, bbox.Max.Z },
                            new[] { bbox.Max.X, bbox.Max.Y, bbox.Max.Z },
                            new[] { bbox.Min.X, bbox.Max.Y, bbox.Max.Z }
                        };
                    }
                }
            }
        }

        private void ExtractSphereSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Brep brep && brep.IsSolid)
                {
                    var bbox = brep.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        var center = bbox.Center;
                        var radius = (bbox.Max.X - bbox.Min.X) / 2.0; // Approximate radius
                        
                        command.SemanticData["creation_method"] = "center_radius";
                        command.SemanticData["center"] = new[] { center.X, center.Y, center.Z };
                        command.SemanticData["radius"] = radius;
                        command.SemanticData["intent"] = $"Created sphere at ({center.X:F1},{center.Y:F1},{center.Z:F1}) with radius {radius:F1}";
                    }
                }
            }
        }

        private void ExtractCylinderSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Brep brep && brep.IsSolid)
                {
                    var bbox = brep.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        var baseCenter = new[] { bbox.Center.X, bbox.Center.Y, bbox.Min.Z };
                        var topCenter = new[] { bbox.Center.X, bbox.Center.Y, bbox.Max.Z };
                        var radius = (bbox.Max.X - bbox.Min.X) / 2.0;
                        var height = bbox.Max.Z - bbox.Min.Z;
                        
                        command.SemanticData["creation_method"] = "base_top_radius";
                        command.SemanticData["base_center"] = baseCenter;
                        command.SemanticData["top_center"] = topCenter;
                        command.SemanticData["radius"] = radius;
                        command.SemanticData["height"] = height;
                        command.SemanticData["intent"] = $"Created cylinder from ({baseCenter[0]:F1},{baseCenter[1]:F1},{baseCenter[2]:F1}) height {height:F1} radius {radius:F1}";
                    }
                }
            }
        }

        private void ExtractMoveSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "translation";
            
            // Enhanced move data with transformation tracking
            if (command.SemanticData.ContainsKey("transformation_data"))
            {
                var transformData = command.SemanticData["transformation_data"] as List<TransformationInfo>;
                if (transformData != null && transformData.Any())
                {
                    var avgTranslation = transformData
                        .Where(t => t.TranslationVector != null)
                        .Select(t => new { X = t.TranslationVector[0], Y = t.TranslationVector[1], Z = t.TranslationVector[2] })
                        .FirstOrDefault();
                        
                    if (avgTranslation != null)
                    {
                        command.SemanticData["translation_vector"] = new[] { avgTranslation.X, avgTranslation.Y, avgTranslation.Z };
                        command.SemanticData["translation_distance"] = Math.Sqrt(avgTranslation.X * avgTranslation.X + 
                                                                                   avgTranslation.Y * avgTranslation.Y + 
                                                                                   avgTranslation.Z * avgTranslation.Z);
                        command.SemanticData["intent"] = $"Moved objects by ({avgTranslation.X:F1}, {avgTranslation.Y:F1}, {avgTranslation.Z:F1})";
                    }
                    else
                    {
                        command.SemanticData["intent"] = "Moved selected objects";
                    }
                }
            }
            else
            {
                command.SemanticData["intent"] = "Moved selected objects";
            }
        }

        private void ExtractCopySemanticData(CapturedCommand command, RhinoDoc doc)
        {
            var copyCount = command.OutputGeometry.Count;
            command.SemanticData["operation_type"] = "duplication";
            command.SemanticData["copy_count"] = copyCount;
            command.SemanticData["intent"] = $"Copied objects - created {copyCount} new instances";
            
            // TODO: Capture copy positions and patterns
        }

        private void ExtractScaleSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "scaling";
            
            // Enhanced scale data with transformation tracking
            if (command.SemanticData.ContainsKey("transformation_data"))
            {
                var transformData = command.SemanticData["transformation_data"] as List<TransformationInfo>;
                if (transformData != null && transformData.Any())
                {
                    var scaleInfo = transformData.First();
                    var avgScale = (scaleInfo.ScaleX + scaleInfo.ScaleY + scaleInfo.ScaleZ) / 3.0;
                    
                    command.SemanticData["scale_factors"] = new Dictionary<string, double>
                    {
                        ["x"] = scaleInfo.ScaleX,
                        ["y"] = scaleInfo.ScaleY,
                        ["z"] = scaleInfo.ScaleZ
                    };
                    command.SemanticData["uniform_scale"] = Math.Abs(scaleInfo.ScaleX - scaleInfo.ScaleY) < 0.001 && 
                                                            Math.Abs(scaleInfo.ScaleY - scaleInfo.ScaleZ) < 0.001;
                    command.SemanticData["average_scale_factor"] = avgScale;
                    command.SemanticData["intent"] = $"Scaled objects by factor {avgScale:F2} (X:{scaleInfo.ScaleX:F2}, Y:{scaleInfo.ScaleY:F2}, Z:{scaleInfo.ScaleZ:F2})";
                }
                else
                {
                    command.SemanticData["intent"] = "Scaled selected objects";
                }
            }
            else
            {
                command.SemanticData["intent"] = "Scaled selected objects";
            }
        }

        private void ExtractBooleanSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            var operationType = command.Command.Replace("Boolean", "").ToLower();
            command.SemanticData["operation_type"] = operationType;
            command.SemanticData["intent"] = $"Boolean {operationType} operation";
            
            // TODO: Track which objects were the targets vs tools
            // This requires knowing what was selected before the operation
        }

        // 2D Curves Semantic Extraction
        private void ExtractRectangleSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Curve curve && curve.IsClosed)
                {
                    var bbox = curve.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        command.SemanticData["creation_method"] = "corner_to_corner";
                        command.SemanticData["first_corner"] = new[] { bbox.Min.X, bbox.Min.Y, bbox.Min.Z };
                        command.SemanticData["opposite_corner"] = new[] { bbox.Max.X, bbox.Max.Y, bbox.Min.Z };
                        command.SemanticData["dimensions"] = new Dictionary<string, double>
                        {
                            ["width"] = bbox.Max.X - bbox.Min.X,
                            ["height"] = bbox.Max.Y - bbox.Min.Y
                        };
                        command.SemanticData["center"] = new[] { bbox.Center.X, bbox.Center.Y, bbox.Center.Z };
                        
                        var dims = command.SemanticData["dimensions"] as Dictionary<string, double>;
                        command.SemanticData["intent"] = $"Created rectangle {dims["width"]:F1} x {dims["height"]:F1} at ({bbox.Center.X:F1},{bbox.Center.Y:F1})";
                    }
                }
            }
        }

        private void ExtractCircleSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Curve curve && curve.IsClosed)
                {
                    var bbox = curve.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        var center = bbox.Center;
                        var radius = (bbox.Max.X - bbox.Min.X) / 2.0;
                        
                        command.SemanticData["creation_method"] = "center_radius";
                        command.SemanticData["center"] = new[] { center.X, center.Y, center.Z };
                        command.SemanticData["radius"] = radius;
                        command.SemanticData["intent"] = $"Created circle at ({center.X:F1},{center.Y:F1},{center.Z:F1}) with radius {radius:F1}";
                    }
                }
            }
        }

        private void ExtractLineSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Curve curve)
                {
                    var start = curve.PointAtStart;
                    var end = curve.PointAtEnd;
                    var length = curve.GetLength();
                    
                    command.SemanticData["creation_method"] = "point_to_point";
                    command.SemanticData["start_point"] = new[] { start.X, start.Y, start.Z };
                    command.SemanticData["end_point"] = new[] { end.X, end.Y, end.Z };
                    command.SemanticData["length"] = length;
                    command.SemanticData["intent"] = $"Created line from ({start.X:F1},{start.Y:F1},{start.Z:F1}) to ({end.X:F1},{end.Y:F1},{end.Z:F1}) length {length:F1}";
                }
            }
        }

        private void ExtractArcSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Curve curve)
                {
                    var start = curve.PointAtStart;
                    var end = curve.PointAtEnd;
                    var length = curve.GetLength();
                    var bbox = curve.GetBoundingBox(true);
                    
                    command.SemanticData["creation_method"] = "start_end_point";
                    command.SemanticData["start_point"] = new[] { start.X, start.Y, start.Z };
                    command.SemanticData["end_point"] = new[] { end.X, end.Y, end.Z };
                    command.SemanticData["center"] = new[] { bbox.Center.X, bbox.Center.Y, bbox.Center.Z };
                    command.SemanticData["length"] = length;
                    command.SemanticData["intent"] = $"Created arc from ({start.X:F1},{start.Y:F1},{start.Z:F1}) to ({end.X:F1},{end.Y:F1},{end.Z:F1})";
                }
            }
        }

        private void ExtractPolylineSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Curve curve && curve is PolylineCurve polyline)
                {
                    var pointCount = polyline.PointCount;
                    var length = curve.GetLength();
                    var start = curve.PointAtStart;
                    var end = curve.PointAtEnd;
                    
                    command.SemanticData["creation_method"] = "point_sequence";
                    command.SemanticData["point_count"] = pointCount;
                    command.SemanticData["start_point"] = new[] { start.X, start.Y, start.Z };
                    command.SemanticData["end_point"] = new[] { end.X, end.Y, end.Z };
                    command.SemanticData["total_length"] = length;
                    command.SemanticData["is_closed"] = curve.IsClosed;
                    command.SemanticData["intent"] = $"Created polyline with {pointCount} points, length {length:F1}";
                }
            }
        }

        // Additional primitive shapes
        private void ExtractConeSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Brep brep && brep.IsSolid)
                {
                    var bbox = brep.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        var baseCenter = new[] { bbox.Center.X, bbox.Center.Y, bbox.Min.Z };
                        var apex = new[] { bbox.Center.X, bbox.Center.Y, bbox.Max.Z };
                        var baseRadius = (bbox.Max.X - bbox.Min.X) / 2.0;
                        var height = bbox.Max.Z - bbox.Min.Z;
                        
                        command.SemanticData["creation_method"] = "base_apex_radius";
                        command.SemanticData["base_center"] = baseCenter;
                        command.SemanticData["apex"] = apex;
                        command.SemanticData["base_radius"] = baseRadius;
                        command.SemanticData["height"] = height;
                        command.SemanticData["intent"] = $"Created cone from ({baseCenter[0]:F1},{baseCenter[1]:F1},{baseCenter[2]:F1}) height {height:F1} base radius {baseRadius:F1}";
                    }
                }
            }
        }

        // Transformation operations
        private void ExtractRotateSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "rotation";
            command.SemanticData["intent"] = "Rotated selected objects";
            
            // TODO: Calculate rotation angle and axis by comparing before/after positions
        }

        private void ExtractMirrorSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "reflection";
            command.SemanticData["intent"] = "Mirrored selected objects";
            
            // TODO: Capture mirror plane information
        }

        private void ExtractArraySemanticData(CapturedCommand command, RhinoDoc doc)
        {
            var arrayCount = command.OutputGeometry.Count;
            var arrayType = command.Command.Contains("Polar") ? "polar" : "linear";
            
            command.SemanticData["operation_type"] = $"{arrayType}_array";
            command.SemanticData["array_count"] = arrayCount;
            command.SemanticData["intent"] = $"Created {arrayType} array with {arrayCount} copies";
            
            // TODO: Capture array parameters (spacing, angle, etc.)
        }

        // Surface operations
        private void ExtractExtrudeSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            if (command.OutputGeometry.Count > 0 && 
                Guid.TryParse(command.OutputGeometry[0], out Guid id))
            {
                var rhinoObj = doc.Objects.FindId(id);
                if (rhinoObj?.Geometry is Brep brep)
                {
                    var bbox = brep.GetBoundingBox(true);
                    var height = bbox.Max.Z - bbox.Min.Z;
                    
                    command.SemanticData["operation_type"] = "extrusion";
                    command.SemanticData["extrude_height"] = height;
                    command.SemanticData["is_solid"] = brep.IsSolid;
                    command.SemanticData["intent"] = $"Extruded curve/surface by height {height:F1}";
                }
            }
        }

        private void ExtractLoftSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "loft";
            command.SemanticData["intent"] = "Created lofted surface between curves";
            
            // TODO: Track input curve count and loft options
        }

        private void ExtractRevolveSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "revolve";
            command.SemanticData["intent"] = "Revolved curve around axis";
            
            // TODO: Capture revolution axis and angle
        }

        // Editing operations
        private void ExtractFilletSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            command.SemanticData["operation_type"] = "fillet";
            command.SemanticData["intent"] = "Applied fillet to edges";
            
            // TODO: Capture fillet radius value
        }

        private void ExtractTrimSplitSemanticData(CapturedCommand command, RhinoDoc doc)
        {
            var operation = command.Command.ToLower();
            command.SemanticData["operation_type"] = operation;
            command.SemanticData["intent"] = $"Applied {operation} operation to geometry";
            
            // TODO: Track what was trimmed/split and what was used as cutting tool
        }

        private void ExtractCommandRelationships(CapturedCommand command)
        {
            // Determine command category
            command.Relationships.CommandCategory = DetermineCommandCategory(command.Command);
            
            // Determine workflow stage
            command.Relationships.WorkflowStage = DetermineWorkflowStage(command.Command);
            
            // Analyze dependencies on previous commands
            AnalyzeCommandDependencies(command);
            
            // Set design intent based on command sequence and type
            command.Relationships.DesignIntent = InferDesignIntent(command);
        }

        private string DetermineCommandCategory(string commandName)
        {
            var primitives = new[] { "Box", "Sphere", "Cylinder", "Cone", "Plane", "Torus" };
            var curves = new[] { "Line", "Polyline", "Rectangle", "Circle", "Curve", "Arc", "Ellipse", "Polygon" };
            var surfaces = new[] { "Extrude", "ExtrudeCrv", "ExtrudeSrf", "Loft", "Revolve", "Sweep1", "Sweep2", "NetworkSrf", "Patch", "PlanarSrf" };
            var transformations = new[] { "Move", "Copy", "Rotate", "Scale", "Mirror", "Array", "ArrayPolar", "ArrayLinear", "Orient", "Flow" };
            var booleans = new[] { "BooleanUnion", "BooleanDifference", "BooleanIntersection", "BooleanSplit" };
            var editing = new[] { "Fillet", "Chamfer", "Trim", "Split", "Join", "Explode", "Offset", "OffsetSrf", "Rebuild", "Simplify" };
            var analysis = new[] { "Distance", "Angle", "Area", "Volume", "CurvatureAnalysis" };
            
            if (primitives.Contains(commandName)) return "primitive";
            if (curves.Contains(commandName)) return "curve";
            if (surfaces.Contains(commandName)) return "surface";
            if (transformations.Contains(commandName)) return "transformation";
            if (booleans.Contains(commandName)) return "boolean";
            if (editing.Contains(commandName)) return "editing";
            if (analysis.Contains(commandName)) return "analysis";
            
            return "other";
        }

        private string DetermineWorkflowStage(string commandName)
        {
            var creation = new[] { "Box", "Sphere", "Cylinder", "Line", "Circle", "Rectangle", "Extrude", "Loft", "Revolve" };
            var modification = new[] { "Move", "Copy", "Rotate", "Scale", "Array", "BooleanUnion", "BooleanDifference", "Trim", "Split" };
            var finishing = new[] { "Fillet", "Chamfer", "Offset", "Rebuild", "Simplify" };
            var analysis = new[] { "Distance", "Angle", "Area", "Volume", "CurvatureAnalysis" };
            
            if (creation.Contains(commandName)) return "creation";
            if (modification.Contains(commandName)) return "modification";
            if (finishing.Contains(commandName)) return "finishing";
            if (analysis.Contains(commandName)) return "analysis";
            
            return "other";
        }

        private void AnalyzeCommandDependencies(CapturedCommand command)
        {
            // Analyze if this command operates on existing geometry
            if (IsTransformationCommand(command.Command))
            {
                // Transformation commands depend on existing geometry selection
                var recentCreationCommands = _sessionCommands
                    .Where(c => c.Relationships.WorkflowStage == "creation")
                    .Skip(Math.Max(0, _sessionCommands.Count(c => c.Relationships.WorkflowStage == "creation") - 3))
                    .Select(c => $"{c.Command}_{c.Sequence}")
                    .ToList();
                    
                command.Relationships.DependsOn.AddRange(recentCreationCommands);
                command.Relationships.TransformsGeometry.AddRange(recentCreationCommands);
            }
            
            // Boolean operations depend on multiple objects
            if (IsBooleanCommand(command.Command))
            {
                var eligibleCommands = _sessionCommands
                    .Where(c => c.Relationships.WorkflowStage == "creation" || c.Relationships.WorkflowStage == "modification")
                    .ToList();
                var recentObjects = eligibleCommands
                    .Skip(Math.Max(0, eligibleCommands.Count - 2))
                    .Select(c => $"{c.Command}_{c.Sequence}")
                    .ToList();
                    
                command.Relationships.DependsOn.AddRange(recentObjects);
            }
            
            // Surface operations often depend on curves
            if (IsSurfaceCommand(command.Command))
            {
                var curveCommands = _sessionCommands
                    .Where(c => c.Relationships.CommandCategory == "curve")
                    .ToList();
                var recentCurves = curveCommands
                    .Skip(Math.Max(0, curveCommands.Count - 2))
                    .Select(c => $"{c.Command}_{c.Sequence}")
                    .ToList();
                    
                command.Relationships.DependsOn.AddRange(recentCurves);
            }
        }

        private string InferDesignIntent(CapturedCommand command)
        {
            var sequence = _sessionCommands.Count;
            var category = command.Relationships.CommandCategory;
            var stage = command.Relationships.WorkflowStage;
            
            switch (stage)
            {
                case "creation":
                    if (sequence == 1)
                        return $"Started design by creating base {category} geometry";
                    else
                        return $"Added {category} element to expand the design";
                        
                case "modification":
                    return $"Modified existing geometry using {command.Command.ToLower()} operation";
                    
                case "finishing":
                    return $"Applied finishing touches with {command.Command.ToLower()}";
                    
                case "analysis":
                    return $"Analyzed design properties using {command.Command.ToLower()}";
                    
                default:
                    return $"Applied {command.Command.ToLower()} operation";
            }
        }

        private bool IsTransformationCommand(string command)
        {
            return new[] { "Move", "Copy", "Rotate", "Scale", "Mirror", "Array", "ArrayPolar", "ArrayLinear" }.Contains(command);
        }

        private bool IsBooleanCommand(string command)
        {
            return command.StartsWith("Boolean");
        }

        private bool IsSurfaceCommand(string command)
        {
            return new[] { "Extrude", "ExtrudeCrv", "Loft", "Revolve", "Sweep1", "Sweep2" }.Contains(command);
        }

        private void ExtractTransformationData(CapturedCommand command)
        {
            var doc = RhinoDoc.ActiveDoc;
            var transformationData = new Dictionary<string, object>();
            
            // Find objects that were transformed (existed before and after)
            var transformedObjects = new List<TransformationInfo>();
            
            foreach (var objId in command.OutputGeometry)
            {
                if (Guid.TryParse(objId, out Guid id))
                {
                    var currentObj = doc.Objects.FindId(id);
                    if (currentObj != null && _beforeObjects.ContainsKey(id))
                    {
                        // This object existed before and was transformed
                        var beforeObj = _beforeObjects[id];
                        var transformInfo = CalculateTransformation(beforeObj, currentObj, command.Command);
                        if (transformInfo != null)
                        {
                            transformedObjects.Add(transformInfo);
                        }
                    }
                }
            }
            
            // Also check for copy operations where new objects are created
            if (command.Command == "Copy" || command.Command.Contains("Array"))
            {
                var copiedObjects = AnalyzeCopyOperations(command, doc);
                transformedObjects.AddRange(copiedObjects);
            }
            
            if (transformedObjects.Any())
            {
                command.SemanticData["transformation_data"] = transformedObjects;
                command.SemanticData["transformation_count"] = transformedObjects.Count;
                
                // Calculate average transformation for summary
                if (transformedObjects.Count > 0)
                {
                    var avgTranslation = CalculateAverageTranslation(transformedObjects);
                    command.SemanticData["average_translation"] = avgTranslation;
                }
            }
        }

        private TransformationInfo CalculateTransformation(RhinoObject beforeObj, RhinoObject afterObj, string commandType)
        {
            if (!_beforeCenters.ContainsKey(beforeObj.Id) || !_beforeBoundingBoxes.ContainsKey(beforeObj.Id))
                return null;
                
            var beforeCenter = _beforeCenters[beforeObj.Id];
            var beforeBox = _beforeBoundingBoxes[beforeObj.Id];
            
            var afterBox = afterObj.Geometry.GetBoundingBox(true);
            if (!afterBox.IsValid) return null;
            
            var afterCenter = afterBox.Center;
            
            var transformInfo = new TransformationInfo
            {
                ObjectId = beforeObj.Id.ToString(),
                TransformationType = commandType,
                BeforeCenter = new[] { beforeCenter.X, beforeCenter.Y, beforeCenter.Z },
                AfterCenter = new[] { afterCenter.X, afterCenter.Y, afterCenter.Z }
            };
            
            // Calculate translation vector
            var translation = afterCenter - beforeCenter;
            transformInfo.TranslationVector = new[] { translation.X, translation.Y, translation.Z };
            transformInfo.TranslationDistance = translation.Length;
            
            // Calculate scale factors
            if (beforeBox.IsValid && afterBox.IsValid)
            {
                var beforeSize = beforeBox.Max - beforeBox.Min;
                var afterSize = afterBox.Max - afterBox.Min;
                
                if (beforeSize.X > 0.001) transformInfo.ScaleX = afterSize.X / beforeSize.X;
                if (beforeSize.Y > 0.001) transformInfo.ScaleY = afterSize.Y / beforeSize.Y;
                if (beforeSize.Z > 0.001) transformInfo.ScaleZ = afterSize.Z / beforeSize.Z;
            }
            
            return transformInfo;
        }

        private List<TransformationInfo> AnalyzeCopyOperations(CapturedCommand command, RhinoDoc doc)
        {
            var copyTransformations = new List<TransformationInfo>();
            
            // For copy/array operations, new objects are created
            // We need to find the pattern by analyzing the positions of new objects
            var newObjects = new List<RhinoObject>();
            foreach (var objId in command.OutputGeometry)
            {
                if (Guid.TryParse(objId, out Guid id))
                {
                    var obj = doc.Objects.FindId(id);
                    if (obj != null && !_beforeObjects.ContainsKey(id))
                    {
                        newObjects.Add(obj);
                    }
                }
            }
            
            // Analyze positions to determine copy pattern
            if (newObjects.Count > 1)
            {
                for (int i = 0; i < newObjects.Count; i++)
                {
                    var obj = newObjects[i];
                    var bbox = obj.Geometry.GetBoundingBox(true);
                    if (bbox.IsValid)
                    {
                        var transformInfo = new TransformationInfo
                        {
                            ObjectId = obj.Id.ToString(),
                            TransformationType = command.Command,
                            AfterCenter = new[] { bbox.Center.X, bbox.Center.Y, bbox.Center.Z },
                            CopyIndex = i + 1
                        };
                        
                        // If we have multiple copies, calculate spacing
                        if (i > 0)
                        {
                            var prevBbox = newObjects[i-1].Geometry.GetBoundingBox(true);
                            if (prevBbox.IsValid)
                            {
                                var spacing = bbox.Center - prevBbox.Center;
                                transformInfo.TranslationVector = new[] { spacing.X, spacing.Y, spacing.Z };
                                transformInfo.TranslationDistance = spacing.Length;
                            }
                        }
                        
                        copyTransformations.Add(transformInfo);
                    }
                }
            }
            
            return copyTransformations;
        }

        private double[] CalculateAverageTranslation(List<TransformationInfo> transformations)
        {
            if (!transformations.Any() || transformations.All(t => t.TranslationVector == null))
                return new[] { 0.0, 0.0, 0.0 };
                
            var validTransformations = transformations.Where(t => t.TranslationVector != null).ToList();
            if (!validTransformations.Any())
                return new[] { 0.0, 0.0, 0.0 };
                
            var avgX = validTransformations.Average(t => t.TranslationVector[0]);
            var avgY = validTransformations.Average(t => t.TranslationVector[1]);
            var avgZ = validTransformations.Average(t => t.TranslationVector[2]);
            
            return new[] { avgX, avgY, avgZ };
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class TransformationInfo
    {
        public string ObjectId { get; set; }
        public string TransformationType { get; set; }
        public double[] BeforeCenter { get; set; }
        public double[] AfterCenter { get; set; }
        public double[] TranslationVector { get; set; }
        public double TranslationDistance { get; set; }
        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public double ScaleZ { get; set; } = 1.0;
        public int CopyIndex { get; set; } // For copy/array operations
        public double RotationAngle { get; set; } // For rotation operations (future enhancement)
    }
}