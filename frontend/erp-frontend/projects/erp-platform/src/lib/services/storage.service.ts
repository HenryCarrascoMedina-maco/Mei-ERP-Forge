import { Injectable } from '@angular/core';

export type StorageType = 'local' | 'session';

/**
 * Centralized, type-safe wrapper around localStorage / sessionStorage.
 * Values are JSON serialized; failures are swallowed so a corrupt entry never breaks the app.
 */
@Injectable({ providedIn: 'root' })
export class StorageService {
  private store(type: StorageType): Storage {
    return type === 'session' ? sessionStorage : localStorage;
  }

  set<T>(key: string, value: T, type: StorageType = 'local'): void {
    try {
      this.store(type).setItem(key, JSON.stringify(value));
    } catch {
      // Ignore quota / serialization errors.
    }
  }

  get<T>(key: string, type: StorageType = 'local'): T | null {
    try {
      const raw = this.store(type).getItem(key);
      return raw === null ? null : (JSON.parse(raw) as T);
    } catch {
      return null;
    }
  }

  remove(key: string, type: StorageType = 'local'): void {
    this.store(type).removeItem(key);
  }

  clear(type: StorageType = 'local'): void {
    this.store(type).clear();
  }
}
