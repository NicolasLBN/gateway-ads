import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Title, Button, Group, Badge, Alert } from '@mantine/core';
import { IconArrowLeft, IconCircleCheck, IconCircleX, IconAlertCircle } from '@tabler/icons-react';
import MachineSelector from '../components/MachineSelector';
import { useStore } from '../hooks/useStore';

function MachineSettingsPage() {
  const navigate = useNavigate();
  const { isConnected, selectedMachine } = useStore();

  return (
    <Container size="lg">
      <Group justify="space-between" mb="xl">
        <Title order={1}>Machine Settings</Title>
        <Button
          variant="subtle"
          leftSection={<IconArrowLeft size={16} />}
          onClick={() => navigate('/')}
        >
          Back to Dashboard
        </Button>
      </Group>

      {!isConnected && (
        <Alert 
          icon={<IconAlertCircle size={16} />} 
          color="red" 
          mb="md"
          title="Not Connected"
        >
          Not connected to machine
        </Alert>
      )}

      <Group justify="center" mb="md">
        <Badge
          size="xl"
          color={isConnected ? 'green' : 'red'}
          leftSection={
            isConnected ? <IconCircleCheck size={16} /> : <IconCircleX size={16} />
          }
        >
          {isConnected ? `Connected: ${selectedMachine?.name || 'Unknown'}` : 'Disconnected'}
        </Badge>
      </Group>

      <MachineSelector />
    </Container>
  );
}

export default MachineSettingsPage;
