import * as React from 'react';

import { cn } from '@sim-tuning/ui/lib/utils';

function Input({ className, type, ...props }: React.ComponentProps<'input'>) {
  return (
    <input
      type={type}
      data-slot="input"
      className={cn(
        'file:text-foreground placeholder:text-muted-foreground selection:bg-primary selection:text-primary-foreground bg-raised border-input flex h-11 w-full min-w-0 rounded-md border px-3 py-1 text-base outline-none file:inline-flex file:h-7 file:border-0 file:bg-transparent file:text-sm file:font-medium disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-[0.9375rem]',
        'focus-visible:border-ring focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring',
        'aria-invalid:border-destructive',
        className,
        'max-sm:text-base',
      )}
      {...props}
    />
  );
}

export { Input };
