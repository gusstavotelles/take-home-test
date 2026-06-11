import { test, expect } from '@playwright/test';

const uniqueUser = () => `e2e-user-${Date.now()}`;

test.describe('Loan Management E2E', () => {
  let username: string;
  const password = 'TestPass123!';

  test.beforeEach(async ({ page }) => {
    username = uniqueUser();

    // Register a fresh user
    await page.goto('/register');
    await page.getByLabel('Username').fill(username);
    await page.getByLabel('Password').fill(password);
    await page.getByRole('button', { name: 'Register' }).click();

    // Should redirect to loans page
    await expect(page).toHaveURL('/');
    await expect(page.getByText('Loan Management')).toBeVisible();
  });

  test('should create a new loan', async ({ page }) => {
    // Click new loan button
    await page.getByRole('button', { name: /new loan/i }).click();

    // Fill the form
    await page.getByLabel('Applicant Name').fill('E2E Test Applicant');
    await page.getByLabel('Amount').fill('5000');

    // Submit
    await page.getByRole('button', { name: /create/i }).click();

    // Verify the loan appears in the table
    await expect(page.getByText('E2E Test Applicant')).toBeVisible();
    await expect(page.getByText('5,000')).toBeVisible();
  });

  test('should apply payment to a loan', async ({ page }) => {
    // First create a loan
    await page.getByRole('button', { name: /new loan/i }).click();
    await page.getByLabel('Applicant Name').fill('Payment Test');
    await page.getByLabel('Amount').fill('1000');
    await page.getByRole('button', { name: /create/i }).click();
    await expect(page.getByText('Payment Test')).toBeVisible();

    // Click Pay button on the row
    await page.getByRole('button', { name: /pay/i }).first().click();

    // Fill payment amount
    await page.getByLabel('Amount').fill('400');
    await page.getByRole('button', { name: /submit|pay/i }).click();

    // Balance should update to 600
    await expect(page.getByText('600')).toBeVisible();
  });

  test('should pay off loan completely and mark as paid', async ({ page }) => {
    // Create a small loan
    await page.getByRole('button', { name: /new loan/i }).click();
    await page.getByLabel('Applicant Name').fill('Full Payment');
    await page.getByLabel('Amount').fill('200');
    await page.getByRole('button', { name: /create/i }).click();
    await expect(page.getByText('Full Payment')).toBeVisible();

    // Pay it all
    await page.getByRole('button', { name: /pay/i }).first().click();
    await page.getByLabel('Amount').fill('200');
    await page.getByRole('button', { name: /submit|pay/i }).click();

    // Should show as paid
    await expect(page.getByText('paid')).toBeVisible();
  });

  test('should redirect to login when not authenticated', async ({ page }) => {
    // Clear storage
    await page.evaluate(() => localStorage.clear());
    await page.goto('/');

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });

  test('should login with existing user', async ({ page }) => {
    // Logout
    await page.getByRole('button', { name: /logout/i }).click();
    await expect(page).toHaveURL(/\/login/);

    // Login
    await page.getByLabel('Username').fill(username);
    await page.getByLabel('Password').fill(password);
    await page.getByRole('button', { name: 'Login' }).click();

    // Should be back on loans page
    await expect(page).toHaveURL('/');
    await expect(page.getByText('Loan Management')).toBeVisible();
  });
});
