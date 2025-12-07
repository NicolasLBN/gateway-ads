import React from 'react';
import { Card, Text, Group, Badge, Progress } from '@mantine/core';
import { IconTemperature, IconGauge, IconEngine } from '@tabler/icons-react';
import { useStore } from '../hooks/useStore';

function MachineStatus() {
  const { machineData } = useStore();


  return (
    <Card shadow="sm" padding="lg" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="xs">
        <Text fw={500}>Machine Status</Text>
      </Card.Section>

      <div style={{ marginTop: '1rem' }}>
        <Group justify="apart" mb="xs">
          <Group gap="xs">
            <IconTemperature size={20} />
            <Text size="sm">Motor Temperature</Text>
          </Group>
          <Badge color={machineData.tempWarning ? 'red' : 'green'}>
            {machineData.motorTemperature?.toFixed(1) || 0}°C
          </Badge>
        </Group>
        <Progress
          value={(machineData.motorTemperature / 100) * 100}
          color={machineData.tempWarning ? 'red' : 'blue'}
          mb="md"
        />

        <Group justify="apart" mb="xs">
          <Group gap="xs">
            <IconGauge size={20} />
            <Text size="sm">Oil Pressure</Text>
          </Group>
          <Badge color={machineData.pressureWarning ? 'red' : 'green'}>
            {machineData.oilPressure?.toFixed(1) || 0} bar
          </Badge>
        </Group>
        <Progress
          value={(machineData.oilPressure / 10) * 100}
          color={machineData.pressureWarning ? 'red' : 'blue'}
          mb="md"
        />

        <Group justify="apart" mb="xs">
          <Group gap="xs">
            <IconEngine size={20} />
            <Text size="sm">Motor Speed</Text>
          </Group>
          <Badge color={machineData.speedWarning ? 'red' : 'green'}>
            {Math.round(machineData.motorSpeed) || 0} RPM
          </Badge>
        </Group>
        <Progress
          value={(machineData.motorSpeed / 2500) * 100}
          color={machineData.speedWarning ? 'red' : 'blue'}
        />
      </div>
    </Card>
  );
}

export default MachineStatus;
