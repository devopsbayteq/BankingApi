# Banking Transactions API

[![CI/CD Pipeline](https://github.com/your-org/banking-api/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/your-org/banking-api/actions/workflows/ci-cd.yml)

A production-ready **mock** REST API built with **.NET 9** that exposes bank account movements. Fully compliant with **OpenAPI 3.0**, equipped with a **GitHub Actions** CI/CD pipeline, and deployable to **AWS Elastic Beanstalk**.

---

## Table of Contents

- [Architecture](#architecture)
- [Endpoints](#endpoints)
- [Running locally](#running-locally)
- [Running tests](#running-tests)
- [CI/CD Pipeline](#cicd-pipeline)
- [AWS Deployment](#aws-deployment)
- [GitHub Secrets required](#github-secrets-required)

---

## Architecture

```
BankingApi/
├── .github/
│   └── workflows/
│       └── ci-cd.yml           # GitHub Actions pipeline
├── .ebextensions/
│   └── environment.config      # AWS Elastic Beanstalk config
├── src/
│   └── BankingApi/
│       ├── Controllers/
│       │   ├── AccountsController.cs
│       │   └── TransactionsController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── Models/
│       │   ├── Account.cs
│       │   ├── ErrorResponse.cs
│       │   ├── PagedResponse.cs
│       │   └── Transaction.cs
│       ├── Services/
│       │   ├── ITransactionService.cs
│       │   └── MockTransactionService.cs
│       └── Program.cs
├── tests/
│   └── BankingApi.Tests/
│       ├── Controllers/
│       │   └── TransactionsControllerTests.cs
│       └── Services/
│           └── MockTransactionServiceTests.cs
├── .gitignore
├── BankingApi.sln
└── Procfile
```

---

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/accounts/{accountId}` | Get account details |
| GET | `/api/v1/accounts/{accountId}/transactions` | List transactions (paginated) |
| GET | `/api/v1/accounts/{accountId}/transactions/{transactionId}` | Get single transaction |
| GET | `/health` | Health check (used by Elastic Beanstalk) |
| GET | `/swagger` | Swagger UI |
| GET | `/api-docs/v1/swagger.json` | OpenAPI 3.0 JSON spec |

### Query parameters for `GET /transactions`

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number (≥ 1) |
| `pageSize` | int | 10 | Items per page (1–100) |
| `from` | datetime | — | Filter from date (UTC ISO 8601) |
| `to` | datetime | — | Filter to date (UTC ISO 8601) |
| `type` | string | — | `CREDIT` or `DEBIT` |

### Seed accounts

| ID | Holder | Type |
|----|--------|------|
| ACC-001 | Maria Garcia | Checking |
| ACC-002 | Carlos Mendez | Savings |
| ACC-003 | Ana Torres | Checking |

---

## Running locally

```bash
# Prerequisites: .NET 9 SDK
cd src/BankingApi
dotnet run

# API available at:
#   http://localhost:5000
#   https://localhost:5001
#   http://localhost:5000/swagger  ← Swagger UI
```

---

## Running tests

```bash
dotnet test BankingApi.sln \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --logger "console;verbosity=normal"
```

---

## CI/CD Pipeline

The pipeline (`.github/workflows/ci-cd.yml`) has **5 jobs** that run in order:

```
push / PR
    │
    ├─► 1. BUILD          Restore, build Release, publish artifact
    │         │
    │    ┌────┴──────────────┐
    │    ▼                   ▼
    │  2. TEST           3. STATIC ANALYSIS
    │  (xUnit,           (dotnet-format,
    │  coverage)          Roslyn, SonarCloud)
    │    │                   │
    │    └─────┬─────────────┘
    │          ▼
    │       4. SECURITY
    │       (NuGet audit,
    │        OWASP DC,
    │        Trivy, Gitleaks,
    │        CodeQL)
    │          │
    │   (main branch only)
    │          ▼
    └─► 5. DEPLOY → AWS Elastic Beanstalk
```

| Job | Tools used |
|-----|-----------|
| Build | `dotnet build`, `dotnet publish` |
| Test | xUnit, Coverlet, code coverage summary |
| Static Analysis | `dotnet-format`, Roslyn Analyzers, SonarCloud |
| Security | NuGet audit, OWASP Dependency-Check, Trivy, Gitleaks, CodeQL |
| Deploy | AWS CLI, Elastic Beanstalk |

---

## AWS Deployment

### Prerequisites

1. Create an Elastic Beanstalk application named `banking-api`.
2. Create an environment named `banking-api-production` with platform **.NET on Linux**.
3. Create an S3 bucket for deployment bundles.
4. Create an IAM user with `AWSElasticBeanstalkFullAccess` + `AmazonS3FullAccess`.

### .ebextensions

The `.ebextensions/environment.config` file configures:
- Health check path → `/health`
- Port → `5000`
- Instance type → `t3.small`
- `ASPNETCORE_ENVIRONMENT` → `Production`

---

## GitHub Secrets required

| Secret | Description |
|--------|-------------|
| `AWS_ACCESS_KEY_ID` | IAM access key |
| `AWS_SECRET_ACCESS_KEY` | IAM secret key |
| `EB_S3_BUCKET` | S3 bucket for deployment ZIP files |
| `SONAR_TOKEN` | SonarCloud token *(optional)* |
| `SONAR_ORGANIZATION` | SonarCloud org key *(optional)* |
| `SONAR_PROJECT_KEY` | SonarCloud project key *(optional)* |
