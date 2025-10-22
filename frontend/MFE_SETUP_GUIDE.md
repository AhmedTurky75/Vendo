# Vendo Micro-Frontend Setup Guide

## Overview

This guide documents the implementation of 3 new micro-frontend (MFE) applications using Angular and Webpack Module Federation:

- **mfe-admin** (port 4201): Admin dashboard for tenant management
- **mfe-merchant** (port 4202): Merchant store management portal
- **mfe-customer** (port 4203): Customer-facing storefront

## Architecture

### Module Federation Configuration

The shell-app acts as the **host** application that dynamically loads remote MFE modules:

```
shell-app (host) → Port 4200
├── mfe-admin (remote) → Port 4201
├── mfe-merchant (remote) → Port 4202
└── mfe-customer (remote) → Port 4203
```

### Routing Structure

- `/admin/*` → mfe-admin (Admin Dashboard)
- `/merchant/*` → mfe-merchant (Merchant Portal)
- `/store/:domain/*` → mfe-customer (Customer Storefront)
- `/login/*` → shell-app auth module (Login pages)

## Files Created

### mfe-admin (Admin MFE)
```
/home/user/Vendo/frontend/mfe-admin/
├── webpack.config.js                              # Module Federation config
├── tailwind.config.js                             # Tailwind CSS config
├── src/
│   ├── styles.css                                 # Updated with Tailwind
│   └── app/
│       └── admin/
│           ├── admin.module.ts                    # Main admin module
│           ├── admin-routing.module.ts            # Admin routing
│           ├── dashboard/
│           │   ├── dashboard.component.ts
│           │   ├── dashboard.component.html
│           │   └── dashboard.component.css
│           ├── tenants/
│           │   ├── tenants.component.ts
│           │   ├── tenants.component.html
│           │   └── tenants.component.css
│           └── settings/
│               ├── settings.component.ts
│               ├── settings.component.html
│               └── settings.component.css
├── angular.json                                   # Updated for Module Federation
└── package.json                                   # Updated scripts
```

### mfe-merchant (Merchant MFE)
```
/home/user/Vendo/frontend/mfe-merchant/
├── webpack.config.js                              # Module Federation config
├── tailwind.config.js                             # Tailwind CSS config
├── src/
│   ├── styles.css                                 # Updated with Tailwind
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── app/
│       ├── merchant/
│       │   ├── merchant.module.ts                 # Main merchant module
│       │   ├── merchant-routing.module.ts         # Merchant routing
│       │   ├── store-creation/
│       │   │   ├── store-creation.component.ts    # Migrated from shell-app
│       │   │   ├── store-creation.component.html
│       │   │   └── store-creation.component.css
│       │   ├── store-list/
│       │   │   ├── store-list.component.ts        # Migrated from shell-app
│       │   │   ├── store-list.component.html
│       │   │   └── store-list.component.css
│       │   └── store-settings/
│       │       ├── store-settings.component.ts    # Migrated from shell-app
│       │       ├── store-settings.component.html
│       │       └── store-settings.component.css
│       └── core/
│           ├── models/
│           │   └── store.model.ts                 # Migrated from shell-app
│           └── services/
│               ├── store.service.ts               # Migrated from shell-app
│               └── auth.service.ts                # Basic auth service
├── angular.json                                   # Updated for Module Federation
└── package.json                                   # Updated scripts
```

### mfe-customer (Customer MFE)
```
/home/user/Vendo/frontend/mfe-customer/
├── webpack.config.js                              # Module Federation config
├── tailwind.config.js                             # Tailwind CSS config
├── src/
│   ├── styles.css                                 # Updated with Tailwind
│   └── app/
│       └── customer/
│           ├── customer.module.ts                 # Main customer module
│           ├── customer-routing.module.ts         # Customer routing
│           ├── storefront-home/
│           │   ├── storefront-home.component.ts
│           │   ├── storefront-home.component.html
│           │   └── storefront-home.component.css
│           ├── product-detail/
│           │   ├── product-detail.component.ts
│           │   ├── product-detail.component.html
│           │   └── product-detail.component.css
│           └── cart/
│               ├── cart.component.ts
│               ├── cart.component.html
│               └── cart.component.css
├── angular.json                                   # Updated for Module Federation
└── package.json                                   # Updated scripts
```

### shell-app (Host Application)
```
/home/user/Vendo/frontend/shell-app/
├── webpack.config.js                              # NEW: Module Federation host config
├── src/
│   ├── decl.d.ts                                  # NEW: TypeScript declarations for remotes
│   └── app/
│       └── app-routing.module.ts                  # UPDATED: Routes to load MFE remotes
├── angular.json                                   # UPDATED: Module Federation builder
└── package.json                                   # UPDATED: Module Federation dependency
```

## Module Federation Configuration Details

### Remote Configurations (mfe-admin, mfe-merchant, mfe-customer)

Each remote MFE exposes its main module:

**mfe-admin/webpack.config.js:**
```javascript
exposes: {
  "./Module": "./src/app/admin/admin.module.ts"
}
```

**mfe-merchant/webpack.config.js:**
```javascript
exposes: {
  "./Module": "./src/app/merchant/merchant.module.ts"
}
```

**mfe-customer/webpack.config.js:**
```javascript
exposes: {
  "./Module": "./src/app/customer/customer.module.ts"
}
```

### Host Configuration (shell-app)

**shell-app/webpack.config.js:**
```javascript
remotes: {
  mfeAdmin: "http://localhost:4201/remoteEntry.js",
  mfeMerchant: "http://localhost:4202/remoteEntry.js",
  mfeCustomer: "http://localhost:4203/remoteEntry.js"
}
```

### Shared Dependencies

All MFEs share Angular packages as singletons:
- @angular/core
- @angular/common
- @angular/router
- @angular/forms
- RxJS

## How to Run All MFEs

### Option 1: Run Each MFE in Separate Terminals

Terminal 1 - Shell App (Host):
```bash
cd /home/user/Vendo/frontend/shell-app
npm start
# Runs on http://localhost:4200
```

Terminal 2 - Admin MFE:
```bash
cd /home/user/Vendo/frontend/mfe-admin
npm start
# Runs on http://localhost:4201
```

Terminal 3 - Merchant MFE:
```bash
cd /home/user/Vendo/frontend/mfe-merchant
npm start
# Runs on http://localhost:4202
```

Terminal 4 - Customer MFE:
```bash
cd /home/user/Vendo/frontend/mfe-customer
npm start
# Runs on http://localhost:4203
```

### Option 2: Use a Single Script (Recommended)

Create a `start-all.sh` script in `/home/user/Vendo/frontend/`:

```bash
#!/bin/bash

# Start all MFEs in the background
cd /home/user/Vendo/frontend/mfe-admin && npm start &
cd /home/user/Vendo/frontend/mfe-merchant && npm start &
cd /home/user/Vendo/frontend/mfe-customer && npm start &

# Wait for remotes to start
sleep 10

# Start the host app
cd /home/user/Vendo/frontend/shell-app && npm start
```

Make it executable:
```bash
chmod +x /home/user/Vendo/frontend/start-all.sh
```

Run it:
```bash
./start-all.sh
```

## Testing Instructions

### 1. Test Admin MFE

1. Start all MFEs (see above)
2. Navigate to http://localhost:4200/admin
3. You should see the Admin Dashboard with:
   - Stats cards (Total Tenants, Active Merchants, etc.)
   - Recent activity feed
   - Quick action buttons
4. Test navigation:
   - http://localhost:4200/admin/dashboard
   - http://localhost:4200/admin/tenants (tenant management table)
   - http://localhost:4200/admin/settings (platform settings form)

### 2. Test Merchant MFE

1. Navigate to http://localhost:4200/merchant/dashboard
2. You should see the Merchant Dashboard (store list)
3. Test navigation:
   - http://localhost:4200/merchant/onboarding (create new store)
   - http://localhost:4200/merchant/stores/123/settings (store settings)
4. Test merchant features:
   - Store creation form with subdomain validation
   - Store list with cards
   - Store settings with update functionality

### 3. Test Customer MFE

1. Navigate to http://localhost:4200/store/example (replace 'example' with any domain)
2. You should see the Customer Storefront with:
   - Hero section
   - Featured products
   - Product grid with filtering
   - Footer
3. Test navigation:
   - Browse products
   - Click on product categories
   - View product details (when implemented)
   - View cart (when implemented)

### 4. Test Module Federation

1. Open browser DevTools → Network tab
2. Navigate to http://localhost:4200/admin
3. You should see:
   - Request to `http://localhost:4201/remoteEntry.js` (Admin MFE)
   - Chunk files being loaded from port 4201
4. Navigate to http://localhost:4200/merchant
5. You should see:
   - Request to `http://localhost:4202/remoteEntry.js` (Merchant MFE)
   - Chunk files being loaded from port 4202
6. Navigate to http://localhost:4200/store/test
7. You should see:
   - Request to `http://localhost:4203/remoteEntry.js` (Customer MFE)
   - Chunk files being loaded from port 4203

## Migration Notes: Merchant Features

### What Was Moved

The following files were migrated from `shell-app` to `mfe-merchant`:

**Components:**
- `features/merchant/store-creation/*` → `mfe-merchant/src/app/merchant/store-creation/*`
- `features/merchant/store-list/*` → `mfe-merchant/src/app/merchant/store-list/*`
- `features/merchant/store-settings/*` → `mfe-merchant/src/app/merchant/store-settings/*`

**Services & Models:**
- `core/services/store.service.ts` → `mfe-merchant/src/app/core/services/store.service.ts`
- `core/models/store.model.ts` → `mfe-merchant/src/app/core/models/store.model.ts`

**New Files Created:**
- `mfe-merchant/src/app/core/services/auth.service.ts` (basic implementation for merchant MFE)
- `mfe-merchant/src/environments/environment.ts` (environment configuration)

### Breaking Changes

1. **Import Paths Changed:**
   - Old: `import { StoreService } from '../../../core/services/store.service'`
   - New: Still valid in mfe-merchant

2. **Routing Changes:**
   - Old: Shell-app loaded merchant module directly
   - New: Shell-app loads merchant module via Module Federation
   - Routes remain the same: `/merchant/*`

3. **Module Federation Boundary:**
   - Merchant features are now completely isolated in mfe-merchant
   - Shared dependencies are provided via Module Federation
   - Auth service is duplicated (basic implementation) in mfe-merchant

### Post-Migration Steps

1. **Remove old merchant module from shell-app (Optional):**
   ```bash
   # Backup first!
   mv /home/user/Vendo/frontend/shell-app/src/app/features/merchant \
      /home/user/Vendo/frontend/shell-app/src/app/features/merchant.backup
   ```

2. **Update shared services:**
   - Consider moving shared services (like AuthService) to a shared library
   - Use `shared-lib` project for truly shared code

3. **Environment configuration:**
   - Update API URLs in environment files for each MFE
   - Ensure consistent configuration across MFEs

## Troubleshooting

### Issue: "Cannot find module 'mfeAdmin/Module'"

**Solution:** Ensure the TypeScript declaration file exists at:
`/home/user/Vendo/frontend/shell-app/src/decl.d.ts`

### Issue: "remoteEntry.js failed to load"

**Solution:**
1. Ensure the remote MFE is running on the correct port
2. Check the webpack.config.js remote URL
3. Verify port is not being used by another process

### Issue: "Shared module version mismatch"

**Solution:**
1. Ensure all MFEs use the same Angular version
2. Check package.json in each MFE
3. Run `npm install` in each MFE to sync versions

### Issue: "Tailwind styles not working"

**Solution:**
1. Verify tailwind.config.js exists
2. Check styles.css has `@import 'tailwindcss';`
3. Restart the dev server

## Production Deployment

### Build Commands

Build all MFEs for production:

```bash
# Admin MFE
cd /home/user/Vendo/frontend/mfe-admin
npm run build:prod

# Merchant MFE
cd /home/user/Vendo/frontend/mfe-merchant
npm run build:prod

# Customer MFE
cd /home/user/Vendo/frontend/mfe-customer
npm run build:prod

# Shell App
cd /home/user/Vendo/frontend/shell-app
npm run build
```

### Production Configuration

Update webpack.config.js in shell-app for production:

```javascript
remotes: {
  mfeAdmin: "https://admin.vendo.com/remoteEntry.js",
  mfeMerchant: "https://merchant.vendo.com/remoteEntry.js",
  mfeCustomer: "https://customer.vendo.com/remoteEntry.js"
}
```

## Next Steps

1. **Add Authentication Guards:**
   - Implement route guards for admin/merchant routes
   - Add role-based access control

2. **Implement State Management:**
   - Consider NgRx or Akita for shared state
   - Implement cross-MFE communication if needed

3. **Add Error Boundaries:**
   - Implement error handling for remote module loading failures
   - Add fallback UI for MFE load errors

4. **Performance Optimization:**
   - Implement lazy loading for components within MFEs
   - Add caching strategies for remoteEntry.js

5. **Testing:**
   - Add unit tests for each MFE
   - Add E2E tests for critical user flows
   - Test MFE independence (each should run standalone)

6. **Documentation:**
   - Document component APIs
   - Create developer onboarding guide
   - Add architectural decision records (ADRs)

## Resources

- [Angular Module Federation Documentation](https://www.angulararchitects.io/en/aktuelles/the-microfrontend-revolution-module-federation-in-webpack-5/)
- [Module Federation GitHub](https://github.com/angular-architects/module-federation-plugin)
- [Webpack Module Federation](https://webpack.js.org/concepts/module-federation/)

## Summary

All three micro-frontends have been successfully created and configured with:
- ✅ NgModule-based architecture (not standalone)
- ✅ Webpack Module Federation
- ✅ Tailwind CSS for styling
- ✅ Proper routing configuration
- ✅ TypeScript declarations for remotes
- ✅ Merchant features migrated from shell-app
- ✅ Independent deployment capability

The system is now ready for development and testing!
