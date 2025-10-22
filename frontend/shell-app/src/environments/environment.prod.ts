export const environment = {
  production: true,
  apiUrl: 'https://api.vendo.com/api',
  identityUrl: 'https://identity.vendo.com',

  // OAuth2/OIDC Configuration
  oidc: {
    issuer: 'https://identity.vendo.com',
    clientId: 'spa',
    scope: 'openid profile email roles tenant vendo.api.full_access',
    responseType: 'code',
    requireHttps: true, // MUST be true in production
    showDebugInformation: false
  }
};
