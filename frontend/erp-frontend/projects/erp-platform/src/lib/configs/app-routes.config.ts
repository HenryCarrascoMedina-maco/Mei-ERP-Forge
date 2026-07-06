/**
 * Centralized route path constants. Reference these instead of hardcoding strings so route
 * changes happen in one place. Guards default to `login` / `forbidden`.
 */
export const APP_ROUTES = {
  home: '/',
  login: '/login',
  forbidden: '/forbidden',
  notFound: '/404',
  users: '/users',
} as const;

export type AppRouteKey = keyof typeof APP_ROUTES;
