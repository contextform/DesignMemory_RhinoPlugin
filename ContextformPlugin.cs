using System;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using Contextform.UI;
using Contextform.Capture;

namespace Contextform
{
    [System.Runtime.InteropServices.Guid("3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e")]
    public class ContextformPlugin : PlugIn
    {
        private static ContextformPlugin _instance;
        private CommandCapture _commandCapture;
        private MemoryManager _memoryManager;

        public static ContextformPlugin Instance => _instance;

        public CommandCapture CommandCapture => _commandCapture;
        public MemoryManager MemoryManager => _memoryManager;

        public ContextformPlugin()
        {
            _instance = this;
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                _memoryManager = new MemoryManager();
                _commandCapture = new CommandCapture(_memoryManager);

                // Register the panel
                Panels.RegisterPanel(this, typeof(ContextformPanel), "Contextform", null);

                RhinoApp.WriteLine("Contextform plugin loaded successfully.");
                return LoadReturnCode.Success;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load Contextform plugin: {ex.Message}";
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        protected override void OnShutdown()
        {
            _commandCapture?.Dispose();
            base.OnShutdown();
        }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

        public void StartCapture()
        {
            _commandCapture?.Start();
        }

        public void StopCapture()
        {
            _commandCapture?.Stop();
        }

        public bool IsCapturing => _commandCapture?.IsCapturing ?? false;
    }
}