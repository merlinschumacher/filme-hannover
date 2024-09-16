import { Plugin } from "vite";
import { minify } from "html-minifier-terser";
import { promises as fs } from "fs";

export default function htmlTemplatePlugin(): Plugin {
  return {
    name: "vite-plugin-html-template",

    resolveId(source) {
      if (source.endsWith(".tpl")) {
        return source;
      }
      return null;
    },

    async load(id) {
      if (id.endsWith(".tpl")) {
        try {
          const html = await fs.readFile(id, "utf-8");
          const minifiedHtml = await minify(html, {
            collapseWhitespace: true,
            removeComments: true,
            minifyCSS: true,
            minifyJS: true,
            collapseInlineTagWhitespace: true,
            removeAttributeQuotes: true,
            removeEmptyAttributes: true,
            removeOptionalTags: true,
            removeRedundantAttributes: true,
            removeScriptTypeAttributes: true,
            removeStyleLinkTypeAttributes: true,
            sortAttributes: true,
            sortClassName: true,
          });

          return `
            const template = document.createElement('template');
            template.innerHTML = \`${minifiedHtml}\`;
            export default template;
          `;
        } catch (error) {
          if (error instanceof Error) {
            console.error(`Error loading HTML file: ${error.message}`);
            this.error(
              `Failed to load HTML file: ${id}, error: ${error.message}`,
            );
          } else {
            this.error(`Failed to load HTML file: ${id}, unknown error`);
          }
        }
      }
      return null;
    },
  };
}
