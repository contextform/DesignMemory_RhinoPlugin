# Changelog

All notable changes to the Contextform plugin will be documented in this file.

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