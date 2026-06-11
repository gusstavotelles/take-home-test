# **Take-Home Test: Backend-Focused Full-Stack Developer (.NET C# & Angular)**

## **Objective**

This take-home test evaluates your ability to develop and integrate a .NET Core (C#) backend with an Angular frontend, focusing on API design, database integration, and basic DevOps practices.

## **Instructions**

1.  **Fork the provided repository** before starting the implementation.
2.  Implement the requested features in your forked repository.
3.  Once you have completed the implementation, **send the link** to your forked repository via email for review.

## **Task**

You will build a simple **Loan Management System** with a **.NET Core backend (C#)** exposing RESTful APIs and a **basic Angular frontend** consuming these APIs.

---

## **Requirements**

### **1. Backend (API) - .NET Core**

* Create a **RESTful API** in .NET Core to handle **loan applications**.
* Implement the following endpoints:
    * `POST /loans` → Create a new loan.
    * `GET /loans/{id}` → Retrieve loan details.
    * `GET /loans` → List all loans.
    * `POST /loans/{id}/payment` → Deduct from `currentBalance`.
* Loan example (feel free to improve it):

    ```json
    {
        "amount": 1500.00, // Amount requested
        "currentBalance": 500.00, // Remaining balance
        "applicantName": "Maria Silva", // User name
        "status": "active" // Status can be active or paid
    }
    ```

* Use **Entity Framework Core** with **SQL Server**.
* Create seed data to populate the loans (the frontend will consume this).
* Write **unit/integration tests for the API** (xUnit or NUnit).
* **Dockerize** the backend and create a **Docker Compose** file.
* Create a README with setup instructions.

### **2. Frontend - Angular (Simplified UI)**  

Develop a **lightweight Angular app** to interact with the backend

#### **Features:**  
- A **table** to display a list of existing loans.  

#### **Mockup:**  
[View Mockup](https://kzmgtjqt0vx63yji8h9l.lite.vusercontent.net/)  
(*The design doesn’t need to be an exact replica of the mockup—it serves as a reference. Aim to keep it as close as possible.*)  

---

## **Bonus (Optional, Not Required)**

* **Improve error handling and logging** with structured logs.
* Implement **authentication**.
* Create a **GitHub Actions** pipeline for building and testing the backend.

---

## **Evaluation Criteria**

✔ **Code quality** (clean architecture, modularization, best practices).

✔ **Functionality** (the API and frontend should work as expected).

✔ **Security considerations** (authentication, validation, secure API handling).

✔ **Testing coverage** (unit tests for critical backend functions).

✔ **Basic DevOps implementation** (Docker for backend).

---

## **Implementation Notes**

### Stack

| Layer      | Technology                                                        |
| ---------- | ----------------------------------------------------------------- |
| Backend    | .NET 10, ASP.NET Core, EF Core 10 (SQL Server / InMemory)         |
| Auth       | JWT Bearer tokens (BCrypt password hashing)                       |
| Validation | FluentValidation                                                  |
| Logging    | Serilog (structured console)                                      |
| Docs       | Swashbuckle / Swagger UI                                          |
| Tests      | xUnit, FluentAssertions, WebApplicationFactory, Testcontainers    |
| Frontend   | Angular 19, Angular Material, signals, standalone components      |
| E2E        | Playwright                                                        |
| CI         | GitHub Actions (backend + frontend)                               |
| Infra      | Dockerfile + Docker Compose (API + SQL Server 2022)               |

### Project layout

```
take-home-test/
  backend/
    Dockerfile
    src/
      Fundo.Applications.WebApi/   # Web API (Controllers, Services, Data, DTOs, Validators, Middleware)
        Migrations/                # EF Core migration history
      Fundo.Services.Tests/        # Unit + Integration tests (xUnit + Testcontainers)
  frontend/
    src/app/
      auth/                        # Login/Register components, AuthService
      loans/                       # Models, services, store (signals), components
      interceptors/                # Auth token + global error toast interceptors
      guards/                      # Auth route guard
    e2e/                           # Playwright e2e tests
  docker-compose.yml               # Brings up SQL Server + API
  .github/workflows/               # Backend & Frontend CI
```

### Endpoints

| Method | Path                       | Auth     | Description                           |
| ------ | -------------------------- | -------- | ------------------------------------- |
| POST   | `/auth/register`           | Public   | Register a new user                   |
| POST   | `/auth/login`              | Public   | Authenticate and get JWT token        |
| GET    | `/loans`                   | Required | List all loans (per-user)             |
| GET    | `/loans/{id}`              | Required | Get a loan by id (per-user)           |
| POST   | `/loans`                   | Required | Create a new loan                     |
| POST   | `/loans/{id}/payment`      | Required | Apply a payment to a loan             |
| GET    | `/health`                  | Public   | Health probe                          |
| GET    | `/swagger`                 | Public   | OpenAPI documentation (development)   |

Validation, structured logging and a global exception middleware translate domain errors to consistent JSON responses (`400`, `401`, `404`, `409`, `500`).

### Running everything

**Quickest path — backend (in-memory) + frontend locally:**

```bash
# Terminal 1
cd backend/src
dotnet run --project Fundo.Applications.WebApi
# API → http://localhost:5000  (Swagger at /swagger)

# Terminal 2
cd frontend
yarn install
yarn start
# UI → http://localhost:4200
```

**Full stack with Docker (API + SQL Server):**

```bash
docker compose up --build
# API → http://localhost:5000
```

Run the frontend locally pointing at the dockerized API:

```bash
cd frontend
yarn install
yarn start
```

### Tests

```bash
# Backend (xUnit + integration)
cd backend/src
dotnet test

# Backend (Testcontainers - requires Docker running)
cd backend/src
dotnet test --filter "FullyQualifiedName~SqlServer"

# Frontend (Karma + Jasmine, headless Chrome)
cd frontend
yarn test --watch=false --browsers=ChromeHeadless

# E2E (Playwright - requires backend + frontend running)
cd frontend
yarn e2e
```

### Manual test script

1. Start backend and frontend as described above.
2. Open `http://localhost:4200`. You'll be redirected to the login page.
3. Click **Register**, create a username/password, and submit. You'll be redirected to the loans page.
4. Click **New Loan**, fill applicant + amount, submit. The new loan appears at the top of the table.
5. Click **Pay** on an active loan, enter an amount ≤ current balance, submit. The current balance updates; if the payment closes the balance, the row's status flips to `paid` and the **Pay** button is disabled.
6. Open Swagger at `http://localhost:5000/swagger` to exercise endpoints directly. You'll need to authenticate first via `/auth/login` and use the Bearer token.
7. Try invalid payloads (negative amount, empty applicant, payment > balance) — a global snackbar shows structured error messages.
8. Click **Logout** in the toolbar — you'll be redirected to the login page and API calls will return 401.

## **Additional Information**

Candidates are encouraged to include a `README.md` file in their repository detailing their implementation approach, any challenges they faced, features they couldn't complete, and any improvements they would make given more time. Ideally, the implementation should be completed within **two days** of starting the test.
