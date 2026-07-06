import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface FileValidatorOptions {
  /** Maximum size per file in megabytes. */
  maxSizeMb?: number;
  /** Allowed extensions without the dot, e.g. ['pdf', 'xlsx']. Case-insensitive. */
  allowedExtensions?: string[];
  /** Maximum number of files (for multi-file controls). */
  maxFiles?: number;
}

/**
 * Validates a control holding a `File`, `FileList` or `File[]` against size, extension and
 * count rules. Empty values pass. Error key: `file` with the violated sub-rules:
 * `{ file: { maxSize?, extension?, maxFiles? } }`.
 *
 * Usage: `new FormControl(null, [fileValidator({ maxSizeMb: 5, allowedExtensions: ['pdf'] })])`
 */
export function fileValidator(options: FileValidatorOptions = {}): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const files = toFileArray(control.value);
    if (files.length === 0) {
      return null;
    }

    const errors: Record<string, unknown> = {};

    if (options.maxFiles !== undefined && files.length > options.maxFiles) {
      errors['maxFiles'] = { max: options.maxFiles, actual: files.length };
    }

    if (options.maxSizeMb !== undefined) {
      const maxBytes = options.maxSizeMb * 1024 * 1024;
      const tooLarge = files.filter((f) => f.size > maxBytes).map((f) => f.name);
      if (tooLarge.length > 0) {
        errors['maxSize'] = { maxSizeMb: options.maxSizeMb, files: tooLarge };
      }
    }

    if (options.allowedExtensions?.length) {
      const allowed = options.allowedExtensions.map((e) => e.toLowerCase().replace(/^\./, ''));
      const invalid = files
        .filter((f) => !allowed.includes(extensionOf(f.name)))
        .map((f) => f.name);
      if (invalid.length > 0) {
        errors['extension'] = { allowed, files: invalid };
      }
    }

    return Object.keys(errors).length > 0 ? { file: errors } : null;
  };
}

function toFileArray(value: unknown): File[] {
  if (!value) return [];
  if (value instanceof File) return [value];
  if (value instanceof FileList) return Array.from(value);
  if (Array.isArray(value)) return value.filter((v): v is File => v instanceof File);
  return [];
}

function extensionOf(fileName: string): string {
  const dot = fileName.lastIndexOf('.');
  return dot === -1 ? '' : fileName.slice(dot + 1).toLowerCase();
}
