import * as React from 'react';
import { ChevronDownIcon } from 'lucide-react';

import { cn } from '@sim-tuning/ui/lib/utils';

function NativeSelect({ className, children, ...props }: React.ComponentProps<'select'>) {
  const isFullWidth = className?.includes('w-full');
  const isSmAuto = className?.includes('sm:w-auto');
  const isFlex1 = className?.includes('flex-1');

  return (
    <div
      className={cn(
        'relative inline-flex max-w-full min-w-0 items-center',
        isFullWidth && 'w-full',
        isSmAuto && 'sm:w-auto',
        isFlex1 && 'flex-1',
      )}
    >
      <select
        data-slot="native-select"
        className={cn(
          'bg-raised text-foreground border-input focus-visible:border-ring focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring min-h-11 w-full max-w-full min-w-0 truncate appearance-none rounded-md border py-2 pr-9 pl-3 text-base leading-tight outline-none disabled:pointer-events-none disabled:opacity-50 sm:text-[0.9375rem]',
          className,
          'max-sm:text-base',
        )}
        {...props}
      >
        {children}
      </select>
      <ChevronDownIcon className="text-muted-foreground pointer-events-none absolute right-3 size-4 shrink-0" />
    </div>
  );
}

export { NativeSelect };
