import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Button,
  Group,
  Card,
  Text,
  Badge,
  Loader,
  Alert,
  SimpleGrid,
  Stack,
} from '@mantine/core';
import { IconArrowLeft, IconDownload, IconAlertCircle } from '@tabler/icons-react';
import { api } from '../services/api';

// Fake history data
const fakeHistory = [
  {
    id: '1',
    recipeName: 'Chocolate Chip Cookies',
    machineName: 'Mixing Unit A',
    date: new Date('2025-12-07T10:30:00'),
    products: [
      { name: 'Flour' },
      { name: 'Sugar' },
      { name: 'Butter' },
      { name: 'Chocolate Chips' },
      { name: 'Eggs' },
    ],
  },
  {
    id: '2',
    recipeName: 'Vanilla Cupcakes',
    machineName: 'Mixing Unit B',
    date: new Date('2025-12-06T14:20:00'),
    products: [
      { name: 'Flour' },
      { name: 'Sugar' },
      { name: 'Vanilla Extract' },
      { name: 'Milk' },
      { name: 'Eggs' },
    ],
  },
  {
    id: '3',
    recipeName: 'Sourdough Bread',
    machineName: 'Mixing Unit A',
    date: new Date('2025-12-05T08:15:00'),
    products: [
      { name: 'Flour' },
      { name: 'Water' },
      { name: 'Sourdough Starter' },
      { name: 'Salt' },
    ],
  },
  {
    id: '4',
    recipeName: 'Lemon Tart',
    machineName: 'Mixing Unit C',
    date: new Date('2025-12-04T16:45:00'),
    products: [
      { name: 'Flour' },
      { name: 'Butter' },
      { name: 'Lemon Juice' },
      { name: 'Sugar' },
      { name: 'Eggs' },
    ],
  },
  {
    id: '5',
    recipeName: 'Red Velvet Cake',
    machineName: 'Mixing Unit B',
    date: new Date('2025-12-03T11:00:00'),
    products: [
      { name: 'Flour' },
      { name: 'Cocoa Powder' },
      { name: 'Buttermilk' },
      { name: 'Red Food Coloring' },
      { name: 'Cream Cheese' },
    ],
  },
];

function HistoryPage() {
  const navigate = useNavigate();
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadHistory();
  }, []);

  const loadHistory = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await api.getHistory();
      if (result.success && result.history && result.history.length > 0) {
        setHistory(result.history);
      } else {
        // Use fake data when no real history exists
        setHistory(fakeHistory);
      }
    } catch (err) {
      // Use fake data on error
      setHistory(fakeHistory);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = (reportId) => {
    api.downloadPDF(reportId);
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <Container size="xl">
      <Group justify="space-between" mb="xl">
        <Title order={1}>Recipe History</Title>
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

      {error && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" mb="md" onClose={() => setError(null)} withCloseButton>
          {error}
        </Alert>
      )}

      {loading ? (
        <Card shadow="sm" padding="xl" radius="md" withBorder>
          <Group justify="center">
            <Loader />
            <Text>Loading history...</Text>
          </Group>
        </Card>
      ) : history.length === 0 ? (
        <Card shadow="sm" padding="xl" radius="md" withBorder>
          <Text ta="center" c="dimmed">
            No recipe history yet. Create and complete a recipe to generate reports.
          </Text>
        </Card>
      ) : (
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="lg">
          {history.map((report) => (
            <Card key={report.id} shadow="md" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <div>
                  <Text fw={700} size="xl" mb="xs">
                    {report.recipeName}
                  </Text>
                  <Text size="sm" c="dimmed">
                    {report.machineName}
                  </Text>
                  <Text size="sm" c="dimmed">
                    {formatDate(report.date)}
                  </Text>
                </div>

                <div>
                  <Text size="sm" fw={500} mb="xs">
                    Ingredients:
                  </Text>
                  <Group gap="xs">
                    {report.products?.slice(0, 3).map((p, i) => (
                      <Badge key={i} size="md" variant="light" color="blue">
                        {p.name}
                      </Badge>
                    ))}
                    {report.products?.length > 3 && (
                      <Badge size="md" variant="light" color="gray">
                        +{report.products.length - 3} more
                      </Badge>
                    )}
                  </Group>
                </div>

                <Button
                  fullWidth
                  leftSection={<IconDownload size={16} />}
                  onClick={() => handleDownload(report.id)}
                  variant="light"
                >
                  Download PDF
                </Button>
              </Stack>
            </Card>
          ))}
        </SimpleGrid>
      )}
    </Container>
  );
}

export default HistoryPage;
