import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Group, Title, Button, Badge } from '@mantine/core';
import { IconHome, IconFileText, IconHistory, IconSettings, IconCircleCheck, IconCircleX } from '@tabler/icons-react';
import { useStore } from '../hooks/useStore';

function Header() {
  const navigate = useNavigate();
  const location = useLocation();
  const { isConnected, selectedMachine } = useStore();

  return (
    <Group justify="space-between" h="100%" px="md">
      <Group>
        <Title order={3}>
          🏭 recipe-manager
        </Title>
        <Badge
          size="lg"
          color={isConnected ? 'green' : 'red'}
          leftSection={
            isConnected ? <IconCircleCheck size={14} /> : <IconCircleX size={14} />
          }
        >
          {isConnected ? `Connected: ${selectedMachine?.name || 'Unknown'}` : 'Disconnected'}
        </Badge>
      </Group>
      <Group>
        <Button
          variant={location.pathname === '/' ? 'filled' : 'subtle'}
          leftSection={<IconHome size={16} />}
          onClick={() => navigate('/')}
          styles={{
            root: {
              color: '#404040',
              '&[data-variant="filled"]': {
                backgroundColor: '#404040',
                color: 'white',
              },
            },
          }}
        >
          Home
        </Button>
        <Button
          variant={location.pathname === '/new-recipe' ? 'filled' : 'subtle'}
          leftSection={<IconFileText size={16} />}
          onClick={() => navigate('/new-recipe')}
          styles={{
            root: {
              color: '#404040',
              '&[data-variant="filled"]': {
                backgroundColor: '#404040',
                color: 'white',
              },
            },
          }}
        >
          New Recipe
        </Button>
        <Button
          variant={location.pathname === '/history' ? 'filled' : 'subtle'}
          leftSection={<IconHistory size={16} />}
          onClick={() => navigate('/history')}
          styles={{
            root: {
              color: '#404040',
              '&[data-variant="filled"]': {
                backgroundColor: '#404040',
                color: 'white',
              },
            },
          }}
        >
          History
        </Button>
        <Button
          variant={location.pathname === '/machine-settings' ? 'filled' : 'subtle'}
          leftSection={<IconSettings size={16} />}
          onClick={() => navigate('/machine-settings')}
          styles={{
            root: {
              color: '#404040',
              '&[data-variant="filled"]': {
                backgroundColor: '#404040',
                color: 'white',
              },
            },
          }}
        >
          Machine Settings
        </Button>
      </Group>
    </Group>
  );
}

export default Header;
