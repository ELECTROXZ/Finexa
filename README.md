# Finexa

**Smart Business Accounting Software**

Finexa is a modern Windows desktop ERP and accounting application built for business owners, accounting professionals, and managers. It offers an intuitive, fluent interface to manage sales, purchases, parties (customers and suppliers), inventory, payments, expenses, ledgers, financial reports, and business analytics.

---

## 🛠️ Technology Stack

- **Language & Framework**: C# / .NET 8
- **UI Framework**: WinUI 3 (Windows App SDK)
- **Architecture**: Layered Architecture with MVVM Pattern & Dependency Injection
- **Database / ORM**: Entity Framework Core 8 with SQLite
- **Reporting & Exporting**: QuestPDF (PDF Documents), ClosedXML (Excel Export), QRCoder
- **UI & UX Components**: WinUI 3 Controls, Fluent Design System, CommunityToolkit.Mvvm, CommunityToolkit.WinUI
- **Logging**: Serilog

---

## 📌 Current Status

> **Note**: Finexa is currently under active development. New features, enhancements, and optimizations are regularly being introduced.

### 🌟 Features Implemented

- **User Authentication**: Secure multi-user login system with PBKDF2 salted password hashing.
- **Dashboard & Business Analytics**: Real-time overview of key metrics (sales, expenses, receivables, payables) and low-stock alerts.
- **Party Management**: Comprehensive customer and supplier management including GSTIN/PAN details, contact lists, address books, and party ledgers.
- **Product & Inventory Control**: Product catalog with category, brand, and unit management, HSN/SAC codes, tax rate configuration, and stock threshold tracking.
- **Invoicing & Billing**: Invoice creation with itemized line items, auto-calculated GST/discounts, and printable/exportable invoice documents.
- **Expense & Payment Tracking**: Categorized expense tracking and customer/supplier payment receipt logging.
- **Ledger & Financial Statements**: Party-wise and account-wise ledger transaction logs with running balances.
- **PDF & Excel Reporting**: PDF export for invoices and Excel export capabilities for financial & party data.
- **Dynamic Theming**: Support for Dark, Light, and AMOLED high-contrast themes.

---

## 🚀 Development Setup

### Prerequisites

- [Windows 10 / 11](https://www.microsoft.com/windows) (Build 17763 or later)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with **.NET Desktop Development** and **Windows Application Development** workloads

### Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/YOUR_ORGANIZATION/Finexa.git
   cd Finexa
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run --project Finexa.App
   ```

5. **Database Configuration**:
   The application uses SQLite with Entity Framework Core. On initial startup, EF Core migrations automatically create and seed the SQLite database at `%LOCALAPPDATA%\Finexa\Database\finexa.db`.

---

## 📁 Project Structure

```
Finexa/
├── Finexa.App/            # WinUI 3 Desktop Application (Views, ViewModels, Themes)
├── Finexa.Core/           # Core Abstractions (Interfaces, DTOs, Enums)
├── Finexa.Domain/         # Domain Entities & Business Models
├── Finexa.Infrastructure/ # Data Access (EF Core DbContext, Migrations, Repositories)
├── Finexa.Reporting/      # Reporting Engine (QuestPDF & ClosedXML Services)
├── Finexa.Services/       # Business Logic & Application Services
├── Finexa.Shared/         # Shared Utilities & Helpers (Security, Constants)
├── Finexa.Tests/          # Unit Tests & Automated Test Suites
└── Finexa.sln             # Visual Studio Solution File
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:
1. Create a feature branch before making changes (`git checkout -b feature/your-feature-name`).
2. Keep commits concise, modular, and well-described.
3. Ensure all code builds cleanly and existing unit tests pass (`dotnet test`).
4. Submit a pull request for review.

---

## 📄 License & Intellectual Property

**Copyright © 2026 Electrox Labs / Aryan Singh. All Rights Reserved.**

*Finexa is proprietary commercial software. Licensing terms are currently under specification. Unauthorized copying, distribution, modification, or commercial exploitation of this software or its source code is strictly prohibited.*

---

## 🏢 Developer & Organization Information

- **Organization**: Electrox Labs
- **Lead Developer**: Aryan Singh
- **Contact**: support@electroxlabs.com
