'use client';

import { X } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { DateRange } from 'react-day-picker';
import { DataTableSingleDateFilter } from '@sim-tuning/ui/components/data-table/data-table-single-date-filter';

interface DataTableSearchToolbarProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  dateRange?: DateRange;
  onDateChange?: (dateRange?: DateRange) => void;
  placeholder?: string;
}

export function DataTableSearchToolbar({
  searchTerm,
  onSearchChange,
  dateRange,
  onDateChange,
  placeholder = 'Suchen...',
}: DataTableSearchToolbarProps) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex flex-1 items-center space-x-2">
        <Input
          placeholder={placeholder}
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          className="h-8 w-[150px] lg:w-[250px]"
        />

        {onDateChange && (
          <div className="flex space-x-2">
            <DataTableSingleDateFilter
              date={dateRange?.from}
              onDateChange={(date) =>
                onDateChange({
                  from: date || undefined, // Explicitly pass undefined
                  to: dateRange?.to, // Always include 'to'
                })
              }
              placeholder="Startdatum"
            />
            <DataTableSingleDateFilter
              date={dateRange?.to}
              onDateChange={(date) =>
                onDateChange({
                  from: dateRange?.from, // Always include 'from'
                  to: date || undefined, // Explicitly pass undefined
                })
              }
              placeholder="Enddatum"
            />
            <Button
              variant="outline"
              onClick={() =>
                onDateChange({
                  from: new Date(),
                  to: dateRange?.to,
                })
              }
              className="h-8 px-3"
            >
              Zukünftig
            </Button>
          </div>
        )}

        {(searchTerm || dateRange?.from || dateRange?.to) && (
          <Button
            variant="ghost"
            onClick={() => {
              onSearchChange('');
              onDateChange?.({ from: undefined, to: undefined });
            }}
            className="h-8 px-2 lg:px-3"
          >
            Zurücksetzen
            <X className="ml-2 h-4 w-4" />
          </Button>
        )}
      </div>
    </div>
  );
}
