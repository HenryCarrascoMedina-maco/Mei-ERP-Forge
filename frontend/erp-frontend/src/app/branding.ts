/**
 * ════════════════════════════════════════════════════════════════════════════
 *  Branding — single source of truth for the app's user-facing identity.
 * ────────────────────────────────────────────────────────────────────────────
 *  To rebrand the product, edit the values below (and swap `public/favicon.ico`
 *  + the theme accent in `styles.scss`). See docs/REBRANDING.md for the full
 *  checklist. The framework package names (`@erp-platform/*`, `ErpPlatform.*`)
 *  are a separate, OPTIONAL rename covered in that guide.
 * ════════════════════════════════════════════════════════════════════════════
 */
export const BRANDING = {
  /** Product name — shown in the top toolbar, the login screen and the browser tab. */
  appName: 'ERP Template',

  /** Short name — compact contexts (PWA / tight layouts). */
  shortName: 'ERP',

  /** One-line description — the login subtitle. */
  description: 'Reusable ERP starter kit',
} as const;
