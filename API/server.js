const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const Anthropic = require('@anthropic-ai/sdk');
require('dotenv').config();

const app = express();
const port = process.env.PORT || 3000;

// Initialize Claude client
const anthropic = new Anthropic({
  apiKey: process.env.CLAUDE_API_KEY,
});

// Middleware
app.use(helmet());
app.use(cors({
  origin: process.env.ALLOWED_ORIGINS?.split(',') || ['http://localhost:3000'],
  credentials: true
}));
app.use(express.json({ limit: '10mb' }));

// Rate limiting
const limiter = rateLimit({
  windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS) || 15 * 60 * 1000, // 15 minutes
  max: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS) || 100, // limit each IP to 100 requests per windowMs
  message: {
    error: 'Too many requests from this IP, please try again later.',
    retryAfter: Math.ceil((parseInt(process.env.RATE_LIMIT_WINDOW_MS) || 900000) / 1000)
  }
});

app.use('/api/', limiter);

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ 
    status: 'ok', 
    timestamp: new Date().toISOString(),
    service: 'contextform-api',
    version: '1.0.0'
  });
});


// Main generation endpoint
app.post('/api/generate', async (req, res) => {
  try {
    const { memory, prompt } = req.body;

    // Debug logging
    console.log(`[${new Date().toISOString()}] Environment check:`, {
      hasClaudeKey: !!process.env.CLAUDE_API_KEY,
      keyPrefix: process.env.CLAUDE_API_KEY?.substring(0, 15) + '...',
      claudeModel: process.env.CLAUDE_MODEL,
      nodeEnv: process.env.NODE_ENV
    });

    // Validate input
    if (!memory || !prompt) {
      return res.status(400).json({
        error: 'Missing required fields: memory and prompt are required',
        success: false
      });
    }

    if (!memory.commands || memory.commands.length === 0) {
      return res.status(400).json({
        error: 'Design memory must contain at least one command',
        success: false
      });
    }

    console.log(`[${new Date().toISOString()}] Generation request:`, {
      commandCount: memory.commands.length,
      prompt: prompt.substring(0, 100) + (prompt.length > 100 ? '...' : ''),
      sessionId: memory.session_id,
      hasDependencyAnalysis: !!memory.dependency_analysis,
      workflowCount: memory.dependency_analysis?.workflows?.length || 0
    });
    
    // Log dependency analysis if available
    if (memory.dependency_analysis) {
      console.log(`[${new Date().toISOString()}] Dependency Analysis:`, {
        commandTypes: memory.dependency_analysis.command_types,
        workflowStages: memory.dependency_analysis.workflow_stages,
        workflowCount: memory.dependency_analysis.workflows?.length || 0
      });
    }

    // Generate script using Claude
    const script = await generateRhinoScript(memory, prompt);

    res.json({
      script: script,
      success: true,
      timestamp: new Date().toISOString(),
      model: process.env.CLAUDE_MODEL || 'claude-3-5-sonnet-20241022'
    });

  } catch (error) {
    console.error('Generation error:', error);
    console.error('Error stack:', error.stack);
    
    // Handle Claude API specific errors
    if (error.status === 429) {
      return res.status(429).json({
        error: 'Rate limit exceeded. Please try again later.',
        success: false,
        retryAfter: 60
      });
    }

    if (error.status === 401) {
      return res.status(500).json({
        error: 'API configuration error. Please contact support.',
        success: false,
        details: 'Invalid API key'
      });
    }

    res.status(500).json({
      error: 'Failed to generate script. Please try again.',
      success: false,
      details: process.env.NODE_ENV === 'development' ? error.message : error.message
    });
  }
});

async function generateRhinoScript(memory, prompt) {
  console.log(`[${new Date().toISOString()}] === STARTING generateRhinoScript ===`);
  console.log(`[${new Date().toISOString()}] Memory commands count:`, memory.commands?.length);
  console.log(`[${new Date().toISOString()}] Prompt length:`, prompt?.length);
  
  const systemPrompt = `You are a Rhino 3D modeling expert that generates Python scripts using RhinoScriptSyntax based on enhanced design memory and user prompts.

Your task is to:
1. Analyze the captured design memory (sequence of Rhino commands with semantic data, relationships, and dependency analysis)
2. Understand the design intent from command sequences, relationships, and workflow patterns
3. Generate a Python script using RhinoScriptSyntax that creates geometry based on the user's transformation request
4. The script should create new geometry that replaces the original design while respecting command dependencies

CRITICAL SYNTAX RULES for RhinoScriptSyntax:
- ALWAYS use proper coordinate lists: [x, y, z] format
- For rs.AddBox(): Use 8-point format: [[x1,y1,z1], [x2,y2,z2], [x3,y3,z3], [x4,y4,z4], [x5,y5,z5], [x6,y6,z6], [x7,y7,z7], [x8,y8,z8]]
- For rs.AddSphere(): Use rs.AddSphere([center_x, center_y, center_z], radius)
- For rs.AddCylinder(): Use rs.AddCylinder([base_x, base_y, base_z], [top_x, top_y, top_z], radius)
- NEVER use string literals like "corner" - always use numeric coordinates

CORRECT RHINOSCRIPTSYNTAX FUNCTIONS (DO NOT INVENT FUNCTIONS):
- Transformation: rs.MoveObject(), rs.CopyObject(), rs.RotateObject(), rs.ScaleObject()
- Creation: rs.AddRectangle(), rs.AddCircle(), rs.AddLine(), rs.AddPolyline()
- Points: rs.AddPoint(), NOT rs.RotatePoint() (this function does not exist!)
- Curves: rs.AddCurve(), rs.AddArc(), rs.AddEllipse()
- NEVER use functions like rs.RotatePoint(), rs.TransformPoint() - these do not exist!
- For point rotation, use rs.PointTransform() with transformation matrix

ENHANCED MEMORY DATA USAGE:
- Use semantic_data.dimensions for width, height, depth values
- Use semantic_data.first_corner and semantic_data.center for positioning
- Use semantic_data.corner_points when available for precise geometry
- Use relationships.workflow_stage to understand creation vs modification vs finishing operations
- Use relationships.command_category to understand geometry types (primitive, curve, surface, etc.)
- Use relationships.design_intent to understand the purpose of each command
- Use transformation_data when available for precise movement, scaling, or rotation information
- Use dependency_analysis.workflows to understand complex modeling patterns
- Apply transformations based on the semantic intent, relationships, and user request

EXAMPLES:
For a box with semantic data:
import rhinoscriptsyntax as rs
corners = [
  [0, 0, 0], [10, 0, 0], [10, 10, 0], [0, 10, 0],
  [0, 0, 5], [10, 0, 5], [10, 10, 5], [0, 10, 5]
]
rs.AddBox(corners)

For a sphere transformation:
import rhinoscriptsyntax as rs
rs.AddSphere([5, 5, 2.5], 3.0)

Important guidelines:
- Use ONLY RhinoScriptSyntax (import rhinoscriptsyntax as rs)
- Generate complete, executable Python code
- Always start with: import rhinoscriptsyntax as rs
- NEVER use variables without defining them
- Use numeric values ONLY - no string coordinates
- Focus on the geometric transformation requested by the user
- Maintain the overall design structure and command relationships while applying the requested changes
- Consider workflow patterns and dependencies when generating the script
- Respect the original design intent while implementing the requested modifications

Return only the Python script code without any markdown formatting or code blocks.`;

  const userPrompt = `Design Memory (captured modeling session):
${JSON.stringify(memory, null, 2)}

User Request: ${prompt}

Original geometry IDs that will be replaced: ${memory.original_geometry_ids?.join(', ') || 'none'}

Please generate a Python RhinoScriptSyntax script that creates new geometry based on the design memory and user request. The script should create geometry that fulfills the user's transformation request while maintaining the original design intent.`;

  console.log(`[${new Date().toISOString()}] System prompt length:`, systemPrompt.length);
  console.log(`[${new Date().toISOString()}] User prompt length:`, userPrompt.length);

  try {
    const modelName = (process.env.CLAUDE_MODEL || 'claude-3-5-sonnet-20241022').trim();
    console.log(`[${new Date().toISOString()}] Using model:`, JSON.stringify(modelName));
    console.log(`[${new Date().toISOString()}] About to call anthropic.messages.create...`);
    
    const startTime = Date.now();
    const response = await anthropic.messages.create({
      model: modelName,
      max_tokens: 4000,
      system: systemPrompt,
      messages: [
        {
          role: 'user',
          content: userPrompt
        }
      ]
    });
    const endTime = Date.now();
    
    console.log(`[${new Date().toISOString()}] Claude API call completed in ${endTime - startTime}ms`);
    console.log(`[${new Date().toISOString()}] Response received, content length:`, response.content?.[0]?.text?.length);

    const script = response.content[0].text;
    
    // Clean up the script - remove any markdown formatting
    const cleanScript = script
      .replace(/```python\n?/g, '')
      .replace(/```csharp\n?/g, '')
      .replace(/```\n?/g, '')
      .trim();

    console.log(`[${new Date().toISOString()}] Generated script length:`, cleanScript.length);
    console.log(`[${new Date().toISOString()}] === generateRhinoScript COMPLETED ===`);
    
    return cleanScript;

  } catch (error) {
    console.error(`[${new Date().toISOString()}] === CLAUDE API ERROR ===`);
    console.error(`[${new Date().toISOString()}] Error type:`, error.constructor.name);
    console.error(`[${new Date().toISOString()}] Error message:`, error.message);
    console.error(`[${new Date().toISOString()}] Error status:`, error.status);
    console.error(`[${new Date().toISOString()}] Full error:`, error);
    throw error;
  }
}

// Error handling middleware
app.use((error, req, res, next) => {
  console.error('Unhandled error:', error);
  res.status(500).json({
    error: 'Internal server error',
    success: false
  });
});

// 404 handler
app.use('*', (req, res) => {
  res.status(404).json({
    error: 'Endpoint not found',
    success: false
  });
});

app.listen(port, () => {
  console.log(`🚀 Contextform API server running on port ${port}`);
  console.log(`📊 Health check: http://localhost:${port}/health`);
  console.log(`🎯 Generate endpoint: http://localhost:${port}/api/generate`);
  console.log(`🔑 Claude model: ${process.env.CLAUDE_MODEL || 'claude-3-5-sonnet-20241022'}`);
});