import React, { useEffect, useState } from 'react';
import { Select, Card, Text, Badge, Group, Button } from '@mantine/core';
import { IconCircleCheck, IconCircleX } from '@tabler/icons-react';
import { api } from '../services/api';
import { useStore } from '../hooks/useStore';

function MachineSelector() {
  const { machines, setMachines, selectedMachine, setSelectedMachine, isConnected } = useStore();
  const [loading, setLoading] = useState(false);
  const [connecting, setConnecting] = useState(false);

  useEffect(() => {
    loadMachines();
    // Only run on mount - loadMachines doesn't need to be a dependency as it's stable
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadMachines = async () => {
    setLoading(true);
    try {
      const result = await api.getMachines();
      if (result.success) {
        setMachines(result.machines);
        if (result.machines.length > 0 && !selectedMachine) {
          setSelectedMachine(result.machines[0]);
        }
      }
    } catch (error) {
      console.error('Error loading machines:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleConnect = async () => {
    if (!selectedMachine) return;

    setConnecting(true);
    try {
      const result = await api.connectToPLC(
        selectedMachine.amsNetId,
        selectedMachine.amsPort
      );
      if (result.success) {
        console.log('Connected to PLC');
      } else {
        console.error('Connection failed:', result.error);
      }
    } catch (error) {
      console.error('Error connecting:', error);
    } finally {
      setConnecting(false);
    }
  };

  const handleDisconnect = async () => {
    setConnecting(true);
    try {
      await api.disconnectFromPLC();
    } catch (error) {
      console.error('Error disconnecting:', error);
    } finally {
      setConnecting(false);
    }
  };

  return (
    <Card shadow="md" padding="xl" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="md">
        <Group justify="space-between">
          <Text fw={600} size="lg">Machine Selection</Text>
          <Badge
            size="lg"
            color={isConnected ? 'green' : 'red'}
            leftSection={
              isConnected ? <IconCircleCheck size={14} /> : <IconCircleX size={14} />
            }
          >
            {isConnected ? 'Connected' : 'Disconnected'}
          </Badge>
        </Group>
      </Card.Section>

      <Select
        label="Select Machine"
        placeholder="Choose a machine"
        data={machines.map((m) => ({ value: m.id, label: m.name }))}
        value={selectedMachine?.id}
        onChange={(value) => {
          const machine = machines.find((m) => m.id === value);
          setSelectedMachine(machine);
        }}
        disabled={loading}
        mt="lg"
        size="lg"
        radius="md"
        styles={{
          input: {
            padding: '12px 16px',
            fontSize: '16px',
          },
          dropdown: {
            padding: '8px',
          },
          option: {
            padding: '12px 16px',
            fontSize: '16px',
            borderRadius: '8px',
            marginBottom: '4px',
          },
        }}
      />

      {selectedMachine && (
        <Card withBorder mt="lg" padding="md" radius="md" bg="#f8f9fa">
          <Text size="sm" fw={500} mb="xs">Machine Details:</Text>
          <Text size="sm" c="dimmed" mb="xs">
            📍 Location: {selectedMachine.location}
          </Text>
          <Text size="sm" c="dimmed" mb="xs">
            🔌 AMS Net ID: {selectedMachine.amsNetId}
          </Text>
          <Text size="sm" c="dimmed">
            🔧 AMS Port: {selectedMachine.amsPort}
          </Text>
        </Card>
      )}

      <Group mt="xl">
        {!isConnected ? (
          <Button
            fullWidth
            onClick={handleConnect}
            loading={connecting}
            disabled={!selectedMachine}
            size="lg"
          >
            Connect to Machine
          </Button>
        ) : (
          <Button
            fullWidth
            color="red"
            onClick={handleDisconnect}
            loading={connecting}
            size="lg"
          >
            Disconnect from Machine
          </Button>
        )}
      </Group>
    </Card>
  );
}

export default MachineSelector;
