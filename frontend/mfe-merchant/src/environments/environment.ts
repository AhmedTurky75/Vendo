export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  bffUrl: 'https://localhost:5102',
  auth: {
    authority: 'https://localhost:5001',
    clientId: 'merchant-bff',
    redirectUri: window.location.origin + '/auth/callback',
    postLogoutRedirectUri: window.location.origin + '/auth/signout-callback',
    responseType: 'code',
    scope: 'openid profile email roles tenant vendo.api.full_access offline_access',
    usePkce: true
  }
};
