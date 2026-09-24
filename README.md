# SmartInvoice Pro

Production-ready invoice and customer management SaaS for freelancers, small businesses, agencies, and consultants.

**Demo company:** Acme Consulting Pvt Ltd (India) · **Currency:** INR · **Tax:** CGST/SGST + IGST

---

## Live Demo Architecture (Client Presentations)

| Layer | Local | Production (Free-Tier Split) |
|-------|-------|------------------------------|
| **Frontend** | http://localhost:4200 | **Netlify** or **Vercel** (static Angular build) |
| **API** | http://localhost:5000 | **Render** or **Railway** (ASP.NET Core 8) |
| **Database** | SQL Server Express (local) | **Azure SQL** (when going live) |

See [DEPLOYMENT.md](./DEPLOYMENT.md) for step-by-step publish instructions and demo URL setup.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 20 (Standalone Components, Signals, Lazy Loading) |
| Backend | ASP.NET Core 8 Web API |
| Database | SQL Server |
| Auth | JWT + Refresh Token, Role-Based (Admin / Staff) |
| Architecture | Clean Architecture |
| PDF | QuestPDF |
| Excel | ClosedXML |
| Email | Mock (logged to database; swap for SendGrid/SMTP later) |

---

## Repository Structure

```
GHOUSE_MAHAMMED/
├── SmartInvoicePro.Api/          # ASP.NET Core 8 Web API (Clean Architecture)
│   └── src/
│       ├── SmartInvoicePro.Domain/
│       ├── SmartInvoicePro.Application/
│       ├── SmartInvoicePro.Infrastructure/
│       └── SmartInvoicePro.API/
├── SmartInvoicePro.Web/          # Angular 20 SPA
│   └── src/app/
│       ├── core/                 # Services, guards, interceptors
│       ├── shared/               # Reusable UI components
│       ├── layout/               # Shell, sidebar, header
│       └── features/             # Auth, dashboard, CRUD modules
├── SmartInvoicePro.Database/     # SQL scripts, views, stored procedures
│   ├── 01_CreateDatabase.sql
│   ├── 02_CreateTables.sql
│   ├── 03_CreateViews.sql
│   ├── 04_CreateStoredProcedures.sql
│   └── 05_SeedData.sql
├── docs/
│   └── screenshots/              # Portfolio UI screenshots
├── scripts/
│   └── generate_portfolio_pdfs.py
├── README.md
├── DEPLOYMENT.md
├── PORTFOLIO_PITCH.md            # Freelance one-pager
├── PORTFOLIO_PITCH.pdf
├── PORTFOLIO_CASE_STUDY.md       # Full portfolio case study
└── PORTFOLIO_CASE_STUDY.pdf
```

---

## Features

### Authentication
- Register, Login, Forgot/Reset/Change Password
- JWT + refresh tokens
- Roles: **Admin** (full access) · **Staff** (customers, products, invoices, payments only)

### Dashboard
- KPI cards: customers, invoices, paid/pending/overdue, revenue
- Charts: revenue trend, invoice status breakdown, top customers

### Modules
- **Customers** — CRUD, search, pagination, sort, Excel export
- **Products/Services** — CRUD, search, GST tax rates
- **Invoices** — Create/edit, line items, CGST/SGST or IGST, clone, mark paid, PDF, email
- **Payments** — Cash, Bank Transfer, UPI, Credit Card
- **Reports** — Monthly/yearly revenue, customer revenue, outstanding, tax summary (Excel)
- **Company Settings** — Logo, GSTIN, bank details, terms (Admin)
- **Notifications** — Due tomorrow, overdue, payment received
- **Audit Logs** — All major actions (Admin)
- **Demo Reset** — One-click fresh demo data (Admin)

### UI
- Professional green/teal SaaS dashboard
- Dark / light mode
- Responsive (mobile, tablet, desktop)

---

## Quick Start (Local)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) (or full SQL Server)

### 1. Database

**Option A — EF Core migrations (recommended):** The API auto-migrates and seeds on startup.

**Option B — Manual SQL scripts:**
```powershell
cd SmartInvoicePro.Database
sqlcmd -S localhost -E -i 01_CreateDatabase.sql
sqlcmd -S localhost -E -i 02_CreateTables.sql
sqlcmd -S localhost -E -i 03_CreateViews.sql
sqlcmd -S localhost -E -i 04_CreateStoredProcedures.sql
sqlcmd -S localhost -E -i 05_SeedData.sql
```

### 2. API

```powershell
cd SmartInvoicePro.Api
dotnet run --project src/SmartInvoicePro.API
```

- API: http://localhost:5000  
- Swagger: http://localhost:5000/swagger  

Connection string in `appsettings.json` (LocalDB by default).

**JWT:** Development key lives in `appsettings.Development.json`. For production, set env var `Jwt__Key` (32+ chars) — do not commit production secrets.

### 3. Frontend

```powershell
cd SmartInvoicePro.Web
npm install
npm start
```

- App: http://localhost:4200

### Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@acmeconsulting.in | Admin@123 |
| Staff | staff@acmeconsulting.in | Staff@123 |

---

## API Documentation

Swagger UI is available at `/swagger` in Development.

All endpoints return:
```json
{
  "success": true,
  "message": "...",
  "data": { },
  "errors": []
}
```

### Main Endpoints

| Module | Base Route |
|--------|------------|
| Auth | `POST /api/auth/register`, `login`, `refresh`, `forgot-password`, `reset-password`, `change-password`, `GET me` |
| Customers | `GET/POST/PUT/DELETE /api/customers`, `GET export/excel` |
| Products | `GET/POST/PUT/DELETE /api/products` |
| Invoices | `GET/POST/PUT/DELETE /api/invoices`, `GET {id}/pdf`, `POST {id}/send`, `clone`, `mark-paid` |
| Payments | `GET/POST/DELETE /api/payments` |
| Dashboard | `GET /api/dashboard` |
| Reports | `GET /api/reports/*`, Excel exports |
| Settings | `GET/PUT /api/companysettings` |
| Notifications | `GET /api/notifications` |
| Audit Logs | `GET /api/auditlogs` (Admin) |
| Demo | `POST /api/demo/reset` (Admin) |

---

## GST (India)

- **Intra-state (same state as company):** CGST + SGST (default 9% + 9%)
- **Inter-state:** IGST (default 18%)
- Tax type is auto-determined from company state vs customer state
- Override per invoice via `TaxType`: `CGST_SGST` or `IGST`

---

## Portfolio & Freelance Docs

- **[PORTFOLIO_PITCH.md](./PORTFOLIO_PITCH.md)** / **[PORTFOLIO_PITCH.pdf](./PORTFOLIO_PITCH.pdf)** — short client one-pager for proposals  
- **[PORTFOLIO_CASE_STUDY.md](./PORTFOLIO_CASE_STUDY.md)** / **[PORTFOLIO_CASE_STUDY.pdf](./PORTFOLIO_CASE_STUDY.pdf)** — full technical case study with screenshots  
- Screenshots: [`docs/screenshots/`](./docs/screenshots/)

### Client Demo Tips

1. Deploy API to **Render** and frontend to **Netlify** (see DEPLOYMENT.md).
2. Use custom subdomain: `demo.yourdomain.com` + `api.yourdomain.com`.
3. Share **Admin** login for full tour; **Staff** login to show role restrictions.
4. Use **Demo Reset** before each client call for a clean dataset.
5. Show PDF download and mock email log in Swagger or Audit.

---

## License

MIT — suitable for portfolio and client demonstrations.
