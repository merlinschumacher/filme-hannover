// vite.config.js
import { defineConfig } from 'vite'
import { minify } from 'html-minifier';
import eslintPlugin from "@nabla/vite-plugin-eslint";

export default defineConfig({
  plugins: [htmlMinify(), eslintPlugin()],
  build: {
    target: 'esnext',
    minify: 'esbuild',
  },
  esbuild: {
    treeShaking: true,
    pure: [ 'console.log' ],
    minifyIdentifiers: true,
  }
});

const htmlComponentFile = /\.component\.html\?inline$/;

const minifyHTMLConfig = {
  collapseInlineTagWhitespace: true,
  collapseWhitespace: true,
  minifyCSS: true,
  minifyJS: true,
  removeAttributeQuotes: true,
  removeComments: true,
  removeEmptyAttributes: true,
  removeOptionalTags: true,
  removeRedundantAttributes: true,
  removeScriptTypeAttributes: true,
  removeStyleLinkTypeAttributes: true,
  sortAttributes: true,
  sortClassName: true,
};

function htmlMinify() {
  return {
    name: 'html-minify',

    transform(src: string, id: string) {
      if (htmlComponentFile.test(id)) {
        return {
          code: `export default \`${minify(src, minifyHTMLConfig)}\``,
          map: null,
        };
      } else {
        return;
      }
    },
  };
}
