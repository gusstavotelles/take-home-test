import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  workers: 1,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  webServer: [
    {
      command: 'cd ../backend/src && dotnet run --project Fundo.Applications.WebApi',
      url: 'http://localhost:5000/health',
      reuseExistingServer: !process.env['CI'],
      timeout: 30_000,
    },
    {
      command: 'yarn start',
      url: 'http://localhost:4200',
      reuseExistingServer: !process.env['CI'],
      timeout: 30_000,
    },
  ],
});
