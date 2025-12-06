const WS_URL = process.env.REACT_APP_WS_URL || 'ws://localhost:3001/ws/status';

class WebSocketService {
  constructor() {
    this.ws = null;
    this.listeners = new Set();
    this.reconnectInterval = null;
    this.isConnecting = false;
  }

  connect() {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      return;
    }

    if (this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    try {
      this.ws = new WebSocket(WS_URL);

      this.ws.onopen = () => {
        console.log('✅ WebSocket connected');
        this.isConnecting = false;
        this.startUpdates();
        
        // Clear reconnect interval if exists
        if (this.reconnectInterval) {
          clearInterval(this.reconnectInterval);
          this.reconnectInterval = null;
        }
      };

      this.ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          this.notifyListeners(data);
        } catch (error) {
          console.error('Error parsing WebSocket message:', error);
        }
      };

      this.ws.onerror = (error) => {
        console.error('❌ WebSocket error:', error);
        this.isConnecting = false;
      };

      this.ws.onclose = () => {
        console.log('🔌 WebSocket disconnected');
        this.isConnecting = false;
        this.scheduleReconnect();
      };
    } catch (error) {
      console.error('WebSocket connection error:', error);
      this.isConnecting = false;
      this.scheduleReconnect();
    }
  }

  scheduleReconnect() {
    if (!this.reconnectInterval) {
      this.reconnectInterval = setInterval(() => {
        console.log('🔄 Attempting to reconnect...');
        this.connect();
      }, 5000);
    }
  }

  disconnect() {
    if (this.reconnectInterval) {
      clearInterval(this.reconnectInterval);
      this.reconnectInterval = null;
    }

    if (this.ws) {
      this.stopUpdates();
      this.ws.close();
      this.ws = null;
    }
  }

  send(data) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(data));
    }
  }

  startUpdates() {
    this.send({ command: 'start_updates' });
  }

  stopUpdates() {
    this.send({ command: 'stop_updates' });
  }

  subscribe(callback) {
    this.listeners.add(callback);
    return () => {
      this.listeners.delete(callback);
    };
  }

  notifyListeners(data) {
    this.listeners.forEach((callback) => {
      try {
        callback(data);
      } catch (error) {
        console.error('Error in WebSocket listener:', error);
      }
    });
  }

  getReadyState() {
    return this.ws ? this.ws.readyState : WebSocket.CLOSED;
  }
}

export const wsService = new WebSocketService();
