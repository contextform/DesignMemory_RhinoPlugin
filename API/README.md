# Contextform API

Backend API service for the Contextform AI Design Memory Plugin for Rhino.

## Setup

### 1. Install Dependencies
```bash
cd api
npm install
```

### 2. Configure Environment
```bash
cp .env.example .env
```

Edit `.env` and add your Claude API key:
```
CLAUDE_API_KEY=your_actual_claude_api_key_here
```

### 3. Get Claude API Key
1. Go to [console.anthropic.com](https://console.anthropic.com/)
2. Sign up/Login
3. Navigate to API Keys
4. Create a new API key
5. Copy the key to your `.env` file

### 4. Run the Server

**Development:**
```bash
npm run dev
```

**Production:**
```bash
npm start
```

## API Endpoints

### Health Check
```
GET /health
```

### Generate Script
```
POST /api/generate
```

**Request Body:**
```json
{
  "memory": {
    "session_id": "uuid",
    "commands": [
      {
        "command": "Box",
        "parameters": {...},
        "sequence": 1
      }
    ],
    "original_geometry_ids": ["guid1", "guid2"]
  },
  "prompt": "make this organic"
}
```

**Response:**
```json
{
  "script": "var doc = RhinoDoc.ActiveDoc;\n// Generated C# code...",
  "success": true,
  "timestamp": "2025-07-31T10:30:00Z"
}
```

## Deployment

### Vercel (Recommended)
1. Install Vercel CLI: `npm i -g vercel`
2. Run: `vercel`
3. Add environment variables in Vercel dashboard
4. Update plugin URL to your Vercel URL

### Other Options
- Railway
- Render
- DigitalOcean App Platform
- AWS Lambda

## Rate Limiting

- Default: 100 requests per 15 minutes per IP
- Configurable via environment variables

## Error Handling

The API includes comprehensive error handling for:
- Invalid input validation
- Claude API errors
- Rate limiting
- Authentication issues

## Security

- CORS protection
- Helmet security headers
- Request size limits
- Rate limiting
- Input validation