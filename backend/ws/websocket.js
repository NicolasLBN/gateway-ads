const WebSocket = require('ws');
const adsClient = require('../ads/adsClient');

function setupWebSocket(server) {
  const wss = new WebSocket.Server({ server, path: '/ws/status' });

  console.log('📡 WebSocket server initialized');

  wss.on('connection', (ws) => {
    console.log('🔌 New WebSocket client connected');

    let updateInterval = null;

    // Send initial connection message
    ws.send(JSON.stringify({
      type: 'connected',
      message: 'Connected to WebSocket server',
      timestamp: new Date().toISOString(),
    }));

    // Start sending real-time updates
    const startUpdates = () => {
      updateInterval = setInterval(async () => {
        try {
          const connectionStatus = adsClient.getConnectionStatus();

          if (!connectionStatus.connected) {
            ws.send(JSON.stringify({
              type: 'status',
              connected: false,
              timestamp: new Date().toISOString(),
            }));
            return;
          }

          // Read data from PLC
          const machineData = await adsClient.readMachineData();
          const processData = await adsClient.readProcessData();

          ws.send(JSON.stringify({
            type: 'status',
            connected: true,
            machine: machineData,
            process: processData,
            timestamp: new Date().toISOString(),
          }));
        } catch (error) {
          console.error('WebSocket update error:', error.message);
          ws.send(JSON.stringify({
            type: 'error',
            error: error.message,
            timestamp: new Date().toISOString(),
          }));
        }
      }, 1000); // Update every second
    };

    // Handle incoming messages
    ws.on('message', (message) => {
      try {
        const data = JSON.parse(message);
        
        if (data.command === 'start_updates') {
          startUpdates();
          ws.send(JSON.stringify({
            type: 'ack',
            message: 'Started real-time updates',
          }));
        } else if (data.command === 'stop_updates') {
          if (updateInterval) {
            clearInterval(updateInterval);
            updateInterval = null;
          }
          ws.send(JSON.stringify({
            type: 'ack',
            message: 'Stopped real-time updates',
          }));
        }
      } catch (error) {
        console.error('Error parsing WebSocket message:', error);
      }
    });

    // Handle client disconnect
    ws.on('close', () => {
      console.log('🔌 WebSocket client disconnected');
      if (updateInterval) {
        clearInterval(updateInterval);
      }
    });

    // Handle errors
    ws.on('error', (error) => {
      console.error('WebSocket error:', error);
    });
  });

  return wss;
}

module.exports = setupWebSocket;
