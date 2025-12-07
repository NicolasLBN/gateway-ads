import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Grid, Button, Title, Space } from '@mantine/core';
import { IconPlus, IconHistory, IconSettings } from '@tabler/icons-react';
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

      <Space h="xl" />

      <Grid>
        <Grid.Col span={{ base: 12, sm: 4 }}>
          <Button
            fullWidth
            size="xl"
            color="blue"
            leftSection={<IconPlus size={24} />}
            onClick={() => navigate('/new-recipe')}
            styles={{
              root: {
                height: '120px',
                fontSize: '20px',
              },
            }}
          >
            Create New Recipe
          </Button>
        </Grid.Col>

        <Grid.Col span={{ base: 12, sm: 4 }}>
          <Button
            fullWidth
            size="xl"
            color="blue"
            leftSection={<IconHistory size={24} />}
            onClick={() => navigate('/history')}
            styles={{
              root: {
                height: '120px',
                fontSize: '20px',
              },
            }}
          >
            View Recipe History
          </Button>
        </Grid.Col>

        <Grid.Col span={{ base: 12, sm: 4 }}>
          <Button
            fullWidth
            size="xl"
            color="blue"
            leftSection={<IconSettings size={24} />}
            onClick={() => navigate('/machine-settings')}
            styles={{
              root: {
                height: '120px',
                fontSize: '20px',
              },
            }}
          >
            Machine Settings
          </Button>
        </Grid.Col>
      </Grid>
    </Container>
  );
}

export default HomePage;
