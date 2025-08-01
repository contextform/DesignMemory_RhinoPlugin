using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Contextform.Models;
using Contextform.Utils;
using Rhino;

namespace Contextform.API
{
    public class ClaudeClient
    {
        private readonly HttpClient _httpClient;
        private const string CLAUDE_API_URL = "https://api.anthropic.com/v1/messages";

        public ClaudeClient()
        {
            // Enable TLS 1.2 for .NET Framework 4.8
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GenerateScript(DesignMemory memory, string prompt)
        {
            try
            {
                var settings = SettingsManager.Instance;
                
                // For MVP, use free endpoint by default
                if (settings.UseFreeEndpoint || string.IsNullOrEmpty(settings.ClaudeApiKey))
                {
                    return await CallFreeEndpoint(memory, prompt);
                }
                else
                {
                    return await CallClaudeAPI(memory, prompt);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error generating script: {ex.Message}");
                return null;
            }
        }

        private async Task<string> CallFreeEndpoint(DesignMemory memory, string prompt)
        {
            try
            {
                var settings = SettingsManager.Instance;
                RhinoApp.WriteLine($"Calling API endpoint: {settings.FreeApiEndpoint}");
                
                var requestBody = new
                {
                    memory = memory,
                    prompt = prompt
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(settings.FreeApiEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<FreeEndpointResponse>(responseContent);
                    return result?.Script;
                }
                else
                {
                    RhinoApp.WriteLine($"Free API error: {response.StatusCode} - {responseContent}");
                    return null; // No fallback - return null on API failure
                }
            }
            catch (HttpRequestException httpEx)
            {
                RhinoApp.WriteLine($"HTTP error: {httpEx.Message}");
                RhinoApp.WriteLine($"Inner exception: {httpEx.InnerException?.Message}");
                return null;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Unexpected error in CallFreeEndpoint: {ex.Message}");
                return null;
            }
        }

        private async Task<string> CallClaudeAPI(DesignMemory memory, string prompt)
        {
            var settings = SettingsManager.Instance;
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", settings.ClaudeApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var systemPrompt = CreateSystemPrompt();
            var userPrompt = CreateUserPrompt(memory, prompt);

            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 4000,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(CLAUDE_API_URL, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ClaudeResponse>(responseContent);
                return ExtractScriptFromResponse(result.Content[0].Text);
            }
            else
            {
                RhinoApp.WriteLine($"Claude API error: {response.StatusCode} - {responseContent}");
                return null;
            }
        }

        private string GenerateFallbackScript(DesignMemory memory, string prompt)
        {
            // Generate Python RhinoScriptSyntax based on the prompt
            var promptLower = prompt.ToLower();
            
            if (promptLower.Contains("sphere"))
            {
                return @"import rhinoscriptsyntax as rs
rs.AddSphere([0,0,0], 5)";
            }
            else if (promptLower.Contains("box"))
            {
                return @"import rhinoscriptsyntax as rs
rs.AddBox([
    [0,0,0], [10,0,0], [10,10,0], [0,10,0],
    [0,0,10], [10,0,10], [10,10,10], [0,10,10]
])";
            }
            else if (promptLower.Contains("cylinder"))
            {
                return @"import rhinoscriptsyntax as rs
rs.AddCylinder([0,0,0], [0,0,10], 5)";
            }
            else if (promptLower.Contains("organic") || promptLower.Contains("smooth"))
            {
                // For organic transformations, create a sphere with subdivision
                return @"import rhinoscriptsyntax as rs
sphere = rs.AddSphere([0,0,0], 5)
rs.RebuildSurface(sphere, degree=(3,3), pointcount=(20,20))";
            }
            else if (promptLower.Contains("array") || promptLower.Contains("pattern"))
            {
                // Create an array pattern
                return @"import rhinoscriptsyntax as rs
box = rs.AddBox([
    [0,0,0], [2,0,0], [2,2,0], [0,2,0],
    [0,0,2], [2,0,2], [2,2,2], [0,2,2]
])
for i in range(5):
    for j in range(5):
        if i > 0 or j > 0:
            rs.CopyObject(box, [i*3, j*3, 0])";
            }
            else
            {
                // Default: create a sphere
                return @"import rhinoscriptsyntax as rs
rs.AddSphere([0,0,0], 5)";
            }
        }

        private string CreateSystemPrompt()
        {
            return @"You are a Rhino 3D modeling expert that generates Python scripts using RhinoScriptSyntax based on design memory and user prompts.

Your task is to:
1. Analyze the captured design memory (sequence of Rhino commands)
2. Understand the design intent from the command sequence
3. Generate a Python script using RhinoScriptSyntax that creates geometry based on the user's transformation request
4. The script should create new geometry that replaces the original

Important guidelines:
- Use ONLY RhinoScriptSyntax (import rhinoscriptsyntax as rs)
- Generate complete, executable Python code
- Always start with: import rhinoscriptsyntax as rs
- Use RhinoScriptSyntax functions like: rs.AddSphere(), rs.AddBox(), rs.AddCylinder(), rs.AddLine(), rs.AddPolyline(), rs.AddCircle(), rs.AddArc(), rs.AddCurve(), rs.ExtrudeCurve(), rs.AddRevSrf(), rs.AddLoftSrf(), rs.BooleanUnion(), rs.BooleanDifference(), rs.MoveObject(), rs.CopyObject(), rs.RotateObject(), rs.ScaleObject(), rs.MirrorObject()
- The script will be executed directly in Rhino's Python interpreter
- Focus on the geometric transformation requested by the user
- Maintain the overall design structure while applying the requested changes

Return only the Python script code, wrapped in ```python code blocks.";
        }

        private string CreateUserPrompt(DesignMemory memory, string prompt)
        {
            var memoryJson = memory.ToJson();
            
            return $@"Design Memory (captured modeling session):
{memoryJson}

User Request: {prompt}

Original geometry IDs to replace: {string.Join(", ", memory.OriginalGeometryIds)}

Please generate a Python RhinoScriptSyntax script that:
1. Creates new geometry based on the design memory and user request
2. The new geometry should reflect the transformation requested
3. Maintains the design intent while applying the requested transformation

Return only the Python script.";
        }

        private string ExtractScriptFromResponse(string response)
        {
            // Extract Python code from markdown code blocks
            var startMarker = "```python";
            var endMarker = "```";
            
            var startIndex = response.IndexOf(startMarker);
            if (startIndex == -1)
            {
                // Try without language specifier
                startMarker = "```";
                startIndex = response.IndexOf(startMarker);
                if (startIndex == -1) return response; // No code block found, return as-is
            }
            
            startIndex += startMarker.Length;
            var endIndex = response.IndexOf(endMarker, startIndex);
            
            if (endIndex == -1) return response.Substring(startIndex).Trim();
            
            return response.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private class ClaudeResponse
        {
            [JsonProperty("content")]
            public ClaudeContent[] Content { get; set; }
        }

        private class ClaudeContent
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        private class FreeEndpointResponse
        {
            [JsonProperty("script")]
            public string Script { get; set; }
            
            [JsonProperty("success")]
            public bool Success { get; set; }
        }
    }
}