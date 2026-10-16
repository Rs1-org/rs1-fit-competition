export type SortDir = 'asc' | 'desc';

/** Backend column sorting (RS1 item 13): sent as `sortBy` / `sortDir` query params. */
export interface SortParams {
  sortBy?: string;
  sortDir?: SortDir;
}

export type ExportFormat = 'csv' | 'xlsx' | 'json';

export interface ImportError {
  /** Spreadsheet row number; the header is row 1. */
  row: number;
  column: string;
  message: string;
}

/** Import is all-or-nothing: when `errors` is not empty, nothing was saved. */
export interface ImportResult {
  imported: number;
  totalRows: number;
  errors: ImportError[];
  success: boolean;
}
