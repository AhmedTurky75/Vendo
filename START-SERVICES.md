# 🚀 Start Services - Quick Reference

## Port Configuration

| Service | Port | Protocol | URL |
|---------|------|----------|-----|
| **Identity Service** | 5001 | HTTPS | https://localhost:5001 |
| **Tenant Management Service** | 5005 | HTTPS | https://localhost:5005 |
| **Admin BFF** | 5101 | HTTPS | https://localhost:5101 |
| **Merchant BFF** | 5102 | HTTPS | https://localhost:5102 |
| **Shell App (Main Portal)** | 4200 | HTTP | http://localhost:4200 |
| **Merchant MFE** | 4206 | HTTP | http://localhost:4206 |

---

## 🔧 Start All Services

### Terminal 1: Identity Service
```bash
cd services/identity/src/Api
dotnet run --launch-profile https
```
✅ Should see: `Now listening on: https://localhost:5001`

### Terminal 2: Tenant Management Service
```bash
cd services/tenant-management/src/Api
dotnet run
```
✅ Should see: `Now listening on: https://localhost:5005`

### Terminal 3: Admin BFF
```bash
cd services/admin-bff/src/Vendo.AdminBFF
dotnet run
```
✅ Should see: `Now listening on: https://localhost:5101`

### Terminal 4: Merchant BFF
```bash
cd services/merchant-bff/src/Vendo.MerchantBFF
dotnet run
```
✅ Should see: `Now listening on: https://localhost:5102`

### Terminal 5: Shell App
```bash
cd frontend/shell-app
npm start
```
✅ Should see: `Angular Live Development Server is listening on localhost:4200`

### Terminal 6: Merchant MFE
```bash
cd frontend/mfe-merchant
npm start
```
✅ Should see: `Angular Live Development Server is listening on localhost:4206`

---

## 🧪 Test the Flows

### Admin Flow
1. **Open browser**: http://localhost:4200
2. **Navigate to**: http://localhost:4200/admin
3. **Redirects to BFF**: https://localhost:5101/bff/login
4. **BFF redirects to Identity**: https://localhost:5001/connect/authorize
5. **Login with admin credentials**: admin@vendo.com / Admin123!
6. **Success!** Redirected back to admin portal with session cookie `__Host-admin-bff`

### Merchant Flow (NEW!)
1. **Open browser**: http://localhost:4200/merchant
2. **Redirects to BFF**: https://localhost:5102/bff/login
3. **BFF redirects to Identity**: https://localhost:5001/connect/authorize
4. **Login with merchant credentials**: merchant@vendo.com / Merchant123!
5. **First-time merchant**: Redirected to `/merchant/onboarding` to create store
6. **Returning merchant**: Redirected to `/merchant/dashboard` with store list
7. **Success!** Session cookie `__Host-merchant-bff` created

---

## 🔍 Verify Services Are Running

### Check Identity Service
```bash
curl https://localhost:5001/.well-known/openid-configuration
```
Should return JSON with OAuth configuration

### Check Tenant Management Service
```bash
curl https://localhost:5005/health
```
Should return: `{"status":"healthy","timestamp":"..."}`

### Check Admin BFF
```bash
curl https://localhost:5101/health
```
Should return: `{"status":"healthy","service":"Admin BFF","timestamp":"..."}`

### Check Merchant BFF
```bash
curl https://localhost:5102/health
```
Should return: `{"status":"healthy","service":"Merchant BFF","timestamp":"..."}`

### Check Shell App
Visit: http://localhost:4200

### Check Merchant MFE
Visit: http://localhost:4206

---

## ⚠️ Troubleshooting

### Error: "Connection refused" on port 5001
**Cause**: Identity Service not running or wrong port
**Fix**:
1. Check launchSettings.json: `services/identity/src/Api/Properties/launchSettings.json`
2. Should have: `"applicationUrl": "https://localhost:5001;http://localhost:5000"`
3. Run: `cd services/identity/src/Api && dotnet run --launch-profile https`

### Error: "Connection refused" on port 5101
**Cause**: Admin BFF not running
**Fix**: `cd services/admin-bff/src/Vendo.AdminBFF && dotnet run`

### Error: CORS error in browser console
**Cause**: Port mismatch or service not running
**Fix**:
1. Verify all services are running on correct ports
2. Check CORS in Admin BFF Program.cs allows port 4200
3. Check browser is accessing http://localhost:4200 (not 4300)

### Error: Certificate errors (HTTPS)
**Fix**: Trust development certificates
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Error: Cookie not set after login
**Cause**: HTTPS/HTTP mismatch or cookie settings
**Fix**:
1. Ensure BFF runs on HTTPS (5101)
2. Check cookie settings in BFF Program.cs
3. Verify `withCredentials: true` in Angular

---

## 📋 Pre-Flight Checklist

Before starting services:
- [ ] All code built successfully
- [ ] Development certificates trusted
- [ ] No other services running on ports: 5001, 5005, 5101, 5102, 4200, 4206
- [ ] Database/data store ready (if applicable)
- [ ] Run `dotnet dev-certs https --trust` if you haven't already

---

## 🔐 Test Credentials

Use these seeded test users:

### Admin User
- **Email**: admin@vendo.com
- **Password**: Admin123!
- **Role**: Admin

### Merchant User
- **Email**: merchant@vendo.com
- **Password**: Merchant123!
- **Role**: Merchant

### Customer User
- **Email**: customer@vendo.com
- **Password**: Customer123!
- **Role**: Customer

*(Check your SeedData.cs for actual credentials)*

---

## 🛑 Stop All Services

### Windows (PowerShell)
```powershell
# Find and kill processes
Get-Process | Where-Object {$_.ProcessName -like "dotnet"} | Stop-Process
Get-Process | Where-Object {$_.ProcessName -like "node"} | Stop-Process
```

### Mac/Linux
```bash
# Kill dotnet processes
pkill -f dotnet

# Kill node processes
pkill -f node
```

Or simply press `Ctrl+C` in each terminal window.

---

## 📊 Service Startup Order (Recommended)

1. **First**: Identity Service (other services depend on it)
2. **Second**: Tenant Management Service (provides store data)
3. **Third**: Admin BFF & Merchant BFF (depend on Identity and services)
4. **Fourth**: Shell App (depends on BFFs)
5. **Fifth**: Merchant MFE (loaded by Shell App)

---

## ✅ Success Indicators

When everything is working:
- ✅ No console errors in any terminal
- ✅ Browser can access http://localhost:4200
- ✅ Admin login: Session cookie `__Host-admin-bff` appears in DevTools
- ✅ Merchant login: Session cookie `__Host-merchant-bff` appears in DevTools
- ✅ Login redirects work smoothly
- ✅ User info loads after login
- ✅ Protected routes accessible
- ✅ Merchant can create stores via `/merchant/onboarding`
- ✅ Store creation API calls succeed to Tenant Management Service

---

**Last Updated**: 2025-10-24
**Status**: Ready for Testing - Merchant Onboarding Flow Complete
