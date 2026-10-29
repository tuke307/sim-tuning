'use client';

import { useTheme } from 'next-themes';
import { Toaster as Sonner, ToasterProps, toast } from 'sonner';

const Toaster = ({ ...props }: ToasterProps) => {
  const { theme = 'system' } = useTheme();

  return (
    <Sonner
      theme={theme as ToasterProps['theme']}
      className="toaster group"
      style={
        {
          // Sonner consumes these raw for `background:` / `color:` (no hsl()
          // wrap), so values must be complete colors. The theme stores full
          // OKLCH colors, so the tokens can be passed through directly — a bare
          // channel triple would be discarded and the toast would render
          // transparent with inherited text, making it invisible on the page.
          '--normal-bg': 'var(--popover)',
          '--normal-text': 'var(--popover-foreground)',
          '--normal-border': 'var(--border)',
          '--error-bg': 'var(--destructive)',
          '--error-text': 'var(--destructive-foreground)',
          '--error-border': 'var(--destructive)',
          '--success-bg': 'var(--success)',
          '--success-text': 'var(--success-foreground)',
          '--success-border': 'var(--success)',
        } as React.CSSProperties
      }
      {...props}
    />
  );
};

export { Toaster, toast };
