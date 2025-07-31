using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Contextform.Models
{
    public class DesignMemory
    {
        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("commands")]
        public List<CapturedCommand> Commands { get; set; }

        [JsonProperty("original_geometry_ids")]
        public List<string> OriginalGeometryIds { get; set; }

        public DesignMemory()
        {
            SessionId = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            Commands = new List<CapturedCommand>();
            OriginalGeometryIds = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static DesignMemory FromJson(string json)
        {
            return JsonConvert.DeserializeObject<DesignMemory>(json);
        }
    }
}