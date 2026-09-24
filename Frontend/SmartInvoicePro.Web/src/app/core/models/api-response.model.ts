export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string | null;
  data: T;
  errors: string[] | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
