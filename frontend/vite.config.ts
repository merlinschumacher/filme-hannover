// vite.config.js
import { defineConfig } from 'vite';
import htmlMinifier from 'vite-plugin-html-minifier';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import htmlTemplatePlugin from './plugins/vite-plugin-html-template';
import { optimizeCssModules } from 'vite-plugin-optimize-css-modules';

export default defineConfig({
  plugins: [
    {
      name: 'html-transform',
      transformIndexHtml(html) {
        const d = new Date();
        let date = d.toISOString();
        return html.replace(
          /<meta http-equiv="last-modified" content="date">/,
          `<meta http-equiv="last-modified" content="` + date + `"/>`,
        )
      },
    },
    ViteImageOptimizer({}),
    optimizeCssModules(),
    htmlTemplatePlugin(),
    htmlMinifier({
      minify: true,
    }),
  ],
  build: {
    target: 'esnext',
  },
  esbuild: {
    pure: ['console.log', 'console.debug', 'console.info', 'console.warn'],

  },
});

