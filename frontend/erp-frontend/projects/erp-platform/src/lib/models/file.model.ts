/**
 * File metadata returned by the backend. Mirrors `StoredFileInfo`.
 */
export interface FileMetadata {
  fileName: string;
  /** Relative handle used to read/delete the file later. */
  relativePath: string;
  sizeBytes: number;
  contentType?: string | null;
  createdAtUtc: string;
}

/** Upload progress event emitted while a file is being uploaded. */
export interface UploadProgress {
  /** 0–100. */
  percent: number;
  loaded: number;
  total: number;
}
