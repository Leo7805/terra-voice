import { useState } from 'react';
import {
  Box,
  Container,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button,
} from '@mui/material';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL; // e.g., http://localhost:5059

export function TtsPanel() {
  const [voice, setVoice] = useState('en-US-JennyNeural');
  const [speakingRate, setSpeakingRate] = useState(1.0);
  const [text, setText] = useState('Hello, this is a text-to-speech demo.');
  const [isLoading, setIsLoading] = useState(false);

  // connect with backend API to fetch available voices and play synthesized speech
  const handlePlay = async () => {
    if (isLoading) return; // Prevent multiple clicks while loading
    setIsLoading(true);

    let audioUrl: string | null = null; // To store the URL of the audio blob

    try {
      const res = await fetch(`${API_BASE_URL}/tts`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ text, voiceName: voice, speakingRate }),
      });

      if (!res.ok) {
        let detail = 'Failed to play speech.';
        try {
          const errorBody = await res.json();
          detail = errorBody.detail || detail;
        } catch {
          // If response is not JSON, fallback to text
          detail = await res.text();
        }
        throw new Error(detail);
      }

      // Read the response as an ArrayBuffer and create a Blob for audio playback
      const audioBuffer = await res.arrayBuffer();
      if (audioBuffer.byteLength === 0) {
        throw new Error('Received empty audio data from server.');
      }

      const contentType = res.headers.get('Content-Type') ?? 'audio/mpeg';
      const blob = new Blob([audioBuffer], { type: contentType });

      audioUrl = URL.createObjectURL(blob); // Create a URL for the audio blob
      const audio = new Audio(audioUrl); // Create an audio element
      // audio.playbackRate = speakingRate; // Set speaking rate
      await audio.play(); // Play the audio

      await new Promise<void>((resolve, reject) => {
        audio.onended = () => resolve(); // Resolve the promise when audio finishes playing
        audio.onerror = () => reject(new Error('Audio playback failed.')); // Handle playback errors
      });
    } catch (error) {
      console.error('Error occurred while playing speech:', error);
    } finally {
      if (audioUrl) {
        URL.revokeObjectURL(audioUrl); // Clean up the object URL to free memory
      }
      setIsLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Stack spacing={3}>
        {/* <Typography variant="h4">TerraVoice</Typography> */}

        {/* Voice Selection and Speaking Rate */}
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl sx={{ flex: 1 }}>
            <InputLabel>Voice</InputLabel>
            <Select
              value={voice}
              label="Voice"
              onChange={(e) => setVoice(e.target.value as string)}
            >
              <MenuItem value="zh-CN-XiaoxiaoNeural">
                Chinese (CN) - Xiaoxiao - Female
              </MenuItem>
              <MenuItem value="zh-CN-YunxiNeural">
                Chinese (CN) - Yunxi - Male
              </MenuItem>
              <MenuItem value="en-AU-NatashaNeural">
                English (AU) - Natasha - Female
              </MenuItem>
              <MenuItem value="en-AU-WilliamNeural">
                English (AU) - William - Male
              </MenuItem>
              <MenuItem value="en-GB-SoniaNeural">
                English (UK) - Sonia - Female
              </MenuItem>
              <MenuItem value="en-GB-RyanNeural">
                English (UK) - Ryan - Male
              </MenuItem>
              <MenuItem value="en-US-JennyNeural">
                English (US) - Jenny - Female
              </MenuItem>
              <MenuItem value="en-US-GuyNeural">
                English (US) - Guy - Male
              </MenuItem>
            </Select>
          </FormControl>

          <FormControl sx={{ flex: 0.5 }}>
            <InputLabel>Speaking Rate</InputLabel>
            <Select
              value={speakingRate}
              label="Speaking Rate"
              onChange={(e) => setSpeakingRate(e.target.value as number)}
            >
              {[0.5, 0.7, 1.0, 1.5, 2.0].map((rate) => (
                <MenuItem key={rate} value={rate}>
                  {rate}x
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {/* Text Input */}
        <TextField
          label="Text to speak"
          multiline
          minRows={4}
          fullWidth
          value={text}
          onChange={(e) => setText(e.target.value)}
        />

        <Button
          variant="contained"
          onClick={handlePlay}
          disabled={!text.trim() || isLoading}
        >
          {isLoading ? 'Playing...' : 'Play'}
        </Button>
      </Stack>
    </Container>
  );
}
