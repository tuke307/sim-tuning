import * as React from 'react';
import { Slot, Slottable } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';

import { cn } from '@sim-tuning/ui/lib/utils';

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-full text-[0.9375rem] font-semibold transition-[background-color,color,box-shadow,transform,opacity] duration-150 ease-(--ease-out-quart) active:scale-[0.97] disabled:pointer-events-none disabled:opacity-40 [&_svg]:pointer-events-none [&_svg:not([class*='size-'])]:size-4 shrink-0 [&_svg]:shrink-0 outline-hidden focus-visible:ring-ring/50 focus-visible:ring-[3px] aria-invalid:ring-destructive/20",
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        accent: 'bg-primary/12 text-primary hover:bg-primary/18',
        destructive:
          'bg-destructive/12 text-destructive hover:bg-destructive/18 focus-visible:ring-destructive/30',
        outline: 'border border-border bg-card text-foreground hover:bg-secondary',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/70',
        ghost: 'text-foreground hover:bg-secondary',
        link: 'text-primary underline-offset-4 hover:underline rounded-md',
      },
      size: {
        default: 'h-11 px-5 has-[>svg]:px-4',
        xs: 'h-8 gap-1 px-3 text-[0.8125rem]',
        sm: 'h-9 gap-1.5 px-4 has-[>svg]:px-3 text-sm',
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
