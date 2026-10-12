// apiUrl is empty: the app is served behind nginx, which proxies /api to the backend (same origin).
export const environment = {
  production: true,
  apiUrl: '',
  stripePublishableKey: 'pk_test_placeholder',
};
