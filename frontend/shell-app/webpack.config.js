const {
  shareAll,
  withModuleFederationPlugin,
} = require("@angular-architects/module-federation/webpack");

module.exports = withModuleFederationPlugin({
  remotes: {
    mfeAdmin: "http://localhost:4201/remoteEntry.js",
    mfeCustomer: "http://localhost:4203/remoteEntry.js",
    mfeMerchant: "http://localhost:4206/remoteEntry.js",
    mfeProducts: "http://localhost:4207/remoteEntry.js",
    mfeOrders: "http://localhost:4208/remoteEntry.js",
    mfeStore: "http://localhost:4209/remoteEntry.js",
  },

  shared: {
    ...shareAll({
      singleton: true,
      strictVersion: true,
      requiredVersion: "auto",
    }),
  },
});
