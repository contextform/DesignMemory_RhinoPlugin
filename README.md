# Contextform - AI Design Memory Plugin for Rhino

## Overview

Contextform captures your modeling process as "memory" and uses AI to regenerate designs based on any criteria. Transform your designs by simply describing what you want: "make this organic", "optimize for 3D printing", "convert to sheet metal", etc.

## Features

- **Automatic Capture**: Records your modeling commands in the background
- **AI Generation**: Uses Claude AI to generate new geometry based on your design memory
- **Simple UI**: Easy-to-use panel with start/stop capture and prompt input  
- **Script Execution**: Automatically replaces original geometry with AI-generated versions

## Installation

1. Download the .yak file from Food4Rhino
2. Install using Rhino's Package Manager or drag-and-drop into Rhino
3. Restart Rhino
4. Find the Contextform panel in Panels menu

## Usage

1. **Start Capture**: Click "Start Capture" and begin modeling
2. **Create Geometry**: Use any Rhino commands (Box, Sphere, Move, etc.)
3. **Stop Capture**: Click "Stop Capture" when finished
4. **AI Transform**: Enter a prompt like "make this organic" and click "Generate with AI"
5. **View Result**: Your original geometry will be replaced with the AI-generated version

## Example Prompts

- "make this organic"
- "optimize for 3D printing"
- "add ventilation patterns"
- "make it parametric"
- "apply art deco style"

## Requirements

- Rhino 8
- Internet connection for AI generation

## Beta Version

This is a free beta version. Full AI capabilities are provided at no cost during the beta period.

## 🐛 Support

- **Issues**: [GitHub Issues](https://github.com/contextform/rhino-plugin/issues)
- **Website**: [contextform.dev](https://contextform.dev)
- **Food4Rhino**: [Plugin Page](https://www.food4rhino.com/)

## 🙏 Acknowledgments

- Built with [RhinoCommon SDK](https://developer.rhino3d.com/)
- UI powered by [Eto.Forms](https://github.com/picoe/Eto)
- AI generation by [Anthropic Claude](https://www.anthropic.com/)

---

<div align="center">

**[Website](https://contextform.dev) • [Food4Rhino](https://www.food4rhino.com/) • [GitHub](https://github.com/contextform/rhino-plugin)**

Made with ❤️ by the Contextform Team

</div>