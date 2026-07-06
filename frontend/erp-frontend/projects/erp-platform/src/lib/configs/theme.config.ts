export type AppTheme = 'light' | 'dark';

/**
 * UI theme configuration consumed by a theme service. Centralizes the available themes, the
 * default and the storage key used to persist the user's choice.
 */
export const THEME_CONFIG = {
  defaultTheme: 'light' as AppTheme,
  themes: ['light', 'dark'] as AppTheme[],
  storageKey: 'erp.theme',
  /** CSS class applied to the document body per theme. */
  bodyClassPrefix: 'theme-',
} as const;
