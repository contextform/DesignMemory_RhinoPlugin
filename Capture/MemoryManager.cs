using System;
using System.IO;
using Contextform.Models;
using Rhino;

namespace Contextform.Capture
{
    public class MemoryManager
    {
        private readonly string _dataDirectory;
        private DesignMemory _currentMemory;

        public DesignMemory CurrentMemory => _currentMemory;

        public MemoryManager()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Contextform");
            
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public void StartNewSession()
        {
            _currentMemory = new DesignMemory();
            RhinoApp.WriteLine($"Started new capture session: {_currentMemory.SessionId}");
        }

        public void AddCommand(CapturedCommand command)
        {
            if (_currentMemory == null)
            {
                StartNewSession();
            }

            command.Sequence = _currentMemory.Commands.Count + 1;
            _currentMemory.Commands.Add(command);
        }

        public void AddOriginalGeometry(string geometryId)
        {
            if (_currentMemory != null && !_currentMemory.OriginalGeometryIds.Contains(geometryId))
            {
                _currentMemory.OriginalGeometryIds.Add(geometryId);
            }
        }

        public void SaveCurrentSession()
        {
            if (_currentMemory == null || _currentMemory.Commands.Count == 0)
            {
                return;
            }

            try
            {
                string fileName = $"session_{_currentMemory.SessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(_dataDirectory, fileName);
                
                File.WriteAllText(filePath, _currentMemory.ToJson());
                RhinoApp.WriteLine($"Design memory saved: {fileName}");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error saving design memory: {ex.Message}");
            }
        }

        public DesignMemory LoadSession(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return DesignMemory.FromJson(json);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error loading design memory: {ex.Message}");
                return null;
            }
        }

        public void ClearCurrentSession()
        {
            _currentMemory = null;
        }
    }
}