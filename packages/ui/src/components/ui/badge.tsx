import * as React from 'react';
import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';

import { cn } from '@sim-tuning/ui/lib/utils';

const badgeVariants = cva(
  'inline-flex items-center justify-center rounded-full px-2.5 py-0.5 text-xs font-medium w-fit whitespace-nowrap shrink-0 [&>svg]:size-3 gap-1 [&>svg]:pointer-events-none focus-visible:ring-ring/50 focus-visible:ring-[3px] transition-[color,background-color] overflow-hidden',
  {
    variants: {
      variant: {
        default: 'bg-primary/12 text-primary [a&]:hover:bg-primary/18',
        solid: 'bg-primary text-primary-foreground [a&]:hover:bg-primary/90',
        accent: 'bg-primary/12 text-primary [a&]:hover:bg-primary/18',
        secondary: 'bg-secondary text-secondary-foreground [a&]:hover:bg-secondary/80',
        success: 'bg-success/12 text-success',
        warning: 'bg-warning/15 text-warning',
        destructive: 'bg-destructive/12 text-destructive [a&]:hover:bg-destructive/18',
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
