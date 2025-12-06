import React from 'react';
import { Card, Text } from '@mantine/core';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { useStore } from '../hooks/useStore';

function RealtimeChart() {
  const { processHistory } = useStore();

  if (processHistory.length === 0) {
    return (
      <Card shadow="sm" padding="lg" radius="md" withBorder>
        <Text c="dimmed" ta="center">
          No data yet. Start a process to see real-time data.
        </Text>
      </Card>
    );
  }

  // Format data for chart
  const chartData = processHistory.map((item, index) => ({
    index,
    temperature: item.temperature,
    pressure: item.pressure,
    speed: item.speed / 10, // Scale down for visibility
    progress: item.progress,
  }));

  return (
    <Card shadow="sm" padding="lg" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="xs">
        <Text fw={500}>Real-time Data</Text>
      </Card.Section>

      <div style={{ marginTop: '1rem', height: '300px' }}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" stroke="#444" />
            <XAxis dataKey="index" stroke="#888" />
            <YAxis stroke="#888" />
            <Tooltip
              contentStyle={{
                backgroundColor: '#1A1B1E',
                border: '1px solid #444',
              }}
            />
            <Legend />
            <Line
              type="monotone"
              dataKey="temperature"
              stroke="#ff6b6b"
              name="Temperature (°C)"
              dot={false}
            />
            <Line
              type="monotone"
              dataKey="pressure"
              stroke="#51cf66"
              name="Pressure (bar)"
              dot={false}
            />
            <Line
              type="monotone"
              dataKey="speed"
              stroke="#339af0"
              name="Speed (RPM/10)"
              dot={false}
            />
            <Line
              type="monotone"
              dataKey="progress"
              stroke="#ffd43b"
              name="Progress (%)"
              dot={false}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
}

export default RealtimeChart;
