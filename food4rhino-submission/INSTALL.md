# Contextform Plugin Installation Guide

## Quick Installation via Package Manager

1. **Download** the `.yak` file from Food4Rhino
2. **Open Rhino 8**
3. **Install via Package Manager**:
   - Type `PackageManager` in Rhino command line
   - Click "Install from File"
   - Select the downloaded `.yak` file
   - Click "Install"
4. **Restart Rhino**
5. **Open Panel**: Go to **Panels** → **Contextform** to start using the plugin

## Manual Installation (Alternative)

1. **Download files** from Food4Rhino
2. **Locate Rhino Plugin Folder**:
   - Windows: `%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins`
3. **Create plugin directory**: `Contextform`
4. **Copy files**:
   - `Contextform.dll`
   - `Newtonsoft.Json.dll`
5. **Restart Rhino**
6. **Verify Installation**: Check if "Contextform" appears in the Panels menu

## First Use

1. **Open Panel**: Panels → Contextform
2. **Start Capture**: Click "Start Capture"
3. **Model Something**: Create a box, sphere, or any geometry
4. **Stop Capture**: Click "Stop Capture"  
5. **Transform**: Enter a prompt like "make this organic" and click "Generate with AI"
6. **See Results**: Your geometry will be replaced with the AI-generated version

## Supported Rhino Commands

The plugin captures 50+ commands across all major categories:
- **Primitives**: Box, Sphere, Cylinder, Cone, Plane, Torus
- **Curves**: Line, Circle, Rectangle, Arc, Polyline, Ellipse
- **Surfaces**: Extrude, Loft, Revolve, Sweep operations
- **Transformations**: Move, Copy, Rotate, Scale, Mirror, Array
- **Boolean Operations**: Union, Difference, Intersection, Split
- **Editing**: Fillet, Chamfer, Trim, Split, Join, Offset

## Troubleshooting

### Plugin Not Appearing
- Ensure Rhino 8 is installed (plugin requires Rhino 8+)
- Restart Rhino after installation
- Check Windows → Panels menu for "Contextform"

### Internet Connection Required
- The plugin requires internet connection for AI generation
- Ensure firewall allows Rhino to access the internet

### Command Not Captured
- Plugin supports 50+ commands - if a command isn't supported, it won't be captured
- Supported commands are listed above and in the documentation

## Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/contextform/DesignMemory_RhinoPlugin/issues)
- **Documentation**: [Full README](https://github.com/contextform/DesignMemory_RhinoPlugin)
- **Website**: [contextform.dev](https://contextform.dev)

## Requirements

- **Rhino 8** (Windows)
- **Internet connection** for AI generation
- **.NET Framework 4.8** (included with Rhino 8)

---

**Free Beta Version** - Full AI capabilities provided at no cost during beta period.