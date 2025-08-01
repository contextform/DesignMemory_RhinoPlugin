# Changelog

All notable changes to the Contextform plugin will be documented in this file.

## [1.1.0] - 2025-08-01

### 🚀 Major Enhancements
- **Comprehensive Command Support**: Expanded from 25 to 50+ supported Rhino commands
- **Intelligent Memory Capture**: Added semantic data extraction with dimensions, positions, and design intent
- **Command Relationship Tracking**: Implemented workflow analysis with dependencies and design patterns
- **Geometric Transformation Tracking**: Precise capture of movement, scaling, and rotation data
- **Command Dependency System**: Advanced analysis of modeling workflows and command relationships

### 🧠 Semantic Understanding
- **Rich Semantic Data**: Captures creation methods, dimensions, centers, corner points for all geometry
- **Command Categories**: Automatic classification (primitive, curve, surface, transformation, boolean, editing)
- **Workflow Stages**: Distinguishes creation, modification, finishing, and analysis operations
- **Design Intent Inference**: AI-powered understanding of modeling purpose and context

### 🔗 Workflow Intelligence
- **Dependency Analysis**: Tracks which commands depend on others
- **Pattern Recognition**: Identifies creation sequences, modification chains, boolean operations
- **Transformation Data**: Records precise movement vectors, scale factors, rotation angles
- **Enhanced AI Prompts**: Leverages rich memory data for context-aware script generation

### 📋 Expanded Command Coverage
- **Primitives**: Box, Sphere, Cylinder, Cone, Plane, Torus
- **Curves**: Line, Circle, Rectangle, Arc, Polyline, Ellipse, Polygon, InterpCrv, CurveThroughPt
- **Surfaces**: Extrude, ExtrudeCrv, ExtrudeSrf, Loft, Revolve, Sweep1, Sweep2, NetworkSrf, Patch, PlanarSrf
- **Transformations**: Move, Copy, Rotate, Scale, Mirror, Array, ArrayPolar, ArrayLinear, ArrayCrv, Orient, Flow
- **Boolean Operations**: BooleanUnion, BooleanDifference, BooleanIntersection, BooleanSplit
- **Editing**: Fillet, Chamfer, Trim, Split, Join, Explode, Offset, OffsetSrf, Rebuild, Simplify, Fair, MatchSrf
- **Organization**: Delete, Group, Ungroup, Hide, Show, Lock, Unlock
- **Analysis**: Distance, Angle, Area, Volume, CurvatureAnalysis

### 🛠 Technical Improvements
- **CommandDependencyManager**: New system for workflow analysis and relationship tracking
- **TransformationInfo**: Detailed capture of geometric transformations
- **Enhanced Memory Model**: Rich data structure with relationships and dependency analysis
- **Improved API Integration**: Context-aware prompts using semantic data and workflow patterns

### 🐛 Bug Fixes
- Fixed .NET Framework 4.8 compatibility issues with LINQ methods
- Improved error handling for command capture edge cases
- Enhanced memory serialization for complex dependency data

## [1.0.0] - 2025-07-31

### Added
- Initial release of Contextform AI Design Memory Plugin
- Automatic command capture system for Rhino modeling operations
- AI-powered script generation using Claude API
- Memory-based design transformation capabilities
- Modern Eto.Forms UI panel with capture controls
- Support for 25+ Rhino commands (Box, Sphere, Move, Rotate, Boolean operations, etc.)
- Local JSON storage for design memory
- Script execution with geometry replacement
- Settings management for API configuration
- Free API endpoint support for beta users

### Features
- **Smart Capture**: Records modeling commands, parameters, and geometry relationships
- **AI Generation**: Transform designs with natural language prompts
- **Seamless Integration**: Native Rhino 8 plugin with dockable panel
- **Geometry Replacement**: Original objects replaced with AI-generated versions
- **Command Support**: Comprehensive coverage of essential Rhino commands

### Technical
- Built for Rhino 8 (.NET Framework 4.8)
- Uses RhinoCommon API for native integration
- Eto.Forms for cross-platform UI compatibility
- Newtonsoft.Json for data serialization
- Proper plugin architecture with GUID registration