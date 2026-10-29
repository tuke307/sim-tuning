'use client';

import * as React from 'react';
import {
  ColumnDef,
  ColumnFiltersState,
  OnChangeFn,
  PaginationState,
  Row,
  RowData,
  SortingState,
  flexRender,
  useTable,
  tableFeatures,
  columnFilteringFeature,
  columnFacetingFeature,
  columnSizingFeature,
  rowPaginationFeature,
  rowSortingFeature,
  columnVisibilityFeature,
  rowSelectionFeature,
  createFilteredRowModel,
  createSortedRowModel,
  createPaginatedRowModel,
  createFacetedRowModel,
  createFacetedUniqueValues,
} from '@tanstack/react-table';

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@sim-tuning/ui/components/ui/table';

import { ScrollArea, ScrollBar } from '@sim-tuning/ui/components/ui/scroll-area';
import { DataTablePagination } from '@sim-tuning/ui/components/data-table/data-table-pagination';
import { DataTableSearchToolbar } from '@sim-tuning/ui/components/data-table/data-table-search-toolbar';
import { DateRange } from 'react-day-picker';

interface DataTableProps<TData extends RowData> {
  columns: ColumnDef<typeof features, TData, unknown>[];
  data: TData[];
  headerStyle?: React.CSSProperties;
  enablePagination?: boolean;
  enableFiltering?: boolean;
  enableRowActions?: boolean;
  totalCount?: number;
  enableSearch?: boolean;
  searchTerm?: string;
  searchPlaceholder?: string;
  enableSorting?: boolean;
  dateRange?: DateRange;
  isLoading?: boolean;
  showSelectionLabel?: boolean;
  onDateChange?: (dateRange?: DateRange) => void;
  onSortingChange?: (sorting: SortingState) => void;
  onSearchChange?: (searchTerm: string) => void;
  onPaginationChange?: (pagination: { page: number; pageSize: number }) => void;
  onFilterChange?: (filters: ColumnFiltersState) => void;
  rowActions?: (row: Row<typeof features, TData>) => React.ReactNode;
  initialSorting?: SortingState;
}

// Define table features for TanStack Table v9
export const features = tableFeatures({
  columnFilteringFeature,
  columnFacetingFeature,
  columnSizingFeature,
  rowPaginationFeature,
  rowSortingFeature,
  columnVisibilityFeature,
  rowSelectionFeature,
  filteredRowModel: createFilteredRowModel(),
  sortedRowModel: createSortedRowModel(),
  paginatedRowModel: createPaginatedRowModel(),
  facetedRowModel: createFacetedRowModel(),
  facetedUniqueValues: createFacetedUniqueValues(),
});

export function DataTable<TData extends RowData>({
  columns,
  data,
  headerStyle,
  enablePagination = false,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  enableFiltering = false,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  enableRowActions = false,
  totalCount = 0,
  enableSearch = false,
  searchTerm = '',
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  enableSorting = false,
  searchPlaceholder = 'Suchen...',
  dateRange,
  isLoading = false,
  showSelectionLabel = false,
  initialSorting,
  onDateChange,
  onSortingChange,
  onSearchChange = () => {},
  onPaginationChange,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  onFilterChange,
  rowActions,
}: DataTableProps<TData>) {
  const [rowSelection, setRowSelection] = React.useState({});
  const [columnVisibility, setColumnVisibility] = React.useState<Record<string, boolean>>({});
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
  const [sorting, setSorting] = React.useState<SortingState>(initialSorting || []);
  const [pagination, setPagination] = React.useState<PaginationState>({
    pageIndex: 0,
    pageSize: 10,
  });

  const handleSortingChange: OnChangeFn<SortingState> = (updater) => {
    const newSorting = updater instanceof Function ? updater(sorting) : updater;
    setSorting(newSorting);
    onSortingChange?.(newSorting);
  };

  const handlePaginationChange = React.useCallback(
    (updater: React.SetStateAction<PaginationState>) => {
      const newPagination = updater instanceof Function ? updater(pagination) : updater;
      setPagination(newPagination);

      if (onPaginationChange) {
        onPaginationChange({
          page: newPagination.pageIndex + 1, // Convert 0-based to 1-based
          pageSize: newPagination.pageSize,
        });
      }
    },
    [pagination, onPaginationChange],
  );

  const finalColumns = React.useMemo(() => {
    if (!rowActions) return columns; // use existing columns if no custom rowActions
    return [
      // row actions at front
      {
        id: 'actions',
        header: '',
        cell: ({ row }: { row: Row<typeof features, TData> }) => rowActions(row),
      },
      ...columns,
    ];
  }, [columns, rowActions]);

  // TanStack Table v9 - features defined globally for tree-shaking
  const table = useTable({
    features,
    data,
    columns: finalColumns,
    state: {
      sorting,
      columnVisibility,
      rowSelection,
      columnFilters,
      pagination,
    },
    pageCount: totalCount ? Math.ceil(totalCount / pagination.pageSize) : -1,
    enableRowSelection: true,
    onRowSelectionChange: setRowSelection,
    manualSorting: !!onSortingChange,
    manualPagination: !!onPaginationChange,
    onSortingChange: handleSortingChange,
    onPaginationChange: handlePaginationChange,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
  });

  return (
    <div className="space-y-4">
      {enableSearch && (
        <DataTableSearchToolbar
          searchTerm={searchTerm}
          onSearchChange={onSearchChange}
          dateRange={dateRange}
          onDateChange={onDateChange}
          placeholder={searchPlaceholder}
        />
      )}
      <div className="rounded-md border">
        {isLoading ? (
          <div className="p-4 text-center">Laden...</div>
        ) : (
          <ScrollArea className="w-full">
            {/* fixed layout, no auto widths */}
            <Table className="w-full table-fixed">
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => {
                      const columnSize = header.column.getSize();
                      const defSize = typeof columnSize === 'number' ? columnSize : undefined;

                      // strict pixels when provided
                      const sizeStyle =
                        defSize != null
                          ? {
                              width: `${defSize}px`,
                              minWidth: `${defSize}px`,
                              maxWidth: `${defSize}px`,
                            }
                          : undefined;
                      const style = headerStyle
                        ? {
                            ...sizeStyle,
                            ...headerStyle,
                          }
                        : sizeStyle;

                      return (
                        <TableHead key={header.id} colSpan={header.colSpan} style={style}>
                          {header.isPlaceholder ? null : (
                            <div className="min-w-0 overflow-hidden text-ellipsis whitespace-nowrap">
                              {flexRender(header.column.columnDef.header, header.getContext())}
                            </div>
                          )}
                        </TableHead>
                      );
                    })}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows?.length ? (
                  table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id} data-state={row.getIsSelected() && 'selected'}>
                      {row.getVisibleCells().map((cell) => {
                        const columnSize = cell.column.getSize();
                        const defSize = typeof columnSize === 'number' ? columnSize : undefined;

                        const style =
                          defSize != null
                            ? {
                                width: `${defSize}px`,
                                minWidth: `${defSize}px`,
                                maxWidth: `${defSize}px`,
                              }
                            : undefined;

                        return (
                          <TableCell key={cell.id} style={style}>
                            <div className="min-w-0 overflow-hidden text-ellipsis whitespace-nowrap">
                              {flexRender(cell.column.columnDef.cell, cell.getContext())}
                            </div>
                          </TableCell>
                        );
                      })}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={finalColumns.length} className="h-24 text-center">
                      Keine Einträge gefunden
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
            <ScrollBar orientation="horizontal" />
          </ScrollArea>
        )}
      </div>
      {enablePagination && (
        <DataTablePagination
          table={table}
          totalCount={totalCount}
          showSelectionLabel={showSelectionLabel}
        />
      )}
    </div>
  );
}
