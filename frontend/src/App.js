import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AppShell, Button, Group } from '@mantine/core';
import { IconLogout } from '@tabler/icons-react';
import HomePage from './pages/HomePage';
import NewRecipePage from './pages/NewRecipePage';
import HistoryPage from './pages/HistoryPage';
import MachineSettingsPage from './pages/MachineSettingsPage';
import DeveloperPage from './pages/DeveloperPage';
import LoginPage from './pages/LoginPage';
import Header from './components/Header';
import Footer from './components/Footer';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    // Check if user is already authenticated
    const authStatus = localStorage.getItem('isAuthenticated');
    if (authStatus === 'true') {
      setIsAuthenticated(true);
    }
  }, []);

  const handleLogin = () => {
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('username');
    setIsAuthenticated(false);
  };

  if (!isAuthenticated) {
    return (
      <Router>
        <Routes>
          <Route path="*" element={<LoginPage onLogin={handleLogin} />} />
        </Routes>
      </Router>
    );
  }

  return (
    <Router>
      <AppShell
        header={{ height: 60 }}
        footer={{ height: 60 }}
        padding="md"
        styles={(theme) => ({
          main: {
            backgroundColor: '#f5f5f5',
            minHeight: '100vh',
            paddingBottom: '80px',
          },
        })}
      >
        <AppShell.Header>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', height: '100%', paddingRight: '20px' }}>
            <Header />
            <Button
              variant="subtle"
              leftSection={<IconLogout size={16} />}
              onClick={handleLogout}
              styles={{
                root: {
                  color: '#61db34',
                  '&:hover': {
                    backgroundColor: 'rgba(97, 219, 52, 0.1)',
                  },
                },
              }}
            >
              Logout
            </Button>
          </div>
        </AppShell.Header>
        <AppShell.Main>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/new-recipe" element={<NewRecipePage />} />
            <Route path="/history" element={<HistoryPage />} />
            <Route path="/machine-settings" element={<MachineSettingsPage />} />
            <Route path="/developer" element={<DeveloperPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AppShell.Main>
        <Footer />
      </AppShell>
      
    </Router>
  );
}

export default App;
