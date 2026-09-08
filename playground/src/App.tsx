import { useState } from 'react';
import { Box, Container, Paper, Tabs, Tab } from '@mui/material';
import { TtsPanel } from '../components/TtsPanel';
import { UsagePanel } from '../components/UsagePanel';

type ActiveTab = 'tts' | 'usage';

function App() {
  const [activeTab, setActiveTab] = useState<ActiveTab>('tts');

  const handleTabChange = (_event: React.SyntheticEvent, newValue: string) => {
    setActiveTab(newValue as ActiveTab);
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f1f5f9', p: 3 }}>
      <Container maxWidth="sm">
        <Paper elevation={4} sx={{ p: 3, borderRadius: 2 }}>
          <Tabs
            value={activeTab}
            onChange={handleTabChange}
            sx={{
              mb: 3,
              borderBottom: 1,
              borderColor: 'divider',
              '& .MuiTab-root': {
                textTransform: 'none',
                fontSize: '1rem',
              },
            }}
          >
            <Tab label="Azure TTS" value="tts" />
            <Tab label="Azure Usage" value="usage" />
          </Tabs>

          {activeTab === 'tts' && <TtsPanel />}
          {activeTab === 'usage' && <UsagePanel />}
        </Paper>
      </Container>
    </Box>
  );
}

export default App;
