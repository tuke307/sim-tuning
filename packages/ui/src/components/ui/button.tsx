import * as React from 'react';
import { Slot, Slottable } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';

import { cn } from '@sim-tuning/ui/lib/utils';

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md border border-transparent text-[0.9375rem] font-semibold disabled:pointer-events-none disabled:opacity-60 disabled:saturate-50 [&_svg]:pointer-events-none [&_svg:not([class*='size-'])]:size-4 shrink-0 [&_svg]:shrink-0 outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring aria-invalid:border-destructive",
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/85',
        accent: 'border-primary/60 text-primary hover:bg-primary/10',
        destructive:
          'border-destructive/70 text-destructive hover:bg-destructive/10 focus-visible:outline-destructive',
        outline: 'border-border bg-transparent text-foreground hover:bg-raised',
        secondary:
          'border-border bg-raised text-secondary-foreground hover:border-muted-foreground',
        ghost: 'text-foreground hover:bg-raised',
        link: 'text-primary underline underline-offset-4 decoration-1 hover:decoration-2',
      },
      size: {
        default: 'h-11 px-5 has-[>svg]:px-4',
        xs: 'h-8 pointer-coarse:h-11 gap-1 px-3 text-[0.8125rem]',
        sm: 'h-9 pointer-coarse:h-11 gap-1.5 px-4 has-[>svg]:px-3 text-sm',
        lg: 'h-12 px-6 has-[>svg]:px-5 text-[1.0625rem]',
        icon: 'size-11',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  },
);

type ButtonProps = React.ComponentProps<'button'> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean;
    icon?: React.ElementType;
    iconPlacement?: 'left' | 'right';
  };

function Button({
  className,
  variant,
  size,
  asChild = false,
  icon: Icon,
  iconPlacement,
  ...props
}: ButtonProps) {
  const Comp = asChild ? Slot : 'button';

  return (
    <Comp
      data-slot="button"
      className={cn(buttonVariants({ variant, size, className }))}
      {...props}
    >
      {Icon && iconPlacement === 'left' && <Icon />}
      <Slottable>{props.children}</Slottable>
      {Icon && iconPlacement === 'right' && <Icon />}
    </Comp>
  );
}

export { Button, buttonVariants };
