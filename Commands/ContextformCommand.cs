using System;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using Contextform.UI;

namespace Contextform.Commands
{
    public class ContextformCommand : Command
    {
        public ContextformCommand()
        {
            Instance = this;
        }

        public static ContextformCommand Instance { get; private set; }

        public override string EnglishName => "Contextform";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Show the panel
                var panelId = ContextformPanel.PanelId;
                var visible = Panels.IsPanelVisible(panelId);
                
                if (!visible)
                {
                    Panels.OpenPanel(panelId);
                    RhinoApp.WriteLine("Contextform panel opened.");
                }
                else
                {
                    RhinoApp.WriteLine("Contextform panel is already open.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error opening Contextform panel: {ex.Message}");
                return Result.Failure;
            }
        }
    }
}