import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Group, Title, Button } from '@mantine/core';
import { IconHome, IconFileText, IconHistory } from '@tabler/icons-react';

function Header() {
  const navigate = useNavigate();
  const location = useLocation();

  return (
    <Group justify="space-between" h="100%" px="md">
      <Title order={3} style={{ color: '#fff' }}>
        🏭 Industrial App
      </Title>
      <Group>
        <Button
          variant={location.pathname === '/' ? 'filled' : 'subtle'}
          leftSection={<IconHome size={16} />}
          onClick={() => navigate('/')}
        >
          Home
        </Button>
        <Button
          variant={location.pathname === '/new-recipe' ? 'filled' : 'subtle'}
          leftSection={<IconFileText size={16} />}
          onClick={() => navigate('/new-recipe')}
        >
          New Recipe
        </Button>
        <Button
          variant={location.pathname === '/history' ? 'filled' : 'subtle'}
          leftSection={<IconHistory size={16} />}
          onClick={() => navigate('/history')}
        >
          History
        </Button>
      </Group>
    </Group>
  );
}

export default Header;
