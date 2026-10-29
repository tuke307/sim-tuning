import * as React from 'react';
import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';

import { cn } from '@sim-tuning/ui/lib/utils';

const badgeVariants = cva(
  'inline-flex items-center justify-center rounded-xs border border-current/40 px-1.5 py-0.5 text-[0.6875rem] font-semibold uppercase tracking-[0.06em] w-fit whitespace-nowrap shrink-0 [&>svg]:size-3 gap-1 [&>svg]:pointer-events-none overflow-hidden',
  {
    variants: {
      variant: {
        default: 'text-primary',
        solid: 'bg-primary text-primary-foreground [a&]:hover:bg-primary/90',
        accent: 'text-primary',
        secondary: 'bg-raised text-muted-foreground border-border',
        success: 'text-success',
        warning: 'text-warning',
        destructive: 'text-destructive',
        outline: 'border border-border text-foreground [a&]:hover:bg-secondary',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  },
);

function Badge({
  className,
  variant,
  asChild = false,
  ...props
}: React.ComponentProps<'span'> & VariantProps<typeof badgeVariants> & { asChild?: boolean }) {
  const Comp = asChild ? Slot : 'span';

  return (
    <Comp data-slot="badge" className={cn(badgeVariants({ variant }), className)} {...props} />
  );
}

export { Badge, badgeVariants };
