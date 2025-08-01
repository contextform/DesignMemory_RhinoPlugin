using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Contextform.Models
{
    public class CapturedCommand
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonProperty("input_geometry")]
        public List<string> InputGeometry { get; set; }

        [JsonProperty("output_geometry")]
        public List<string> OutputGeometry { get; set; }

        [JsonProperty("geometry_data")]
        public List<GeometryData> GeometryData { get; set; }

        [JsonProperty("semantic_data")]
        public Dictionary<string, object> SemanticData { get; set; }

        [JsonProperty("relationships")]
        public CommandRelationships Relationships { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        public CapturedCommand()
        {
            Parameters = new Dictionary<string, object>();
            InputGeometry = new List<string>();
            OutputGeometry = new List<string>();
            GeometryData = new List<GeometryData>();
            SemanticData = new Dictionary<string, object>();
            Relationships = new CommandRelationships();
            Timestamp = DateTime.UtcNow;
        }
    }

    public class GeometryData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // Point, Curve, Surface, Brep, Mesh

        [JsonProperty("bounding_box")]
        public BoundingBoxData BoundingBox { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }

        [JsonProperty("points")]
        public List<Point3dData> Points { get; set; }

        public GeometryData()
        {
            Properties = new Dictionary<string, object>();
            Points = new List<Point3dData>();
        }
    }

    public class BoundingBoxData
    {
        [JsonProperty("min")]
        public Point3dData Min { get; set; }

        [JsonProperty("max")]
        public Point3dData Max { get; set; }

        [JsonProperty("center")]
        public Point3dData Center { get; set; }

        [JsonProperty("dimensions")]
        public DimensionsData Dimensions { get; set; }
    }

    public class Point3dData
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

        public Point3dData() { }

        public Point3dData(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class DimensionsData
    {
        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("depth")]
        public double Depth { get; set; }
    }

    public class CommandRelationships
    {
        [JsonProperty("depends_on")]
        public List<string> DependsOn { get; set; } = new List<string>();

        [JsonProperty("creates_input_for")]
        public List<string> CreatesInputFor { get; set; } = new List<string>();

        [JsonProperty("transforms_geometry")]
        public List<string> TransformsGeometry { get; set; } = new List<string>();

        [JsonProperty("workflow_stage")]
        public string WorkflowStage { get; set; } // "creation", "modification", "analysis", "finishing"

        [JsonProperty("command_category")]
        public string CommandCategory { get; set; } // "primitive", "curve", "surface", "transformation", "boolean", "editing"

        [JsonProperty("design_intent")]
        public string DesignIntent { get; set; } // High-level purpose of this command in the design
    }
}