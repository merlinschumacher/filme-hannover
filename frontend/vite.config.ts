// vite.config.js
import { defineConfig } from "vite";
import htmlMinifier from "vite-plugin-html-minifier";
import { standardCssModules } from "vite-plugin-standard-css-modules";
import { minify } from "html-minifier-terser";

//       if (htmlComponentFile.test(id)) {
//         return {
//           code: `export default \`${minify(src, minifyHTMLConfig)}\``,
//           map: null,
//         };
//       } else {
//         return;
//       }
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

function htmlImport() {
  return {
    name: "html-minify",

    async transform(src: string, id: string) {
      if (id.endsWith(".component.html")) {
        return {
          code: `export default \`${await minify(src, minifyHTMLConfig)}\``,
          map: null,
        };
      } else {
        return;
      }
    },
  };
}

// const htmlImport = () => ({
//   name: "html-import",
//   async transform(src: string, id: string) {
//     // Skip index.html
//     if (path.resolve("index.html") === id) {
//       return;
//     }

//     if (id.endsWith(".component.html")) {
//       try {
//         // Compress HTML
//         const html = await minify(src, {
//           collapseWhitespace: true,
//           removeComments: true,
//           minifyCSS: true,
//           removeAttributeQuotes: true,
//           removeEmptyAttributes: true,
//           quoteCharacter: "'",
//           sortClassName: true,
//           sortAttributes: true,
//           removeRedundantAttributes: true,
//         });

//         const htmlTemplate = `
//           const htmlTemplate = document.createElement('template');
//           htmlTemplate.innerHTML = ${JSON.stringify(html)};
//           export default htmlTemplate;
//         `;
//         console.log(htmlTemplate);

//         return {
//           code: htmlTemplate,
//           map: null,
//         };
//       } catch (error) {
//         console.error(error);
//         return;
//       }
//     }
//   },
// });

export default defineConfig({
  plugins: [
    htmlImport(),
    standardCssModules(),
    htmlMinifier({
      minify: true,
    }),
  ],
  build: {
    target: "esnext",
    minify: "esbuild",
    cssCodeSplit: false,
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
