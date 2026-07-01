import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Build ra thẳng wwwroot của API để ASP.NET phục vụ chung 1 origin (localhost:5094).
// Khi dev (npm run dev) thì proxy /api về API đang chạy ở cổng 5094.
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../backend/TemperatureApi/wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 1500,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5094',
    },
  },
});
