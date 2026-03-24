// @ts-check
import { defineConfig } from 'astro/config';

// https://astro.build/config
export default defineConfig({
  site: 'https://qibla-now.com',
  // Accept both /es and /es/ without the dev-server warning
  trailingSlash: 'ignore',
});
