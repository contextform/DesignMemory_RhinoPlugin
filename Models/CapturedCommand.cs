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

        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        public CapturedCommand()
        {
            Parameters = new Dictionary<string, object>();
            InputGeometry = new List<string>();
            OutputGeometry = new List<string>();
            Timestamp = DateTime.UtcNow;
        }
    }
}