import React from 'react';
import { Card, Text, Progress, Badge, Group } from '@mantine/core';
import { useStore } from '../hooks/useStore';

function ProcessStatus() {
  const { processData, isConnected } = useStore();

  const getStepColor = (step) => {
    if (step === 0) return 'gray';
    if (processData.processDone) return 'green';
    if (processData.errorCode !== 0) return 'red';
    return 'blue';
  };

  return (
    <Card shadow="sm" padding="lg" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="xs">
        <Group justify="space-between">
          <Text fw={500}>Process Status</Text>
          <Badge color={getStepColor(processData.currentStep)}>
            {processData.stepName || 'Idle'}
          </Badge>
        </Group>
      </Card.Section>

      <div style={{ marginTop: '1rem' }}>
        <Group justify="apart" mb="xs">
          <Text size="sm">Overall Progress</Text>
          <Text size="sm" fw={500}>
            {(processData.progress * 100).toFixed(0)}%
          </Text>
        </Group>
        <Progress value={processData.progress * 100} mb="md" />

        <Group justify="apart" mb="xs">
          <Text size="sm">Current Step Progress</Text>
          <Text size="sm" fw={500}>
            {(processData.stepProgress * 100).toFixed(0)}%
          </Text>
        </Group>
        <Progress value={processData.stepProgress * 100} color="teal" mb="md" />

        <Group justify="apart">
          <Text size="sm" c="dimmed">
            Step Time:
          </Text>
          <Text size="sm">{processData.stepTime_s || 0}s</Text>
        </Group>

        <Group justify="apart">
          <Text size="sm" c="dimmed">
            Total Time:
          </Text>
          <Text size="sm">{processData.totalTime_s || 0}s</Text>
        </Group>

        {processData.errorCode !== 0 && (
          <Badge color="red" fullWidth mt="md">
            Error: {processData.errorText || `Code ${processData.errorCode}`}
          </Badge>
        )}

        {processData.processDone && (
          <Badge color="green" fullWidth mt="md">
            ✓ Process Completed
          </Badge>
        )}
      </div>
    </Card>
  );
}

export default ProcessStatus;
