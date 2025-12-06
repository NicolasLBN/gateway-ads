import React from 'react';
import { Card, Text, Timeline } from '@mantine/core';
import { IconCircleCheck, IconCircle, IconClock } from '@tabler/icons-react';
import { useStore } from '../hooks/useStore';

const PROCESS_STEPS = [
  { id: 0, name: 'Idle', title: 'Idle' },
  { id: 1, name: 'Preparation', title: 'Préparation' },
  { id: 2, name: 'Dosing Ingredient A', title: 'Dosage ingrédient A' },
  { id: 3, name: 'Dosing Ingredient B', title: 'Dosage ingrédient B' },
  { id: 4, name: 'Mixing', title: 'Agitation' },
  { id: 5, name: 'Verification', title: 'Vérification' },
  { id: 6, name: 'Finalizing', title: 'Finalisation' },
  { id: 7, name: 'Done', title: 'Terminé' },
];

function ProcessTimeline() {
  const { processData, isConnected } = useStore();

  if (!isConnected) {
    return (
      <Card shadow="sm" padding="lg" radius="md" withBorder>
        <Text c="dimmed" ta="center">
          Not connected to machine
        </Text>
      </Card>
    );
  }

  const currentStep = processData.currentStep || 0;

  const getStepIcon = (stepId) => {
    if (stepId < currentStep) {
      return <IconCircleCheck size={16} />;
    } else if (stepId === currentStep) {
      return <IconClock size={16} />;
    } else {
      return <IconCircle size={16} />;
    }
  };

  const getStepColor = (stepId) => {
    if (stepId < currentStep) return 'green';
    if (stepId === currentStep) return 'blue';
    return 'gray';
  };

  return (
    <Card shadow="sm" padding="lg" radius="md" withBorder>
      <Card.Section withBorder inheritPadding py="xs">
        <Text fw={500}>Process Timeline</Text>
      </Card.Section>

      <Timeline active={currentStep} mt="md" bulletSize={24} lineWidth={2}>
        {PROCESS_STEPS.filter((s) => s.id > 0).map((step) => (
          <Timeline.Item
            key={step.id}
            bullet={getStepIcon(step.id)}
            title={step.title}
            color={getStepColor(step.id)}
          >
            <Text c="dimmed" size="sm">
              {step.name}
            </Text>
          </Timeline.Item>
        ))}
      </Timeline>
    </Card>
  );
}

export default ProcessTimeline;
