import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Button,
  Group,
  Card,
  TextInput,
  NumberInput,
  Stack,
  Text,
  Alert,
  Divider,
  Code,
} from '@mantine/core';
import { IconArrowLeft, IconAlertCircle, IconDeviceFloppy, IconSettings } from '@tabler/icons-react';
import { useStore } from '../hooks/useStore';

function DeveloperPage() {
  const navigate = useNavigate();
  const { machines, setMachines } = useStore();
  
  const [amsNetId, setAmsNetId] = useState('');
  const [amsPort, setAmsPort] = useState(851);
  const [machineName, setMachineName] = useState('');
  const [machineLocation, setMachineLocation] = useState('');
  const [success, setSuccess] = useState(null);
  const [error, setError] = useState(null);

  const handleAddMachine = () => {
    if (!amsNetId || !machineName) {
      setError('Please provide AMS Net ID and Machine Name');
      return;
    }

    // Validate AMS Net ID format (e.g., 127.0.0.1.1.1)
    const amsPattern = /^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$/;
    if (!amsPattern.test(amsNetId)) {
      setError('Invalid AMS Net ID format. Expected format: xxx.xxx.xxx.xxx.x.x');
      return;
    }

    // Validate that IP octets are between 0-255
    const parts = amsNetId.split('.');
    for (let i = 0; i < 4; i++) {
      const octet = parseInt(parts[i], 10);
      if (octet < 0 || octet > 255) {
        setError('Invalid AMS Net ID: IP octets must be between 0 and 255');
        return;
      }
    }

    const newMachine = {
      id: `machine-${Date.now()}`,
      name: machineName,
      amsNetId: amsNetId,
      amsPort: amsPort,
      location: machineLocation || 'Custom Location',
    };

    setMachines([...machines, newMachine]);
    setSuccess('Machine added successfully!');
    setError(null);
    
    // Clear form
    setAmsNetId('');
    setAmsPort(851);
    setMachineName('');
    setMachineLocation('');

    setTimeout(() => {
      setSuccess(null);
    }, 3000);
  };

  const handleRemoveMachine = (machineId) => {
    setMachines(machines.filter(m => m.id !== machineId));
    setSuccess('Machine removed successfully!');
    setTimeout(() => {
      setSuccess(null);
    }, 3000);
  };

  return (
    <Container size="lg">
      <Group justify="space-between" mb="xl">
        <Group gap="xs">
          <IconSettings size={32} />
          <Title order={1}>Developer Settings</Title>
        </Group>
        <Button
          variant="subtle"
          leftSection={<IconArrowLeft size={16} />}
          onClick={() => navigate('/')}
          styles={{
            root: {
              backgroundColor: '#61db34',
              color: 'white',
              '&:hover': {
                backgroundColor: '#4fb828',
              },
            },
          }}
        >
          Back to Home
        </Button>
      </Group>

      <Alert color="yellow" icon={<IconAlertCircle size={16} />} mb="xl" title="Developer Mode">
        This page is intended for developers and system administrators. Changes here affect PLC connection settings.
      </Alert>

      {error && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" mb="md" onClose={() => setError(null)} withCloseButton>
          {error}
        </Alert>
      )}

      {success && (
        <Alert color="green" mb="md" onClose={() => setSuccess(null)} withCloseButton>
          {success}
        </Alert>
      )}

      <Stack gap="xl">
        {/* Add New Machine Configuration */}
        <Card shadow="md" padding="xl" radius="md" withBorder>
          <Card.Section withBorder inheritPadding py="md">
            <Text fw={700} size="xl">Add New Machine</Text>
          </Card.Section>

          <Stack gap="lg" mt="lg">
            <TextInput
              label="Machine Name"
              placeholder="e.g., Mixing Unit D"
              value={machineName}
              onChange={(e) => setMachineName(e.target.value)}
              size="lg"
              required
              styles={{
                input: {
                  fontSize: '18px',
                  padding: '14px',
                },
                label: {
                  fontSize: '16px',
                  fontWeight: 600,
                  marginBottom: '8px',
                },
              }}
            />

            <TextInput
              label="Machine Location"
              placeholder="e.g., Production Line 3"
              value={machineLocation}
              onChange={(e) => setMachineLocation(e.target.value)}
              size="lg"
              styles={{
                input: {
                  fontSize: '18px',
                  padding: '14px',
                },
                label: {
                  fontSize: '16px',
                  fontWeight: 600,
                  marginBottom: '8px',
                },
              }}
            />

            <TextInput
              label="AMS Net ID"
              placeholder="e.g., 127.0.0.1.1.1"
              value={amsNetId}
              onChange={(e) => setAmsNetId(e.target.value)}
              size="lg"
              required
              description="Format: xxx.xxx.xxx.xxx.x.x"
              styles={{
                input: {
                  fontSize: '18px',
                  padding: '14px',
                  fontFamily: 'monospace',
                },
                label: {
                  fontSize: '16px',
                  fontWeight: 600,
                  marginBottom: '8px',
                },
              }}
            />

            <NumberInput
              label="AMS Port"
              placeholder="e.g., 851"
              value={amsPort}
              onChange={setAmsPort}
              min={800}
              max={900}
              size="lg"
              styles={{
                input: {
                  fontSize: '18px',
                  padding: '14px',
                },
                label: {
                  fontSize: '16px',
                  fontWeight: 600,
                  marginBottom: '8px',
                },
              }}
            />

            <Button
              leftSection={<IconDeviceFloppy size={20} />}
              onClick={handleAddMachine}
              size="lg"
              fullWidth
            >
              Add Machine
            </Button>
          </Stack>
        </Card>

        {/* Current Machines List */}
        <Card shadow="md" padding="xl" radius="md" withBorder>
          <Card.Section withBorder inheritPadding py="md">
            <Text fw={700} size="xl">Configured Machines</Text>
          </Card.Section>

          <Stack gap="md" mt="lg">
            {machines.length === 0 ? (
              <Text c="dimmed" ta="center">No machines configured</Text>
            ) : (
              machines.map((machine) => (
                <Card key={machine.id} withBorder padding="lg" bg="#f8f9fa">
                  <Group justify="space-between">
                    <div style={{ flex: 1 }}>
                      <Text fw={700} size="lg" mb="xs">
                        {machine.name}
                      </Text>
                      <Stack gap="xs">
                        <Text size="sm" c="dimmed">
                          📍 Location: {machine.location}
                        </Text>
                        <Text size="sm" c="dimmed">
                          🔌 AMS Net ID: <Code>{machine.amsNetId}</Code>
                        </Text>
                        <Text size="sm" c="dimmed">
                          🔧 AMS Port: <Code>{machine.amsPort}</Code>
                        </Text>
                      </Stack>
                    </div>
                    <Button
                      color="red"
                      variant="light"
                      size="md"
                      onClick={() => handleRemoveMachine(machine.id)}
                    >
                      Remove
                    </Button>
                  </Group>
                </Card>
              ))
            )}
          </Stack>
        </Card>

        {/* Information Section */}
        <Card shadow="md" padding="xl" radius="md" withBorder>
          <Card.Section withBorder inheritPadding py="md">
            <Text fw={700} size="xl">About AMS Net ID</Text>
          </Card.Section>

          <Stack gap="md" mt="lg">
            <Text size="md">
              The AMS (Automation Device Specification) Net ID is a unique identifier for TwinCAT devices in a network.
            </Text>
            <Divider />
            <div>
              <Text fw={600} mb="xs">Format:</Text>
              <Code block>xxx.xxx.xxx.xxx.x.x</Code>
              <Text size="sm" c="dimmed" mt="xs">
                Example: 127.0.0.1.1.1 (for local PLC)
              </Text>
            </div>
            <div>
              <Text fw={600} mb="xs">Common AMS Ports:</Text>
              <Text size="sm" c="dimmed">• 851 - Default TwinCAT Runtime Port 1</Text>
              <Text size="sm" c="dimmed">• 852 - Default TwinCAT Runtime Port 2</Text>
              <Text size="sm" c="dimmed">• 853 - Default TwinCAT Runtime Port 3</Text>
            </div>
          </Stack>
        </Card>
      </Stack>
    </Container>
  );
}

export default DeveloperPage;
