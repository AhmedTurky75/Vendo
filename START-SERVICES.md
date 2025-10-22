# 🚀 Start Services - Quick Reference

## Port Configuration

| Service | Port | Protocol | URL |
|---------|------|----------|-----|
| **Identity Service** | 5001 | HTTPS | https://localhost:5001 |
| **Identity Service** | 5000 | HTTP | http://localhost:5000 |
| **Admin BFF** | 5101 | HTTPS | https://localhost:5101 |
| **Admin Portal (shell-app)** | 4200 | HTTP | http://localhost:4200 |

---

## 🔧 Start All Services (3 Terminals)

### Terminal 1: Identity Service
```bash
cd services/identity/src/Api
dotnet run --launch-profile https
```
✅ Should see: `Now listening on: https://localhost:5001`

### Terminal 2: Admin BFF
```bash
cd services/admin-bff/src/Vendo.AdminBFF
dotnet run
```
✅ Should see: `Now listening on: https://localhost:5101`

### Terminal 3: Admin Portal
```bash
cd frontend/shell-app
npm start
```
✅ Should see: `Angular Live Development Server is listening on localhost:4200`

---

## 🧪 Test the Flow

1. **Open browser**: http://localhost:4200
2. **Should redirect to**: http://localhost:4200/login/admin (or /login/customer)
3. **Click "Sign In"** or enter credentials
4. **Redirects to BFF**: https://localhost:5101/bff/login
5. **BFF redirects to Identity**: https://localhost:5001/connect/authorize
6. **Login with test credentials**
7. **Success!** Redirected back to app with session cookie

---

## 🔍 Verify Services Are Running

### Check Identity Service
```bash
curl https://localhost:5001/.well-known/openid-configuration
```
Should return JSON with OAuth configuration

### Check Admin BFF
```bash
curl https://localhost:5101/health
```
Should return: `{"status":"healthy","service":"Admin BFF","timestamp":"..."}`

### Check Admin Portal
Visit: http://localhost:4200

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
- [ ] No other services running on ports 5000, 5001, 5101, 4200
- [ ] Database/data store ready (if applicable)

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
2. **Second**: Admin BFF (depends on Identity)
3. **Third**: Admin Portal (depends on BFF)

---

## ✅ Success Indicators

When everything is working:
- ✅ No console errors in any terminal
- ✅ Browser can access http://localhost:4200
- ✅ Login redirects work smoothly
- ✅ Session cookie `__Host-admin-bff` appears in DevTools
- ✅ User info loads after login
- ✅ Protected routes accessible

---

**Last Updated**: 2025-10-23
**Status**: Ready for Testing
