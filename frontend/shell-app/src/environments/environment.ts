// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.

export const environment = {
  production: false,
  apiUrl: 'http://localhost:5001/api',
  identityUrl: 'http://localhost:5001',

  // OAuth2/OIDC Configuration
  oidc: {
    issuer: 'http://localhost:5001',
    clientId: 'spa',
    scope: 'openid profile email roles tenant vendo.api.full_access',
    responseType: 'code',
    requireHttps: false, // Only false in development
    showDebugInformation: true
  }
};
