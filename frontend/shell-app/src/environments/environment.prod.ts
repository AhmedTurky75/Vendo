export const environment = {
  production: true,

  // BFF (Backend for Frontend) URL
  // All API calls go through the BFF for secure token management
  bffUrl: 'https://admin-bff.vendo.com',

  // Legacy URLs - kept for backward compatibility but should use BFF
  apiUrl: 'https://admin-bff.vendo.com/api',
  identityUrl: 'https://identity.vendo.com'
};
