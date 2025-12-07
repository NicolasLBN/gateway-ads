import React from 'react';
import ReactDOM from 'react-dom/client';
import { MantineProvider } from '@mantine/core';
import '@mantine/core/styles.css';
import App from './App';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <MantineProvider
      theme={{
        primaryColor: 'blue',
        fontFamily: 'Arial, sans-serif',
        components: {
          Button: {
            defaultProps: {
              size: 'lg',
              radius: 'xl',
            },
          },
        },
      }}
      withGlobalStyles
      withNormalizeCSS
    >
      <App />
    </MantineProvider>
  </React.StrictMode>
);
