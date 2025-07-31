using System;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using Contextform.API;

namespace Contextform.UI
{
    [System.Runtime.InteropServices.Guid("B95AB78C-6C8E-4B8A-9B5D-1E2F3A4B5C6D")]
    public class ContextformPanel : Panel, IPanel
    {
        public static Guid PanelId => typeof(ContextformPanel).GUID;

        private Button _captureButton;
        private TextArea _promptInput;
        private Button _generateButton;
        private Label _statusLabel;
        private TextArea _memoryDisplay;
        private ClaudeClient _claudeClient;

        public ContextformPanel()
        {
            _claudeClient = new ClaudeClient();
            InitializeUI();
        }

        private void InitializeUI()
        {
            
            var layout = new DynamicLayout { Padding = 10, Spacing = new Size(5, 5) };

            // Status section
            _statusLabel = new Label 
            { 
                Text = "Ready to capture", 
                Font = SystemFonts.Bold(),
                TextColor = Colors.Green
            };
            layout.AddRow(_statusLabel);

            // Capture controls
            _captureButton = new Button 
            { 
                Text = "Start Capture",
                Height = 40
            };
            _captureButton.Click += OnCaptureButtonClick;
            layout.AddRow(_captureButton);

            layout.AddRow(new Label { Text = "Design Memory:" });
            
            // Memory display
            _memoryDisplay = new TextArea 
            { 
                ReadOnly = true,
                Height = 120,
                Text = "No commands captured yet..."
            };
            layout.AddRow(_memoryDisplay);

            layout.AddRow(new Label { Text = "AI Prompt:" });

            // Prompt input
            _promptInput = new TextArea 
            { 
                Text = "Describe how you want to transform your design...\nExample: 'make this organic', 'optimize for 3D printing'",
                Height = 80
            };
            layout.AddRow(_promptInput);

            // Generate button
            _generateButton = new Button 
            { 
                Text = "Generate with AI",
                Height = 40,
                Enabled = false
            };
            _generateButton.Click += OnGenerateButtonClick;
            layout.AddRow(_generateButton);

            // Add some spacing
            layout.Add(null);

            Content = layout;

            // Update UI periodically
            var timer = new UITimer { Interval = 1.0 };
            timer.Elapsed += UpdateUI;
            timer.Start();
        }

        private void OnCaptureButtonClick(object sender, EventArgs e)
        {
            var plugin = ContextformPlugin.Instance;
            
            if (plugin.IsCapturing)
            {
                plugin.StopCapture();
            }
            else
            {
                plugin.StartCapture();
            }

            UpdateCaptureButton();
        }

        private async void OnGenerateButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_promptInput.Text))
            {
                MessageBox.Show("Please enter a prompt describing how you want to transform your design.", 
                    "No Prompt", MessageBoxType.Warning);
                return;
            }

            var plugin = ContextformPlugin.Instance;
            var memory = plugin.MemoryManager.CurrentMemory;

            if (memory == null || memory.Commands.Count == 0)
            {
                MessageBox.Show("No design memory captured. Please start capture and create some geometry first.", 
                    "No Memory", MessageBoxType.Warning);
                return;
            }

            try
            {
                _generateButton.Enabled = false;
                _generateButton.Text = "Generating...";
                _statusLabel.Text = "Generating AI script...";
                _statusLabel.TextColor = Colors.Orange;

                var script = await _claudeClient.GenerateScript(memory, _promptInput.Text);
                
                if (!string.IsNullOrEmpty(script))
                {
                    // Execute the generated script
                    var executor = new Utils.ScriptExecutor();
                    executor.ExecuteScript(script, memory.OriginalGeometryIds);
                    
                    _statusLabel.Text = "Script executed successfully!";
                    _statusLabel.TextColor = Colors.Green;
                }
                else
                {
                    _statusLabel.Text = "Failed to generate script";
                    _statusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.TextColor = Colors.Red;
                RhinoApp.WriteLine($"Generate error: {ex.Message}");
            }
            finally
            {
                _generateButton.Enabled = true;
                _generateButton.Text = "Generate with AI";
            }
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            UpdateCaptureButton();
            UpdateMemoryDisplay();
        }

        private void UpdateCaptureButton()
        {
            var plugin = ContextformPlugin.Instance;
            
            if (plugin?.IsCapturing == true)
            {
                _captureButton.Text = "Stop Capture";
                _statusLabel.Text = "Capturing commands...";
                _statusLabel.TextColor = Colors.Orange;
                _generateButton.Enabled = false;
            }
            else
            {
                _captureButton.Text = "Start Capture";
                _statusLabel.Text = "Ready to capture";
                _statusLabel.TextColor = Colors.Green;
                
                var memory = plugin?.MemoryManager?.CurrentMemory;
                _generateButton.Enabled = memory != null && memory.Commands.Count > 0;
            }
        }

        private void UpdateMemoryDisplay()
        {
            var plugin = ContextformPlugin.Instance;
            var memory = plugin?.MemoryManager?.CurrentMemory;
            
            if (memory == null || memory.Commands.Count == 0)
            {
                _memoryDisplay.Text = "No commands captured yet...";
                return;
            }

            var text = $"Session: {memory.SessionId}\n";
            text += $"Commands: {memory.Commands.Count}\n\n";
            
            foreach (var cmd in memory.Commands)
            {
                text += $"{cmd.Sequence}. {cmd.Command}\n";
            }

            _memoryDisplay.Text = text;
        }

        #region IPanel Implementation
        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) { }
        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) { }
        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) { }
        #endregion
    }
}