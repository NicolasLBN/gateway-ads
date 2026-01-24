import React, { useState } from 'react';
import {
  Container,
  Paper,
  Title,
  Text,
  TextInput,
  PasswordInput,
  Button,
  Stack,
  Alert,
} from '@mantine/core';
import { IconAlertCircle, IconLogin } from '@tabler/icons-react';

function LoginPage({ onLogin }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    // Simple authentication logic - in production, this should connect to a real backend
    // Default credentials: admin / admin
    if (username === 'admin' && password === 'admin') {
      localStorage.setItem('isAuthenticated', 'true');
      localStorage.setItem('username', username);
      onLogin();
    } else {
      setError('Invalid username or password');
    }
    
    setLoading(false);
  };

  return (
    <Container size="xs" style={{ marginTop: '10vh' }}>
      <Paper shadow="md" p="xl" radius="md" withBorder>
        <Stack gap="lg">
          <div>
            <Title order={2} ta="center" mb="xs">
              Gateway ADS
            </Title>
            <Text ta="center" c="dimmed" size="sm">
              Please login to continue
            </Text>
          </div>

          {error && (
            <Alert
              icon={<IconAlertCircle size={16} />}
              color="red"
              withCloseButton
              onClose={() => setError(null)}
            >
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <Stack gap="md">
              <TextInput
                label="Username"
                placeholder="Enter your username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                size="md"
              />

              <PasswordInput
                label="Password"
                placeholder="Enter your password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                size="md"
              />

              <Button
                type="submit"
                fullWidth
                size="md"
                loading={loading}
                leftSection={<IconLogin size={18} />}
                styles={{
                  root: {
                    backgroundColor: '#61db34',
                    '&:hover': {
                      backgroundColor: '#4fb828',
                    },
                  },
                }}
              >
                Login
              </Button>
            </Stack>
          </form>

          <Text size="xs" c="dimmed" ta="center">
            Default credentials: admin / admin
          </Text>
        </Stack>
      </Paper>
    </Container>
  );
}

export default LoginPage;
