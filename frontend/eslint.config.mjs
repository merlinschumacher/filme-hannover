import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import eslint from "@eslint/js";

export default [
  { files: ["**/*.{js,mjs,cjs,ts}"] },
  { languageOptions: {
    globals: globals.browser,
    parserOptions: {
      project: "./tsconfig.json",
    },
  } },
  eslint.configs.recommended,
  pluginJs.configs.recommended,
  ...tseslint.configs.strictTypeChecked,
  ...tseslint.configs.stylisticTypeChecked,
];
