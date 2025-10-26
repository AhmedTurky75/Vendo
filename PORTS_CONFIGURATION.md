# Vendo Project - Ports Configuration

This document lists all the default ports used by each service in the Vendo project.

## Backend Services (ASP.NET Core)

| Service | HTTP Port | HTTPS Port | Description |
|---------|-----------|------------|-------------|
| **Identity Server** | 5000 | 5001 | Authentication and authorization service |
| **Admin BFF** | 5083 | 7083 | Backend for Frontend - Admin portal |
| **Catalog API** | 5213 | 7053 | Product catalog management service |
| **Order API** | 5049 | 7215 | Order processing and management service |
| **Payment API** | 5078 | 7251 | Payment processing service |
| **Tenant Management API** | 5120 | 7028 | Multi-tenant store management service |

## Frontend Services (Angular Micro-Frontends)

| Service | Port | Description |
|---------|------|-------------|
| **Shell App** | 4200 | Main application container/host |
| **MFE Admin** | 4201 | Admin portal micro-frontend |
| **MFE Products** | 4202 | Products management micro-frontend |
| **MFE Orders** | 4203 | Orders management micro-frontend |
| **MFE Store** | 4204 | Store configuration micro-frontend |
| **MFE Customer** | 4205 | Customer-facing storefront micro-frontend |
| **MFE Merchant** | 4206 | Merchant dashboard micro-frontend |

## Configuration Files

### Backend Services
- Port configurations are located in: `services/[service-name]/src/Api/Properties/launchSettings.json`
- Application settings in: `services/[service-name]/src/Api/appsettings.json`

### Frontend Services
- Port configurations are located in: `frontend/[mfe-name]/angular.json` (under `architect.serve.options.port`)
- Module federation settings in: `frontend/[mfe-name]/webpack.config.js`

## Quick Start URLs

### Backend Services
- Identity Server: http://localhost:5000
- Admin BFF: http://localhost:5083
- Catalog API: http://localhost:5213
- Order API: http://localhost:5049
- Payment API: http://localhost:5078
- Tenant Management API: http://localhost:5120

### Frontend Services
- Shell App: http://localhost:4200
- Admin Portal: http://localhost:4201
- Products MFE: http://localhost:4202
- Orders MFE: http://localhost:4203
- Store MFE: http://localhost:4204
- Customer MFE: http://localhost:4205
- Merchant MFE: http://localhost:4206

## Notes

1. All backend services support both HTTP and HTTPS protocols
2. HTTPS is recommended for production environments
3. Frontend micro-frontends are loaded into the Shell App via Module Federation
4. The Shell App (port 4200) is the main entry point for the application
5. Ensure no port conflicts exist before starting all services
