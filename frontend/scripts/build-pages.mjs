import { spawnSync } from 'node:child_process';
import { writeFileSync, rmSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { apiOrigin } from './pages-config.mjs';

const root = fileURLToPath(new URL('../', import.meta.url));
const generated = new URL('../src/environments/environment.pages.generated.ts', import.meta.url);
const apiUrl = apiOrigin(process.env.PUBLIC_API_URL);
// Only public configuration is generated. Existing environments stay untouched.
writeFileSync(
  generated,
  `export const environment = ${JSON.stringify(
    {
      production: true,
      apiUrl,
      stripePublishableKey: '',
    },
    null,
    2,
  )};\n`,
);
try {
  const result = spawnSync(
    process.execPath,
    ['node_modules/@angular/cli/bin/ng.js', 'build', '--configuration', 'production,pages'],
    { cwd: root, stdio: 'inherit' },
  );
  if (result.error) throw result.error;
  if (result.status !== 0) process.exitCode = result.status ?? 1;
  else {
    const output = new URL('../dist/pages/browser/', import.meta.url);
    if (!existsSync(new URL('index.html', output)))
      throw new Error('Pages browser output is missing.');
    // Pages natively falls back to index.html when no top-level 404.html exists.
    // A catch-all _redirects rewrite is rejected as a loop by current Wrangler.
    if (existsSync(new URL('404.html', output)))
      throw new Error('A top-level 404.html disables Pages SPA fallback.');
    writeFileSync(
      new URL('_headers', output),
      '/*\n  X-Robots-Tag: noindex, nofollow\n  X-Content-Type-Options: nosniff\n',
    );
    console.log('Cloudflare Pages assets: dist/pages/browser');
  }
} finally {
  rmSync(generated, { force: true });
}
