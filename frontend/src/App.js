import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AppShell } from '@mantine/core';
import HomePage from './pages/HomePage';
import NewRecipePage from './pages/NewRecipePage';
import HistoryPage from './pages/HistoryPage';
import Header from './components/Header';

function App() {
  return (
    <Router>
      <AppShell
        header={{ height: 60 }}
        padding="md"
        styles={(theme) => ({
          main: {
            backgroundColor: theme.colors.dark[8],
            minHeight: '100vh',
          },
        })}
      >
        <AppShell.Header>
          <Header />
        </AppShell.Header>
        <AppShell.Main>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/new-recipe" element={<NewRecipePage />} />
            <Route path="/history" element={<HistoryPage />} />
          </Routes>
        </AppShell.Main>
      </AppShell>
    </Router>
  );
}

export default App;
