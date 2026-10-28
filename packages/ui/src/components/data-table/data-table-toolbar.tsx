'use client';

import { Table, RowData } from '@tanstack/react-table';
import { X } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { DataTableViewOptions } from '@sim-tuning/ui/components/data-table/data-table-view-options';
import type { features } from './data-table';

interface DataTableToolbarProps<TData extends RowData> {
  table: Table<typeof features, TData>;
  searchTerm: string;
  onSearchChange: (value: string) => void;
}

export function DataTableToolbar<TData extends RowData>({
  table,
  searchTerm,
  onSearchChange,
}: DataTableToolbarProps<TData>) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex flex-1 items-center space-x-2">
        <Input
          placeholder="Reservierungen suchen..."
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          className="h-8 w-[150px] lg:w-[250px]"
        />
        {searchTerm && (
          <Button variant="ghost" onClick={() => onSearchChange('')} className="h-8 px-2 lg:px-3">
            Zurücksetzen
            <X className="ml-2 h-4 w-4" />
          </Button>
        )}
      </div>
      <DataTableViewOptions table={table} />
    </div>
  );
}
