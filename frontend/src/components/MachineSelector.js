import React, { useEffect, useState } from 'react';
import { Card, Text, Badge, Group, Button, SimpleGrid, Stack } from '@mantine/core';
import { IconCircleCheck, IconCircleX, IconServer, IconMapPin, IconNetwork } from '@tabler/icons-react';
import { api } from '../services/api';
import { useStore } from '../hooks/useStore';

function MachineSelector() {
  const { machines, setMachines, selectedMachine, setSelectedMachine, isConnected } = useStore();
  const [connecting, setConnecting] = useState(false);

  useEffect(() => {
    loadMachines();
    // Only run on mount - loadMachines doesn't need to be a dependency as it's stable
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadMachines = async () => {
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
      <Card.Section withBorder inheritPadding py="lg">
        <Group justify="space-between">
          <Text fw={600} size="xl">Machine Selection</Text>
          <Badge
            size="xl"
            color={isConnected ? 'green' : 'red'}
            leftSection={
              isConnected ? <IconCircleCheck size={16} /> : <IconCircleX size={16} />
            }
          >
            {isConnected ? 'Connected' : 'Disconnected'}
          </Badge>
        </Group>
      </Card.Section>

      <Stack gap="xl" mt="xl">
        <div>
          <Text size="lg" fw={600} mb="md">
            Select a Machine:
          </Text>
          <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="lg">
            {machines.map((machine) => (
              <Card
                key={machine.id}
                shadow="sm"
                padding="lg"
                radius="md"
                withBorder
                style={{
                  cursor: 'pointer',
                  border: selectedMachine?.id === machine.id ? '3px solid #228be6' : '2px solid #e9ecef',
                  backgroundColor: selectedMachine?.id === machine.id ? '#e7f5ff' : '#ffffff',
                  transition: 'all 0.2s ease',
                }}
                onClick={() => setSelectedMachine(machine)}
              >
                <Stack gap="md">
                  <Group justify="space-between">
                    <Group gap="xs">
                      <IconServer size={24} color={selectedMachine?.id === machine.id ? '#228be6' : '#495057'} />
                      <Text fw={700} size="lg">
                        {machine.name}
                      </Text>
                    </Group>
                    {selectedMachine?.id === machine.id && (
                      <IconCircleCheck size={24} color="#228be6" />
                    )}
                  </Group>
                  
                  <Stack gap="xs">
                    <Group gap="xs">
                      <IconMapPin size={18} />
                      <Text size="sm" c="dimmed">
                        {machine.location}
                      </Text>
                    </Group>
                    <Group gap="xs">
                      <IconNetwork size={18} />
                      <Text size="sm" c="dimmed">
                        {machine.amsNetId}:{machine.amsPort}
                      </Text>
                    </Group>
                  </Stack>
                </Stack>
              </Card>
            ))}
          </SimpleGrid>
        </div>

        {selectedMachine && (
          <Card withBorder padding="lg" radius="md" bg="#f8f9fa">
            <Text size="md" fw={600} mb="md">Selected Machine Details:</Text>
            <Stack gap="sm">
              <Group gap="xs">
                <IconServer size={20} />
                <Text size="md" fw={500}>Name:</Text>
                <Text size="md">{selectedMachine.name}</Text>
              </Group>
              <Group gap="xs">
                <IconMapPin size={20} />
                <Text size="md" fw={500}>Location:</Text>
                <Text size="md">{selectedMachine.location}</Text>
              </Group>
              <Group gap="xs">
                <IconNetwork size={20} />
                <Text size="md" fw={500}>AMS Net ID:</Text>
                <Text size="md">{selectedMachine.amsNetId}</Text>
              </Group>
              <Group gap="xs">
                <IconNetwork size={20} />
                <Text size="md" fw={500}>AMS Port:</Text>
                <Text size="md">{selectedMachine.amsPort}</Text>
              </Group>
            </Stack>
          </Card>
        )}

        <Group grow>
          {!isConnected ? (
            <Button
              onClick={handleConnect}
              loading={connecting}
              disabled={!selectedMachine}
              size="xl"
              styles={{
                root: {
                  fontSize: '18px',
                  padding: '16px 24px',
                },
              }}
            >
              Connect to Machine
            </Button>
          ) : (
            <Button
              color="red"
              onClick={handleDisconnect}
              loading={connecting}
              size="xl"
              styles={{
                root: {
                  fontSize: '18px',
                  padding: '16px 24px',
                },
              }}
            >
              Disconnect from Machine
            </Button>
          )}
        </Group>
      </Stack>
    </Card>
  );
}

export default MachineSelector;
