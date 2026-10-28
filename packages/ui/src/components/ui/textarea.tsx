import * as React from 'react';

import { cn } from '@sim-tuning/ui/lib/utils';

function Textarea({ className, ...props }: React.ComponentProps<'textarea'>) {
  return (
    <textarea
      data-slot="textarea"
      className={cn(
        'placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-ring/50 aria-invalid:ring-destructive/20 aria-invalid:border-destructive bg-secondary flex field-sizing-content min-h-16 w-full rounded-xl border border-transparent px-3.5 py-2.5 text-base transition-[color,box-shadow] outline-none focus-visible:ring-[3px] disabled:cursor-not-allowed disabled:opacity-50 md:text-sm',
        className,
        'max-sm:text-base',
      )}
      {...props}
    />
  );
}

export { Textarea };
