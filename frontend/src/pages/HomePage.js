import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Grid, Button, Title, Space } from '@mantine/core';
import { IconPlus, IconHistory } from '@tabler/icons-react';
import MachineSelector from '../components/MachineSelector';
import MachineStatus from '../components/MachineStatus';
import ProcessStatus from '../components/ProcessStatus';
import { useWebSocket } from '../hooks/useWebSocket';

function HomePage() {
  const navigate = useNavigate();

  // Connect WebSocket for real-time updates
  useWebSocket();

  return (
    <Container size="xl">
      <Title order={1} mb="xl">
        Dashboard
      </Title>

      <Grid>
        <Grid.Col span={{ base: 12, md: 6, lg: 4 }}>
          <MachineSelector />
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6, lg: 4 }}>
          <MachineStatus />
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6, lg: 4 }}>
          <ProcessStatus />
        </Grid.Col>
      </Grid>

      <Space h="xl" />

      <Grid>
        <Grid.Col span={{ base: 12, sm: 6 }}>
          <Button
            fullWidth
            size="lg"
            leftSection={<IconPlus size={20} />}
            onClick={() => navigate('/new-recipe')}
          >
            Create New Recipe
          </Button>
        </Grid.Col>

        <Grid.Col span={{ base: 12, sm: 6 }}>
          <Button
            fullWidth
            size="lg"
            variant="outline"
            leftSection={<IconHistory size={20} />}
            onClick={() => navigate('/history')}
          >
            View Recipe History
          </Button>
        </Grid.Col>
      </Grid>
    </Container>
  );
}

export default HomePage;
