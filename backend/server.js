const express = require('express');
const cors = require('cors');
const http = require('http');
const WebSocket = require('ws');
const path = require('path');

// Import routes
const machinesRouter = require('./routes/machines');
const recipeRouter = require('./routes/recipe');
const historyRouter = require('./routes/history');
const statusRouter = require('./routes/status');

// Import WebSocket handler
const setupWebSocket = require('./ws/websocket');

const app = express();
const PORT = process.env.PORT || 3001;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Static files for PDF reports
app.use('/reports', express.static(path.join(__dirname, 'reports')));

// Routes
app.use('/api/machines', machinesRouter);
app.use('/api/recipe', recipeRouter);
app.use('/api/history', historyRouter);
app.use('/api/status', statusRouter);

// Health check
app.get('/api/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

// Create HTTP server
const server = http.createServer(app);

// Setup WebSocket
setupWebSocket(server);

server.listen(PORT, () => {
  console.log(`🚀 Server running on port ${PORT}`);
  console.log(`📡 WebSocket available at ws://localhost:${PORT}`);
});
