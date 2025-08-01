# Contextform - AI Design Memory Plugin for Rhino

## Overview

Contextform captures your modeling process as intelligent "design memory" and uses AI to regenerate designs based on any criteria. Transform your designs by simply describing what you want: "make this organic", "optimize for 3D printing", "convert to sheet metal", etc.

The plugin understands not just *what* you modeled, but *how* and *why* - capturing relationships, dependencies, and design intent to generate contextually appropriate transformations.

## ✨ Key Features

### 🧠 **Intelligent Memory Capture**
- **50+ Supported Commands**: Comprehensive coverage of Rhino modeling operations
- **Semantic Understanding**: Captures dimensions, positions, and design intent for each command
- **Command Relationships**: Tracks dependencies and workflow patterns between operations
- **Transformation Tracking**: Records precise movement, scaling, and rotation data

### 🔗 **Workflow Intelligence**
- **Dependency Analysis**: Understands which objects depend on others
- **Workflow Stages**: Distinguishes creation, modification, finishing, and analysis operations
- **Pattern Recognition**: Identifies common modeling workflows (creation sequences, modification chains, boolean operations)
- **Design Intent**: Infers the purpose and context of each modeling operation

### 🤖 **Advanced AI Generation**
- **Context-Aware Scripts**: Generates Python scripts that respect original design relationships
- **RhinoScriptSyntax**: Uses precise coordinate data and geometric properties
- **Workflow Preservation**: Maintains command dependencies during transformations
- **Enhanced Prompts**: Leverages rich memory data for more accurate generation

### 💻 **User Experience**
- **Simple UI**: Easy-to-use panel with intuitive start/stop capture
- **Automatic Execution**: Seamlessly replaces original geometry with AI-generated versions
- **Rich Debugging**: Comprehensive logging and analysis output
- **Background Operation**: Captures modeling process without interrupting workflow

## 🛠 Installation

1. Download the .yak file from Food4Rhino
2. Install using Rhino's Package Manager or drag-and-drop into Rhino
3. Restart Rhino
4. Find the Contextform panel in Panels menu

## 🚀 Usage

### Basic Workflow
1. **Start Capture**: Click "Start Capture" and begin modeling
2. **Create Geometry**: Use any supported Rhino commands
3. **Stop Capture**: Click "Stop Capture" when finished
4. **AI Transform**: Enter a transformation prompt and click "Generate with AI"
5. **View Result**: Your original geometry will be replaced with the AI-generated version

### Supported Commands
The plugin now supports 50+ Rhino commands across all major categories:

- **Primitives**: Box, Sphere, Cylinder, Cone, Plane, Torus
- **Curves**: Line, Circle, Rectangle, Arc, Polyline, Ellipse, Polygon
- **Surfaces**: Extrude, Loft, Revolve, Sweep1, Sweep2, NetworkSrf
- **Transformations**: Move, Copy, Rotate, Scale, Mirror, Array operations
- **Boolean Operations**: Union, Difference, Intersection, Split
- **Editing**: Fillet, Chamfer, Trim, Split, Join, Offset, Rebuild
- **Analysis**: Distance, Angle, Area, Volume, Curvature Analysis

## 💡 Example Prompts

### Style Transformations
- "make this organic and flowing"
- "apply art deco geometric patterns"
- "create a biomimetic surface structure"

### Functional Modifications
- "optimize for 3D printing with support structures"
- "add ventilation patterns for airflow"
- "create sheet metal bend allowances"

### Parametric Variations
- "increase overall scale by 150%"
- "make the curves more angular"
- "add recursive subdivision patterns"

### Material-Specific
- "prepare for CNC machining"
- "add injection molding draft angles"
- "create lattice infill structure"

## 🏗 Architecture

### Memory Capture System
```
CommandCapture → MemoryManager → CommandDependencyManager
     ↓               ↓                    ↓
Semantic Data → Design Memory → Dependency Analysis
```

### Data Structure
Each captured command includes:
- **Semantic Data**: Dimensions, positions, creation methods
- **Relationships**: Dependencies, workflow stage, command category
- **Transformation Data**: Movement vectors, scale factors, rotation angles
- **Design Intent**: Inferred purpose and context

### AI Integration
- **Enhanced Prompts**: Utilize rich memory data for context-aware generation
- **Workflow Awareness**: Respect command dependencies and relationships
- **Precision Execution**: Use exact coordinate data for accurate reproduction

## 📋 Requirements

- **Rhino 8** (Windows)
- **Internet connection** for AI generation
- **.NET Framework 4.8** (included with Rhino)

## 🧪 Beta Version

This is a free beta version with full AI capabilities provided at no cost during the beta period. We're actively gathering feedback to improve the plugin's intelligence and capabilities.

## 🔧 Technical Details

### Built With
- **RhinoCommon SDK**: Core Rhino integration
- **Eto.Forms**: Cross-platform UI framework  
- **Anthropic Claude**: Advanced AI generation
- **RhinoScriptSyntax**: Python script execution
- **Newtonsoft.Json**: Data serialization

### Performance
- **Background Capture**: Minimal impact on modeling performance
- **Efficient Storage**: Optimized JSON serialization of design memory
- **Smart Dependencies**: Intelligent relationship analysis without overhead

## 🐛 Support & Feedback

- **Issues**: [GitHub Issues](https://github.com/contextform/rhino-plugin/issues)
- **Website**: [contextform.dev](https://contextform.dev)
- **Food4Rhino**: [Plugin Page](https://www.food4rhino.com/)

## 📈 Roadmap

- **Real-time Collaboration**: Share design memory across teams
- **Version Control**: Track design evolution over time
- **Custom AI Models**: Train on specific design patterns
- **Grasshopper Integration**: Parametric design memory capture
- **Multi-CAD Support**: Extend to other modeling platforms

## 🙏 Acknowledgments

- Built with [RhinoCommon SDK](https://developer.rhino3d.com/)
- UI powered by [Eto.Forms](https://github.com/picoe/Eto)
- AI generation by [Anthropic Claude](https://www.anthropic.com/)
- Cloud deployment on [Render](https://render.com/)

---

<div align="center">

**[Website](https://contextform.dev) • [Food4Rhino](https://www.food4rhino.com/) • [GitHub](https://github.com/contextform/rhino-plugin)**

Made with ❤️ by the Contextform Team

*Transforming CAD with AI-Native Design Memory*

</div>