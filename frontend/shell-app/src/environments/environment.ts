// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.

export const environment = {
  production: false,

  // BFF (Backend for Frontend) URL
  // All API calls go through the BFF for secure token management
  bffUrl: 'https://localhost:5101',

  // Legacy URLs - kept for backward compatibility but should use BFF
  apiUrl: 'https://localhost:5101/api',
  identityUrl: 'https://localhost:5001'
};
