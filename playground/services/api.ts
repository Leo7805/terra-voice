const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5059';

export interface UsageSummary {
  provider: string;
  month: string;
  requestCount: number;
  charCount: number;
  quotaCharLimit: number;
  remainingCharCount: number;
}

export async function getUsageSummary(): Promise<UsageSummary> {
  const response = await fetch(`${API_BASE_URL}/usage`);
  if (!response.ok) {
    throw new Error(`Failed to fetch usage summary: ${response.statusText}`);
  }
  return response.json();
}
