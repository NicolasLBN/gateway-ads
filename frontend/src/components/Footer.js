import React from 'react';
import { Group, Text, Badge, Paper } from '@mantine/core';
import { IconCircleCheck, IconCircleX, IconPlug } from '@tabler/icons-react';
import { useStore } from '../hooks/useStore';

function Footer() {
  const { isConnected, selectedMachine } = useStore();

  return (
    <Paper 
      shadow="sm" 
      p="md" 
      style={{ 
        position: 'fixed', 
        bottom: 0, 
        left: 0, 
        right: 0, 
        zIndex: 100,
        borderTop: '2px solid #e9ecef'
      }}
    >
      <Group justify="center" gap="lg">
        <Group gap="xs">
          <IconPlug size={18} />
          <Text size="sm" fw={500}>PLC Connection Status:</Text>
        </Group>
        <Badge
          size="lg"
          color={isConnected ? 'green' : 'red'}
          leftSection={
            isConnected ? <IconCircleCheck size={14} /> : <IconCircleX size={14} />
          }
        >
          {isConnected ? `Connected to ${selectedMachine?.name || 'Unknown'}` : 'Disconnected'}
        </Badge>
      </Group>
    </Paper>
  );
}

export default Footer;
