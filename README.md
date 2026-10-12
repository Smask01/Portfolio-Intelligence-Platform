# Portfolio Intelligence Platform

A full-stack web app for analyzing stock and ETF portfolios. It breaks down ETF holdings to show actual stock exposure, sector allocation, and overlap between ETFs.

**Live Demo:** https://portfolio-intelligence-platform-i5h5.onrender.com

## Features

- Analyze portfolios with stocks and ETFs
- View underlying exposure from ETF holdings
- View sector allocation
- Compare overlap between ETFs
- Interactive charts for portfolio results
- External financial data integration

## Tech Stack

**Backend:** C#, .NET 10, ASP.NET Core, NUnit  
**Frontend:** React, TypeScript, Vite, Recharts  
**Other:** Docker, Docker Compose, GitHub Actions

## Architecture
```text
React + TypeScript
        │
        │ REST
        ▼
ASP.NET Core API
        │
        ▼
Application Layer
        │
   ┌────┴────┐
   ▼         ▼
Portfolio   ETF Overlap
Analyzer    Calculator
   │
   ▼
Infrastructure
   │
   ▼
Financial Data API
```

## Running Locally

You'll need Docker Desktop and an Alpha Vantage API key.

Create a `.env` file in the project root:

ALPHA_VANTAGE_API_KEY=your_api_key

Then run:

docker compose up --build

Frontend: http://localhost:3000  
API: http://localhost:8080
