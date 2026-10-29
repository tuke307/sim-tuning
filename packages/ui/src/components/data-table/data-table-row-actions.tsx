'use client';

import React from 'react';
import { Row, RowData } from '@tanstack/react-table';
import { Eye, Edit, Trash2, X } from 'lucide-react';
import type { features } from './data-table';

import { Button } from '@sim-tuning/ui/components/ui/button';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@sim-tuning/ui/components/ui/tooltip';

interface DataTableRowActionsProps<TData extends RowData> {
  row: Row<typeof features, TData>;
  onView?: (rowData: TData) => void;
  onEdit?: (rowData: TData) => void;
  onDelete?: (rowData: TData) => void;
  onCancel?: (rowData: TData) => void;
  extraActions?: React.ReactNode;
}

export function DataTableRowActions<TData extends RowData>({
  row,
  onView,
  onEdit,
  onDelete,
  onCancel,
  extraActions,
}: DataTableRowActionsProps<TData>) {
  return (
    <div className="flex items-center gap-1">
      <TooltipProvider>
        {onView && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0"
                onClick={() => onView(row.original)}
              >
                <Eye className="h-4 w-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Details anzeigen</p>
            </TooltipContent>
          </Tooltip>
        )}
        {onEdit && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0"
                data-testid="row-action-edit"
                onClick={() => onEdit(row.original)}
              >
                <Edit className="h-4 w-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Bearbeiten</p>
            </TooltipContent>
          </Tooltip>
        )}
        {onDelete && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0 text-destructive hover:text-destructive"
                data-testid="row-action-delete"
                onClick={() => onDelete(row.original)}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Löschen</p>
            </TooltipContent>
          </Tooltip>
        )}
        {onCancel && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0 text-destructive hover:text-destructive"
                onClick={() => onCancel(row.original)}
              >
                <X className="h-4 w-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Stornieren</p>
            </TooltipContent>
          </Tooltip>
        )}
        {extraActions}
      </TooltipProvider>
    </div>
  );
}
