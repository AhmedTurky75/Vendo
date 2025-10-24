export const environment = {
  production: true,
  apiUrl: 'https://api.vendo.com/api',
  bffUrl: 'https://merchant-bff.vendo.com',
  auth: {
    authority: 'https://identity.vendo.com',
    clientId: 'merchant-bff',
    redirectUri: window.location.origin + '/auth/callback',
    postLogoutRedirectUri: window.location.origin + '/auth/signout-callback',
    responseType: 'code',
    scope: 'openid profile email roles tenant vendo.api.full_access offline_access',
    usePkce: true
  }
};
