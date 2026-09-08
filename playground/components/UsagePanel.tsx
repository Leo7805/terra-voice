'use client';

import { useEffect, useState } from 'react';
import {
  Box,
  Container,
  Stack,
  Typography,
  CircularProgress,
  Alert,
} from '@mui/material';
import { getUsageSummary, type UsageSummary } from '../services/api';

export function UsagePanel() {
  const [usage, setUsage] = useState<UsageSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUsage = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getUsageSummary();
        setUsage(data);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : 'Failed to load usage data'
        );
        console.error('Error fetching usage:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchUsage();
  }, []);

  if (loading) {
    return (
      <Container
        maxWidth="sm"
        sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}
      >
        <CircularProgress />
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="sm" sx={{ mt: 4 }}>
        <Alert severity="error">{error}</Alert>
      </Container>
    );
  }

  if (!usage) {
    return (
      <Container maxWidth="sm" sx={{ mt: 4 }}>
        <Alert severity="warning">No usage data available</Alert>
      </Container>
    );
  }

  const rows = [
    ['Provider', usage.provider],
    ['Month', usage.month],
    ['Requests', usage.requestCount.toLocaleString()],
    ['Characters Used', usage.charCount.toLocaleString()],
    ['Quota', usage.quotaCharLimit.toLocaleString()],
    ['Remaining', usage.remainingCharCount.toLocaleString()],
  ];

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Stack spacing={1}>
        {rows.map(([label, value]) => (
          <Box
            key={label}
            sx={{
              display: 'grid',
              gridTemplateColumns: '150px 1fr',
              columnGap: 2,
              alignItems: 'baseline',
            }}
          >
            <Typography
              sx={{
                textAlign: 'left',
                color: 'text.secondary',
                fontSize: '0.875rem',
              }}
            >
              {label}:
            </Typography>
            <Typography
              sx={{
                textAlign: 'left',
                fontWeight: 400,
                fontSize: '0.875rem',
                color: 'text.primary',
              }}
            >
              {value}
            </Typography>
          </Box>
        ))}
      </Stack>
    </Container>
  );
}
