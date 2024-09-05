// vite.config.js
import { defineConfig } from "vite";
import htmlMinifier from "vite-plugin-html-minifier";

export default defineConfig({
  plugins: [
    htmlMinifier({
      minify: true,
    }),
  ],
  build: {
    target: "esnext",
    minify: "esbuild",
  },
  esbuild: {
    treeShaking: true,
    pure: [
      "console.log",
      "console.debug",
      "console.info",
      "console.warn",
      "console.error",
    ],
    minifyIdentifiers: true,
  },
  assetsInclude: [".src/**/*.html"],
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

// function htmlMinify() {
//   return {
//     name: 'html-minify',

//     transform(src: string, id: string) {
//       if (htmlComponentFile.test(id)) {
//         return {
//           code: `export default \`${minify(src, minifyHTMLConfig)}\``,
//           map: null,
//         };
//       } else {
//         return;
//       }
//     },
//   };
// }
