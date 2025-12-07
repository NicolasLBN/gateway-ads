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
    <Card shadow="sm" padding="lg" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="xs">
        <Group justify="space-between">
          <Text fw={500}>Machine Selection</Text>
          <Badge
            color={isConnected ? 'green' : 'red'}
            leftSection={
              isConnected ? <IconCircleCheck size={12} /> : <IconCircleX size={12} />
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
        mt="md"
      />

      {selectedMachine && (
        <div style={{ marginTop: '1rem' }}>
          <Text size="sm" c="dimmed">
            Location: {selectedMachine.location}
          </Text>
          <Text size="sm" c="dimmed">
            AMS Net ID: {selectedMachine.amsNetId}
          </Text>
          <Text size="sm" c="dimmed">
            AMS Port: {selectedMachine.amsPort}
          </Text>
        </div>
      )}

      <Group mt="md">
        {!isConnected ? (
          <Button
            fullWidth
            onClick={handleConnect}
            loading={connecting}
            disabled={!selectedMachine}
          >
            Connect
          </Button>
        ) : (
          <Button
            fullWidth
            color="red"
            onClick={handleDisconnect}
            loading={connecting}
          >
            Disconnect
          </Button>
        )}
      </Group>
    </Card>
  );
}

export default MachineSelector;
