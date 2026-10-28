'use client';

import { cn } from '@sim-tuning/ui/lib/utils';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Calendar } from '@sim-tuning/ui/components/ui/calendar';
import { CalendarIcon } from 'lucide-react';
import { Popover, PopoverContent, PopoverTrigger } from '@sim-tuning/ui/components/ui/popover';
import { DateRange } from 'react-day-picker';
import { format } from 'date-fns';
import { de } from 'date-fns/locale';

export function DataTableDateFilter({
  dateRange,
  onDateChange,
  className,
}: {
  dateRange?: DateRange;
  onDateChange?: (dateRange?: DateRange) => void;
  className?: string;
}) {
  return (
    <div className={cn('flex flex-col gap-2', className)}>
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            className={cn(
              'w-[300px] justify-start text-left font-normal',
              !dateRange?.from && 'text-muted-foreground',
            )}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {dateRange?.from ? (
              dateRange.to ? (
                <>
                  {format(dateRange.from, 'dd. MMM yyyy', { locale: de })} -{' '}
                  {format(dateRange.to, 'dd. MMM yyyy', { locale: de })}
                </>
              ) : (
                format(dateRange.from, 'dd. MMM yyyy', { locale: de })
              )
            ) : (
              <span>Zeitraum auswählen</span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            autoFocus
            mode="range"
            selected={dateRange}
            onSelect={onDateChange}
            numberOfMonths={2}
            weekStartsOn={1}
            locale={de}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
}
