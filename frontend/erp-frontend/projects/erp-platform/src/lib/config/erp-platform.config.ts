import { InjectionToken, Provider } from '@angular/core';

/**
 * Runtime configuration the host application supplies to the ERP Platform library.
 * Decouples the library from any app-specific `environment` file.
 */
export interface ErpPlatformConfig {
  /** Base URL of the backend API, e.g. `https://host/api`. */
  apiUrl: string;
}

export const ERP_PLATFORM_CONFIG = new InjectionToken<ErpPlatformConfig>('ERP_PLATFORM_CONFIG');

/**
 * Registers the ERP Platform configuration. Call in the host's `app.config.ts`:
 * `provideErpPlatform({ apiUrl: environment.apiUrl })`.
 */
export function provideErpPlatform(config: ErpPlatformConfig): Provider {
  return { provide: ERP_PLATFORM_CONFIG, useValue: config };
}
