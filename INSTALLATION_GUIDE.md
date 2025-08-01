# 📦 Contextform Installation Guide - All Methods

## 🎯 Method 1: YAK Package (Recommended)

**Best for:** Most users, automatic updates, easy uninstall

### Download
- **GitHub Release**: [contextform-1.1.0-rh8_21-any.yak](https://github.com/contextform/DesignMemory_RhinoPlugin/releases/download/v1.1.0/contextform-1.1.0-rh8_21-any.yak)
- **File Size**: ~500KB
- **Rhino Version**: Rhino 8 only

### Installation Steps
1. **Download** the `.yak` file from the link above
2. **Open Rhino 8**
3. **Type command**: `PackageManager` (or `_PackageManager`)
4. **Click "Install from File"** in the Package Manager window
5. **Select** the downloaded `.yak` file
6. **Click "Install"** - Rhino will install the plugin and dependencies
7. **Restart Rhino**
8. **Verify**: Go to **Panels** → **Contextform** to open the plugin

### Advantages
- ✅ Automatic dependency handling (Newtonsoft.Json.dll)
- ✅ Easy uninstall via Package Manager
- ✅ Future updates can be installed the same way
- ✅ Proper plugin registration

---

## 🔧 Method 2: RHP Plugin File

**Best for:** Advanced users, corporate environments, custom installations

### Download
- **GitHub Release**: [Contextform.rhp](https://github.com/contextform/DesignMemory_RhinoPlugin/releases/download/v1.1.0/Contextform.rhp)
- **Dependency**: [Newtonsoft.Json.dll](https://github.com/contextform/DesignMemory_RhinoPlugin/releases/download/v1.1.0/Newtonsoft.Json.dll)
- **File Size**: ~200KB + ~700KB (dependency)

### Installation Steps
1. **Download both files**:
   - `Contextform.rhp` (main plugin)
   - `Newtonsoft.Json.dll` (required dependency)

2. **Option A: Automatic Installation**
   - **Double-click** the `Contextform.rhp` file
   - Rhino will prompt to install the plugin
   - Click **"Install"** and **"Load"**

3. **Option B: Manual Installation**
   - **Open Rhino 8**
   - **Go to**: Tools → Options → Plug-ins
   - **Click "Install"** button
   - **Select** the `Contextform.rhp` file
   - **Click "Open"**

4. **Handle Dependency**
   - If you get a "missing assembly" error, copy `Newtonsoft.Json.dll` to:
   - **Location**: `%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\Contextform\`
   - **Create the folder** if it doesn't exist

5. **Restart Rhino**
6. **Verify**: Check Panels → Contextform

### Advantages
- ✅ Works in restricted environments
- ✅ Direct control over file placement
- ✅ Can be scripted for deployment

---

## 🏢 Method 3: Direct File Distribution

**Best for:** Network deployment, shared installations, development

### Download Files
You can get all files from the GitHub repository:
- **Main Plugin**: `Contextform.dll`
- **Dependency**: `Newtonsoft.Json.dll`
- **Manifest**: `manifest.yml` (for packaging)

### Manual Installation Steps
1. **Create plugin directory**:
   ```
   %APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\Contextform\
   ```

2. **Copy files** to the directory:
   - `Contextform.dll`
   - `Newtonsoft.Json.dll`

3. **Register plugin** in Rhino:
   - Tools → Options → Plug-ins → Install
   - Browse to the `Contextform.dll` file
   - Install and load

4. **Restart Rhino**

### Network Deployment
For IT administrators deploying across multiple machines:

```batch
@echo off
set PLUGIN_DIR=%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\Contextform
mkdir "%PLUGIN_DIR%" 2>nul
copy "Contextform.dll" "%PLUGIN_DIR%\"
copy "Newtonsoft.Json.dll" "%PLUGIN_DIR%\"
echo Contextform plugin installed to %PLUGIN_DIR%
```

---

## 🔍 Verification & Troubleshooting

### ✅ Successful Installation Check
After installation, you should see:
1. **Panels Menu**: Contextform appears in Windows → Panels
2. **Plugin List**: Listed in Tools → Options → Plug-ins
3. **Commands**: `Contextform` command works in command line

### ❌ Common Issues & Solutions

#### "Plugin not found in Panels menu"
- **Restart Rhino** - plugins require restart to appear
- **Check plug-in list**: Tools → Options → Plug-ins
- **Look for "Contextform"** - should be listed and enabled

#### "Could not load assembly Newtonsoft.Json"
- **Download dependency**: Get `Newtonsoft.Json.dll` from releases
- **Copy to plugin folder**: Same folder as `Contextform.dll`
- **Restart Rhino** after copying

#### "Plugin loads but panel is empty"
- **Internet connection required** for AI features
- **Check firewall settings** - allow Rhino internet access
- **Try restart** - some UI elements load on second startup

#### "Commands not being captured"
- **Click "Start Capture"** button in panel
- **Use supported commands** - see list in documentation
- **Check console** for error messages (F2 in Rhino)

#### Installation on Corporate/Restricted Networks
- **Download both files** to local machine first
- **Use RHP installation method** (more control)
- **Contact IT** if blocked by security policies
- **Whitelist domains**: `*.anthropic.com`, `*.render.com`

---

## 🚀 First Use Guide

Once installed:

1. **Open Panel**: Windows → Panels → Contextform
2. **Start Capture**: Click "Start Capture" button  
3. **Model Something**: Create a Box, Sphere, etc.
4. **Stop Capture**: Click "Stop Capture"
5. **Transform**: Enter prompt like "make this organic"
6. **Generate**: Click "Generate with AI"
7. **See Results**: Original geometry replaced with AI version

---

## 🔄 Updates & Uninstall

### YAK Package Updates
- New versions can be installed over existing installation
- Use Package Manager → Install from File with new `.yak`

### Manual Updates
- Replace `.dll` files with newer versions
- Restart Rhino after file replacement

### Uninstalling
- **YAK**: Package Manager → Installed → Contextform → Uninstall
- **Manual**: Delete plugin files and restart Rhino

---

## 💬 Support

- **GitHub Issues**: [Report problems](https://github.com/contextform/DesignMemory_RhinoPlugin/issues)
- **Documentation**: [Full README](https://github.com/contextform/DesignMemory_RhinoPlugin)
- **Discussions**: [Community help](https://github.com/contextform/DesignMemory_RhinoPlugin/discussions)

**Installation successful? Try transforming your first design with AI!** 🎉