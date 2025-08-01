# 🚀 Contextform v1.1.0 - Major Intelligence Upgrade

## Download & Install

### Quick Install (Recommended)
**📥 [Download contextform-1.1.0-rh8_21-any.yak](https://github.com/contextform/DesignMemory_RhinoPlugin/releases/download/v1.1.0/contextform-1.1.0-rh8_21-any.yak)**

1. Download the `.yak` file above
2. Open Rhino 8
3. Type `PackageManager` in command line
4. Click "Install from File" → Select the `.yak` file
5. Restart Rhino
6. Find **Contextform** in Panels menu

### Manual Install (Alternative)
**📥 [Download Contextform.rhp](https://github.com/contextform/DesignMemory_RhinoPlugin/releases/download/v1.1.0/Contextform.rhp)**

1. Download the `.rhp` file above
2. In Rhino: Tools → Options → Plug-ins → Install
3. Select the downloaded `.rhp` file
4. Restart Rhino

---

## 🧠 What's New: AI-Native Intelligence

This release transforms Contextform from a simple command recorder into an **intelligent design companion** that understands your modeling workflow and intent.

### 🚀 Major Enhancements

#### **50+ Command Support**
Expanded from 25 to **50+ supported Rhino commands** across all categories:
- **Primitives**: Box, Sphere, Cylinder, Cone, Plane, Torus
- **Curves**: Line, Circle, Rectangle, Arc, Polyline, Ellipse, Polygon
- **Surfaces**: Extrude, Loft, Revolve, Sweep1, Sweep2, NetworkSrf
- **Transformations**: Move, Copy, Rotate, Scale, Mirror, Array operations
- **Boolean Operations**: Union, Difference, Intersection, Split
- **Editing**: Fillet, Chamfer, Trim, Split, Join, Offset, Rebuild
- **Analysis**: Distance, Angle, Area, Volume, Curvature Analysis

#### **Semantic Understanding**
- **Rich Semantic Data**: Captures dimensions, positions, creation methods, and design intent
- **Command Relationships**: Tracks dependencies and workflow patterns between operations
- **Transformation Tracking**: Records precise movement vectors, scale factors, rotation angles
- **Design Intent Inference**: AI understands the *why* behind each modeling step

#### **Workflow Intelligence**
- **Dependency Analysis**: Understands which objects depend on others
- **Pattern Recognition**: Identifies creation sequences, modification chains, boolean operations
- **Workflow Stages**: Distinguishes creation, modification, finishing, and analysis operations
- **Context-Aware Generation**: AI respects original design relationships during transformations

---

## 💡 Try These Enhanced Prompts

With the new intelligence, you can now use more sophisticated transformations:

### **Workflow-Aware Transformations**
- *"Respect the original modeling sequence but make it organic"*
- *"Keep the boolean relationships but scale everything by 150%"*
- *"Maintain the array pattern but change the base geometry to cylinders"*

### **Semantic-Based Modifications**
- *"Increase all dimensions by 20% while preserving proportions"*
- *"Convert the rectangular patterns to circular ones"*
- *"Make the surfaces more curved while keeping the edge relationships"*

### **Intent-Driven Changes**
- *"Optimize this for 3D printing while maintaining the design intent"*
- *"Add manufacturing features based on the original modeling approach"*
- *"Create variations that follow the same design philosophy"*

---

## 🛠 Technical Improvements

### **New Architecture**
- **CommandDependencyManager**: Analyzes modeling workflows and relationships
- **TransformationInfo**: Detailed capture of geometric transformations
- **Enhanced Memory Model**: Rich data structure with semantic understanding
- **Improved API Integration**: Context-aware prompts for better AI generation

### **Performance & Reliability**
- Optimized background capture with minimal performance impact
- Enhanced error handling for complex modeling sequences
- Improved memory serialization for rich dependency data
- Better compatibility with .NET Framework 4.8

---

## 🔧 System Requirements

- **Rhino 8** (Windows)
- **Internet connection** for AI generation
- **.NET Framework 4.8** (included with Rhino 8)

---

## 🎯 Usage Example

```
1. Start Capture
2. Create a Box
3. Create a Sphere
4. Boolean Union them
5. Array the result
6. Stop Capture
7. Prompt: "make this organic but keep the array pattern"
   → AI understands the boolean relationship and array dependency
```

The plugin now knows that:
- The sphere and box were combined via boolean union
- The union result was arrayed
- The transformation should preserve these relationships
- "Organic" should apply to the base geometry, not break the array

---

## 🐛 Bug Fixes

- Fixed .NET Framework 4.8 compatibility issues with LINQ methods
- Improved error handling for command capture edge cases
- Enhanced memory serialization for complex dependency data
- Better handling of transformation operations with multiple objects

---

## 🆓 Free Beta

Full AI capabilities are provided **free during the beta period**. We're actively gathering feedback to improve the plugin's intelligence.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/contextform/DesignMemory_RhinoPlugin/issues)
- **Discussions**: [GitHub Discussions](https://github.com/contextform/DesignMemory_RhinoPlugin/discussions)
- **Website**: [contextform.dev](https://contextform.dev)

---

## 🙏 Feedback Welcome

This major intelligence upgrade represents a new paradigm in CAD automation. We'd love to hear how the enhanced semantic understanding and workflow intelligence work for your projects!

**Try it out and let us know what you think!** 🎉