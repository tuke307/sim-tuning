export type PageNumberPagination = {
  isFirstPage: boolean;
  isLastPage: boolean;
  currentPage: number;
  previousPage: number | null;
  nextPage: number | null;
};
export type PageNumberCounters = {
  pageCount: number;
  totalCount: number;
};

export type PageNumberPaginationMeta = PageNumberPagination & PageNumberCounters;

export interface ApiResponse<T> {
  /**
   * @description
   * If the request was successful, this will be `true`.
   * If the request was unsuccessful, this will be `false`.
   */
  success: boolean;

  /**
   * @description
   * If the request was successful, this will be the data returned by the request.
   * If the request was unsuccessful, this will be `null`.
   */
  data: T | null;

  /**
   * @description
   * If the request was successful, this will be `null`.
   * If the request was unsuccessful, this will be a string describing the error.
   */
  error: string | null;

  /**
   * @description
   * In non‐production, may carry additional diagnostics (e.g. `stack` or validation errors).
   */
  details?: unknown;

  /**
   * @description
   * If the request was successful and the response includes pagination, this will be the pagination meta
   * information.
   */
  pagination?: PageNumberPaginationMeta;
}
