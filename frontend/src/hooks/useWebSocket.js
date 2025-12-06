import { useEffect } from 'react';
import { wsService } from '../services/websocket';
import { useStore } from './useStore';

export const useWebSocket = () => {
  const {
    setIsConnected,
    setMachineData,
    setProcessData,
    addProcessHistory,
  } = useStore();

  useEffect(() => {
    // Connect to WebSocket
    wsService.connect();

    // Subscribe to messages
    const unsubscribe = wsService.subscribe((data) => {
      if (data.type === 'status') {
        setIsConnected(data.connected);

        if (data.connected && data.machine && data.process) {
          setMachineData(data.machine);
          setProcessData(data.process);

          // Add to history for charting
          addProcessHistory({
            timestamp: data.timestamp,
            temperature: data.machine.motorTemperature,
            pressure: data.machine.oilPressure,
            speed: data.machine.motorSpeed,
            progress: data.process.progress * 100,
          });
        }
      }
    });

    // Cleanup on unmount
    return () => {
      unsubscribe();
      wsService.disconnect();
    };
  }, [setIsConnected, setMachineData, setProcessData, addProcessHistory]);
};
