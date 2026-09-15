import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => ({
  plugins: [react()],
  base: mode === 'github' ? '/efsr14908-bodega-norte/' : '/',
  build: {
    outDir: mode === 'visualstudio' ? 'backend/BodegaNorte.Api/wwwroot' : 'dist',
    emptyOutDir: true,
  },
}));
