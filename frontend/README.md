# Frontend — Loan Management UI

Angular 19 standalone application that consumes the .NET backend to manage loans.

## Stack

- Angular 19 (standalone components, signals, control flow)
- Angular Material (table, dialogs, form fields, buttons, spinner)
- Reactive Forms
- Karma + Jasmine for unit tests

## Architecture

```
src/app/
  app.component.ts            # Shell with router-outlet
  app.config.ts               # HttpClient + animations + router providers
  app.routes.ts               # / → LoansPageComponent
  loans/
    models/                   # Type contracts (Loan, requests)
    services/                 # HTTP layer (LoanService)
    store/                    # Signal-based state (LoanStore)
    components/
      loans-page/             # Page container (table + actions)
      create-loan-dialog/     # New loan form
      payment-dialog/         # Apply payment form
```

The page component reads state from `LoanStore`, which delegates HTTP calls to `LoanService`. Components do not call HTTP directly and the service does not own state.

## Configuration

`src/environments/environment.ts` controls the backend base URL. Defaults to `http://localhost:5000`.

## Running

```sh
cd frontend
yarn install
yarn start
```

Open `http://localhost:4200`. The backend must be running at the URL configured in the environment file.

## Building

```sh
yarn build
```

## Testing

```sh
yarn test --watch=false --browsers=ChromeHeadless
```
