// vite.config.js
import { defineConfig } from 'vite';
import htmlMinifier from 'vite-plugin-html-minifier';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import htmlTemplatePlugin from './plugins/vite-plugin-html-template';
import eslint from 'vite-plugin-eslint';

export default defineConfig({
  plugins: [
    ViteImageOptimizer({}),
    htmlTemplatePlugin(),
    htmlMinifier({
      minify: true,
    }),
    eslint({ cache: false, fix: true }),
  ],
  build: {
    target: 'esnext',
  },
  esbuild: {
    pure: ['console.log', 'console.debug', 'console.info', 'console.warn'],
  },
});
