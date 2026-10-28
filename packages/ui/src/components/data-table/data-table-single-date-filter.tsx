'use client';

import { cn } from '@sim-tuning/ui/lib/utils';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Calendar } from '@sim-tuning/ui/components/ui/calendar';
import { CalendarIcon } from 'lucide-react';
import { Popover, PopoverContent, PopoverTrigger } from '@sim-tuning/ui/components/ui/popover';
import { format } from 'date-fns';
import { de } from 'date-fns/locale';

export function DataTableSingleDateFilter({
  date,
  onDateChange,
  className,
  placeholder = 'Datum auswählen',
}: {
  date?: Date;
  onDateChange?: (date?: Date) => void;
  className?: string;
  placeholder?: string;
}) {
  return (
    <div className={cn('flex flex-col gap-2', className)}>
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            className={cn(
              'w-[200px] justify-start text-left font-normal',
              !date && 'text-muted-foreground',
            )}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date ? format(date, 'dd. MMM yyyy', { locale: de }) : <span>{placeholder}</span>}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            onSelect={onDateChange}
            autoFocus
            weekStartsOn={1}
            locale={de}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
}
