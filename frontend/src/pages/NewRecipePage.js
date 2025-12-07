import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  Container,
  Title,
  TextInput,
  NumberInput,
  Button,
  Group,
  Card,
  Text,
  Grid,
  Space,
  Alert,
  Stack,
} from '@mantine/core';
import {
  IconPlus,
  IconTrash,
  IconPlayerPlay,
  IconArrowLeft,
  IconAlertCircle,
} from '@tabler/icons-react';
import { api } from '../services/api';
import { useStore } from '../hooks/useStore';
import ProcessTimeline from '../components/ProcessTimeline';
import RealtimeChart from '../components/RealtimeChart';
import MachineStatus from '../components/MachineStatus';
import { useWebSocket } from '../hooks/useWebSocket';

function NewRecipePage() {
  const navigate = useNavigate();
  const { register, handleSubmit, watch } = useForm({
    defaultValues: {
      recipeName: '',
    },
  });

  const [ingredients, setIngredients] = useState([
    { name: '', quantity: 0, volume: 0, molarMass: 0 },
  ]);
  const [preparationVolume, setPreparationVolume] = useState(0);
  const [preparationConcentration, setPreparationConcentration] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRunning, setIsRunning] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const { isConnected, processData, setCurrentRecipe, clearProcessHistory } = useStore();

  // Connect WebSocket for real-time updates
  useWebSocket();

  const addIngredient = () => {
    setIngredients([...ingredients, { name: '', quantity: 0, volume: 0, molarMass: 0 }]);
  };

  const removeIngredient = (index) => {
    setIngredients(ingredients.filter((_, i) => i !== index));
  };

  const updateIngredient = (index, field, value) => {
    const updated = [...ingredients];
    updated[index][field] = value;
    setIngredients(updated);
  };

  const onSubmit = async (data) => {
    if (!isConnected) {
      setError('Please connect to a machine first');
      return;
    }

    if (ingredients.length === 0 || !ingredients[0].name) {
      setError('Please add at least one ingredient');
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      const recipe = {
        name: data.recipeName,
        ingredients: ingredients,
      };

      const result = await api.sendRecipe(recipe);

      if (result.success) {
        setCurrentRecipe(recipe);
        setSuccess('Recipe sent successfully to PLC!');
      } else {
        setError(result.error || 'Failed to send recipe');
      }
    } catch (err) {
      setError('Error sending recipe: ' + err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRunProcess = async () => {
    if (!isConnected) {
      setError('Please connect to a machine first');
      return;
    }

    setIsRunning(true);
    setError(null);
    clearProcessHistory();

    try {
      const result = await api.runProcess();
      if (result.success) {
        setSuccess('Process started successfully!');
      } else {
        setError(result.error || 'Failed to start process');
      }
    } catch (err) {
      setError('Error starting process: ' + err.message);
    } finally {
      setIsRunning(false);
    }
  };

  const handleGenerateReport = async () => {
    try {
      const reportData = {
        recipeName: watch('recipeName'),
        machineName: 'Mixing Unit A', // Should come from selected machine
        products: ingredients,
        steps: [
          {
            name: 'Preparation',
            time: 5,
            temp: 28,
            pressure: 3.2,
            speed: 0,
            remark: 'OK',
          },
          // Add more steps based on actual process data
        ],
      };

      const result = await api.createReport(reportData);
      if (result.success) {
        setSuccess('Report generated successfully!');
        setTimeout(() => {
          navigate('/history');
        }, 2000);
      }
    } catch (err) {
      setError('Error generating report: ' + err.message);
    }
  };

  return (
    <Container size="xl">
      <Group justify="space-between" mb="xl">
        <Title order={1}>New Recipe</Title>
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

      <Grid>
        <Grid.Col span={{ base: 12, lg: 6 }}>
          <Card shadow="sm" padding="lg" radius="md" withBorder>
            <form onSubmit={handleSubmit(onSubmit)}>
              <TextInput
                label="Recipe Name"
                placeholder="Enter recipe name"
                required
                {...register('recipeName')}
                mb="md"
              />

              <Text fw={500} mb="xs">
                Preparation
              </Text>
              
              <Grid gutter="md" mb="md">
                <Grid.Col span={6}>
                  <NumberInput
                    label="Volume (L)"
                    placeholder="Volume in liters"
                    value={preparationVolume}
                    onChange={setPreparationVolume}
                    min={0}
                    step={0.1}
                    decimalScale={2}
                  />
                </Grid.Col>
                <Grid.Col span={6}>
                  <NumberInput
                    label="Concentration (mol/L)"
                    placeholder="Concentration in mol/L"
                    value={preparationConcentration}
                    onChange={setPreparationConcentration}
                    min={0}
                    step={0.01}
                    decimalScale={3}
                  />
                </Grid.Col>
              </Grid>

              <Text fw={500} mb="xs">
                Ingredients
              </Text>

              <Stack gap="md">
                {ingredients.map((ingredient, index) => (
                  <Card key={index} withBorder padding="sm">
                    <Group justify="space-between" mb="xs">
                      <Text size="sm" fw={500}>
                        Ingredient {index + 1}
                      </Text>
                      {ingredients.length > 1 && (
                        <Button
                          size="xs"
                          color="red"
                          variant="subtle"
                          onClick={() => removeIngredient(index)}
                        >
                          <IconTrash size={14} />
                        </Button>
                      )}
                    </Group>

                    <TextInput
                      placeholder="Ingredient name"
                      value={ingredient.name}
                      onChange={(e) => updateIngredient(index, 'name', e.target.value)}
                      mb="xs"
                      size="sm"
                    />

                    <Grid gutter="xs">
                      <Grid.Col span={4}>
                        <NumberInput
                          placeholder="Quantity (g)"
                          value={ingredient.quantity}
                          onChange={(val) => updateIngredient(index, 'quantity', val)}
                          min={0}
                          size="sm"
                        />
                      </Grid.Col>
                      <Grid.Col span={4}>
                        <NumberInput
                          placeholder="Volume (ml)"
                          value={ingredient.volume}
                          onChange={(val) => updateIngredient(index, 'volume', val)}
                          min={0}
                          size="sm"
                        />
                      </Grid.Col>
                      <Grid.Col span={4}>
                        <NumberInput
                          placeholder="M.Mass (g/L)"
                          value={ingredient.molarMass}
                          onChange={(val) => updateIngredient(index, 'molarMass', val)}
                          min={0}
                          size="sm"
                        />
                      </Grid.Col>
                    </Grid>
                  </Card>
                ))}
              </Stack>

              <Button
                leftSection={<IconPlus size={16} />}
                variant="light"
                fullWidth
                mt="md"
                onClick={addIngredient}
              >
                Add Ingredient
              </Button>

              <Space h="xl" />

              <Group>
                <Button type="submit" loading={isSubmitting} disabled={!isConnected}>
                  Send Recipe to PLC
                </Button>
                <Button
                  leftSection={<IconPlayerPlay size={16} />}
                  color="green"
                  onClick={handleRunProcess}
                  loading={isRunning}
                  disabled={!isConnected}
                >
                  Run Process
                </Button>
              </Group>

              {processData?.processDone && (
                <Button fullWidth mt="md" onClick={handleGenerateReport}>
                  Generate PDF Report
                </Button>
              )}
            </form>
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, lg: 6 }}>
          <Stack gap="md">
            <MachineStatus />
            <ProcessTimeline />
          </Stack>
        </Grid.Col>

        <Grid.Col span={12}>
          <RealtimeChart />
        </Grid.Col>
      </Grid>
    </Container>
  );
}

export default NewRecipePage;
