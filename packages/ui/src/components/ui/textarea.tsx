import * as React from 'react';

import { cn } from '@sim-tuning/ui/lib/utils';

function Textarea({ className, ...props }: React.ComponentProps<'textarea'>) {
  return (
    <textarea
      data-slot="textarea"
      className={cn(
        'placeholder:text-muted-foreground focus-visible:border-ring focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring aria-invalid:border-destructive bg-raised border-input flex field-sizing-content min-h-16 w-full rounded-md border px-3 py-2.5 text-base outline-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm',
        className,
        'max-sm:text-base',
      )}
      {...props}
    />
  );
}

export { Textarea };
