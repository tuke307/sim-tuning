import { Column, RowData } from '@tanstack/react-table';
import { ArrowDown, ArrowUp, ChevronsUpDown, EyeOff } from 'lucide-react';
import type { features } from './data-table';

import { cn } from '@sim-tuning/ui/lib/utils';
import { Button } from '@sim-tuning/ui/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@sim-tuning/ui/components/ui/dropdown-menu';

interface DataTableColumnHeaderProps<
  TData extends RowData,
  TValue,
> extends React.HTMLAttributes<HTMLDivElement> {
  column: Column<typeof features, TData, TValue>;
  title: string;
}

export function DataTableColumnHeader<TData extends RowData, TValue>({
  column,
  title,
  className,
}: DataTableColumnHeaderProps<TData, TValue>) {
  if (!column.getCanSort()) {
    return (
      <div className={cn('min-w-0 overflow-hidden text-ellipsis whitespace-nowrap', className)}>
        {title}
      </div>
    );
  }

  return (
    <div className={cn('flex items-center min-w-0', className)}>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="sm" className="h-8 px-2 data-[state=open]:bg-accent">
            {/* apply truncation only to the text, not the whole button */}
            <span className="min-w-0 shrink overflow-hidden text-ellipsis whitespace-nowrap">
              {title}
            </span>
            {/* icons should not shrink or be clipped */}
            <span className="shrink-0">
              {column.getIsSorted() === 'desc' ? (
                <ArrowDown />
              ) : column.getIsSorted() === 'asc' ? (
                <ArrowUp />
              ) : (
                <ChevronsUpDown />
              )}
            </span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start">
          <DropdownMenuItem onClick={() => column.toggleSorting(false)}>
            <ArrowUp className="h-3.5 w-3.5 text-muted-foreground/70" />
            Aufsteigend
          </DropdownMenuItem>
          <DropdownMenuItem onClick={() => column.toggleSorting(true)}>
            <ArrowDown className="h-3.5 w-3.5 text-muted-foreground/70" />
            Absteigend
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem onClick={() => column.toggleVisibility(false)}>
            <EyeOff className="h-3.5 w-3.5 text-muted-foreground/70" />
            Ausblenden
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
