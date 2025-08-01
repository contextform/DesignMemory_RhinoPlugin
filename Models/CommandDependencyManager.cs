using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Contextform.Models
{
    public class CommandDependencyManager
    {
        private readonly List<CapturedCommand> _commands = new List<CapturedCommand>();
        private readonly Dictionary<string, CommandNode> _commandGraph = new Dictionary<string, CommandNode>();

        public void AddCommand(CapturedCommand command)
        {
            _commands.Add(command);
            UpdateDependencyGraph(command);
        }

        public List<CommandWorkflow> AnalyzeWorkflows()
        {
            var workflows = new List<CommandWorkflow>();
            
            // Group commands by workflow patterns
            var creationGroups = GroupCreationWorkflows();
            var modificationChains = AnalyzeModificationChains();
            var booleanOperations = AnalyzeBooleanOperations();
            
            workflows.AddRange(creationGroups);
            workflows.AddRange(modificationChains);
            workflows.AddRange(booleanOperations);
            
            return workflows;
        }

        private void UpdateDependencyGraph(CapturedCommand command)
        {
            var nodeId = $"{command.Command}_{command.Sequence}";
            var node = new CommandNode
            {
                Id = nodeId,
                Command = command,
                Dependencies = new List<string>(),
                Dependents = new List<string>()
            };

            // Analyze what this command depends on
            AnalyzeDependencies(command, node);
            
            _commandGraph[nodeId] = node;
            
            // Update existing nodes that this command might depend on
            UpdateExistingDependencies(node);
        }

        private void AnalyzeDependencies(CapturedCommand command, CommandNode node)
        {
            var commandType = command.Relationships.CommandCategory;
            var workflowStage = command.Relationships.WorkflowStage;
            
            switch (workflowStage)
            {
                case "modification":
                    // Modification commands typically depend on the most recent creation
                    var recentCreation = _commands
                        .Where(c => c.Relationships.WorkflowStage == "creation")
                        .OrderByDescending(c => c.Sequence)
                        .FirstOrDefault();
                    if (recentCreation != null)
                    {
                        var depId = $"{recentCreation.Command}_{recentCreation.Sequence}";
                        node.Dependencies.Add(depId);
                    }
                    break;
                    
                case "finishing":
                    // Finishing commands depend on recent creation or modification
                    var recentWork = _commands
                        .Where(c => c.Relationships.WorkflowStage == "creation" || c.Relationships.WorkflowStage == "modification")
                        .OrderByDescending(c => c.Sequence)
                        .FirstOrDefault();
                    if (recentWork != null)
                    {
                        var depId = $"{recentWork.Command}_{recentWork.Sequence}";
                        node.Dependencies.Add(depId);
                    }
                    break;
            }
            
            // Special cases for specific command types
            if (command.Command.StartsWith("Boolean"))
            {
                // Boolean operations typically need 2 objects
                var recentObjects = _commands
                    .Where(c => c.Relationships.WorkflowStage == "creation" || c.Relationships.WorkflowStage == "modification")
                    .OrderByDescending(c => c.Sequence)
                    .Take(2)
                    .ToList();
                    
                foreach (var obj in recentObjects)
                {
                    var depId = $"{obj.Command}_{obj.Sequence}";
                    if (!node.Dependencies.Contains(depId))
                    {
                        node.Dependencies.Add(depId);
                    }
                }
            }
            
            if (commandType == "surface" && (command.Command == "Loft" || command.Command == "Sweep1"))
            {
                // Surface operations often depend on curves
                var recentCurves = _commands
                    .Where(c => c.Relationships.CommandCategory == "curve")
                    .OrderByDescending(c => c.Sequence)
                    .Take(2)
                    .ToList();
                    
                foreach (var curve in recentCurves)
                {
                    var depId = $"{curve.Command}_{curve.Sequence}";
                    if (!node.Dependencies.Contains(depId))
                    {
                        node.Dependencies.Add(depId);
                    }
                }
            }
        }

        private void UpdateExistingDependencies(CommandNode newNode)
        {
            foreach (var depId in newNode.Dependencies)
            {
                if (_commandGraph.ContainsKey(depId))
                {
                    _commandGraph[depId].Dependents.Add(newNode.Id);
                }
            }
        }

        private List<CommandWorkflow> GroupCreationWorkflows()
        {
            var workflows = new List<CommandWorkflow>();
            var creationCommands = _commands.Where(c => c.Relationships.WorkflowStage == "creation").ToList();
            
            if (creationCommands.Count > 1)
            {
                var workflow = new CommandWorkflow
                {
                    Type = "creation_sequence",
                    Description = "Sequential geometry creation",
                    Commands = creationCommands.Select(c => $"{c.Command}_{c.Sequence}").ToList(),
                    DesignIntent = "Building up the design with multiple geometric elements"
                };
                workflows.Add(workflow);
            }
            
            return workflows;
        }

        private List<CommandWorkflow> AnalyzeModificationChains()
        {
            var workflows = new List<CommandWorkflow>();
            var chains = new List<List<CapturedCommand>>();
            
            // Find chains where modification commands follow creation commands
            foreach (var creationCmd in _commands.Where(c => c.Relationships.WorkflowStage == "creation"))
            {
                var chain = new List<CapturedCommand> { creationCmd };
                
                // Find modifications that follow this creation
                var followingMods = _commands
                    .Where(c => c.Sequence > creationCmd.Sequence && 
                               (c.Relationships.WorkflowStage == "modification" || c.Relationships.WorkflowStage == "finishing"))
                    .OrderBy(c => c.Sequence)
                    .Take(3) // Limit to prevent overly long chains
                    .ToList();
                
                chain.AddRange(followingMods);
                
                if (chain.Count > 1)
                {
                    chains.Add(chain);
                }
            }
            
            foreach (var chain in chains)
            {
                var workflow = new CommandWorkflow
                {
                    Type = "modification_chain",
                    Description = $"Creation followed by {chain.Count - 1} modifications",
                    Commands = chain.Select(c => $"{c.Command}_{c.Sequence}").ToList(),
                    DesignIntent = $"Iterative refinement: {string.Join(" → ", chain.Select(c => c.Command))}"
                };
                workflows.Add(workflow);
            }
            
            return workflows;
        }

        private List<CommandWorkflow> AnalyzeBooleanOperations()
        {
            var workflows = new List<CommandWorkflow>();
            var booleanCommands = _commands.Where(c => c.Command.StartsWith("Boolean")).ToList();
            
            foreach (var booleanCmd in booleanCommands)
            {
                var nodeId = $"{booleanCmd.Command}_{booleanCmd.Sequence}";
                if (_commandGraph.ContainsKey(nodeId))
                {
                    var node = _commandGraph[nodeId];
                    if (node.Dependencies.Count >= 2)
                    {
                        var workflow = new CommandWorkflow
                        {
                            Type = "boolean_operation",
                            Description = $"{booleanCmd.Command} combining multiple objects",
                            Commands = node.Dependencies.Concat(new[] { nodeId }).ToList(),
                            DesignIntent = $"Combining geometry using {booleanCmd.Command.Replace("Boolean", "").ToLower()} operation"
                        };
                        workflows.Add(workflow);
                    }
                }
            }
            
            return workflows;
        }

        public Dictionary<string, object> GetDependencyAnalysis()
        {
            var analysis = new Dictionary<string, object>();
            
            // Count command types
            var commandTypes = _commands
                .GroupBy(c => c.Relationships.CommandCategory)
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Count workflow stages
            var workflowStages = _commands
                .GroupBy(c => c.Relationships.WorkflowStage)
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Find the longest dependency chain
            var longestChain = FindLongestDependencyChain();
            
            analysis["command_count"] = _commands.Count;
            analysis["command_types"] = commandTypes;
            analysis["workflow_stages"] = workflowStages;
            analysis["longest_dependency_chain"] = longestChain;
            analysis["workflows"] = AnalyzeWorkflows();
            
            return analysis;
        }

        private List<string> FindLongestDependencyChain()
        {
            var longestChain = new List<string>();
            
            foreach (var node in _commandGraph.Values)
            {
                var chain = GetDependencyChain(node.Id, new HashSet<string>());
                if (chain.Count > longestChain.Count)
                {
                    longestChain = chain;
                }
            }
            
            return longestChain;
        }

        private List<string> GetDependencyChain(string nodeId, HashSet<string> visited)
        {
            if (visited.Contains(nodeId) || !_commandGraph.ContainsKey(nodeId))
                return new List<string>();
                
            visited.Add(nodeId);
            var node = _commandGraph[nodeId];
            var longestChain = new List<string> { nodeId };
            
            foreach (var depId in node.Dependencies)
            {
                var depChain = GetDependencyChain(depId, new HashSet<string>(visited));
                if (depChain.Count + 1 > longestChain.Count)
                {
                    longestChain = new List<string> { nodeId };
                    longestChain.AddRange(depChain);
                }
            }
            
            return longestChain;
        }
    }

    public class CommandNode
    {
        public string Id { get; set; }
        public CapturedCommand Command { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
        public List<string> Dependents { get; set; } = new List<string>();
    }

    public class CommandWorkflow
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("commands")]
        public List<string> Commands { get; set; } = new List<string>();
        
        [JsonProperty("design_intent")]
        public string DesignIntent { get; set; }
    }
}