import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Button,
  Group,
  Card,
  Text,
  Table,
  Badge,
  Loader,
  Alert,
} from '@mantine/core';
import { IconArrowLeft, IconDownload, IconAlertCircle } from '@tabler/icons-react';
import { api } from '../services/api';

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
      if (result.success) {
        setHistory(result.history || []);
      } else {
        setError(result.error || 'Failed to load history');
      }
    } catch (err) {
      setError('Error loading history: ' + err.message);
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
        >
          Back to Dashboard
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
        <Card shadow="sm" padding="lg" radius="md" withBorder>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Recipe Name</Table.Th>
                <Table.Th>Machine</Table.Th>
                <Table.Th>Date</Table.Th>
                <Table.Th>Products</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {history.map((report) => (
                <Table.Tr key={report.id}>
                  <Table.Td>
                    <Text fw={500}>{report.recipeName}</Text>
                  </Table.Td>
                  <Table.Td>{report.machineName}</Table.Td>
                  <Table.Td>
                    <Text size="sm">{formatDate(report.date)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Group gap="xs">
                      {report.products?.slice(0, 3).map((p, i) => (
                        <Badge key={i} size="sm" variant="light">
                          {p.name}
                        </Badge>
                      ))}
                      {report.products?.length > 3 && (
                        <Badge size="sm" variant="light">
                          +{report.products.length - 3}
                        </Badge>
                      )}
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Button
                      size="xs"
                      leftSection={<IconDownload size={14} />}
                      onClick={() => handleDownload(report.id)}
                    >
                      Download PDF
                    </Button>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      )}
    </Container>
  );
}

export default HistoryPage;
