import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => ({
  plugins: [react()],
  base: mode === 'github' ? '/efsr14908-bodega-norte/' : '/',
  build: {
    manifest: true,
    outDir: mode === 'visualstudio' ? 'backend/BodegaNorte.Api/wwwroot' : 'dist',
    emptyOutDir: true,
    rolldownOptions: {
      output: {
        entryFileNames: 'assets/[name]-[hash].js',
        chunkFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash][extname]',
        codeSplitting: { groups: [{ name: 'vendor', test: /node_modules/ }] },
      },
    },
  },
}));
