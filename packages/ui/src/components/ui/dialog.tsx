'use client';

import * as React from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { XIcon } from 'lucide-react';

import { cn } from '@sim-tuning/ui/lib/utils';

function Dialog({ ...props }: React.ComponentProps<typeof DialogPrimitive.Root>) {
  return <DialogPrimitive.Root data-slot="dialog" {...props} />;
}

function DialogTrigger({ ...props }: React.ComponentProps<typeof DialogPrimitive.Trigger>) {
  return <DialogPrimitive.Trigger data-slot="dialog-trigger" {...props} />;
}

function DialogPortal({ ...props }: React.ComponentProps<typeof DialogPrimitive.Portal>) {
  return <DialogPrimitive.Portal data-slot="dialog-portal" {...props} />;
}

function DialogClose({ ...props }: React.ComponentProps<typeof DialogPrimitive.Close>) {
  return <DialogPrimitive.Close data-slot="dialog-close" {...props} />;
}

function DialogOverlay({
  className,
  ...props
}: React.ComponentProps<typeof DialogPrimitive.Overlay>) {
  return (
    <DialogPrimitive.Overlay
      data-slot="dialog-overlay"
      className={cn(
        'data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed inset-0 z-50 bg-scrim/55 backdrop-blur-[2px]',
        className,
      )}
      {...props}
    />
  );
}

const DISMISS_DISTANCE_RATIO = 0.25;
const DISMISS_VELOCITY = 0.5; // px per ms
const TAP_SLOP_PX = 6;

/**
 * On phones the dialog is a bottom sheet: the grabber at the top is a real handle — drag it down
 * to dismiss, or tap it to close — so no extra close button is shown there. Desktop keeps the
 * centered dialog with its close button.
 */
function useSheetDrag(close: () => void) {
  const contentRef = React.useRef<HTMLDivElement>(null);
  const drag = React.useRef<{
    startY: number;
    startTime: number;
    pointerId: number;
    dy: number;
  } | null>(null);

  const setOffset = (dy: number, animate: boolean) => {
    const el = contentRef.current;
    if (!el) return;
    el.style.transition = animate ? 'transform 200ms cubic-bezier(0.25, 1, 0.5, 1)' : 'none';
    el.style.transform = dy > 0 ? `translate3d(0, ${dy}px, 0)` : '';
  };

  const handlers = {
    onPointerDown: (e: React.PointerEvent<HTMLElement>) => {
      drag.current = { startY: e.clientY, startTime: e.timeStamp, pointerId: e.pointerId, dy: 0 };
      e.currentTarget.setPointerCapture(e.pointerId);
    },
    onPointerMove: (e: React.PointerEvent<HTMLElement>) => {
      const d = drag.current;
      if (!d || d.pointerId !== e.pointerId) return;
      d.dy = Math.max(0, e.clientY - d.startY);
      setOffset(d.dy, false);
    },
    onPointerUp: (e: React.PointerEvent<HTMLElement>) => {
      const d = drag.current;
      drag.current = null;
      if (!d) return;
      // Move events can lag behind on heavy sheets; the release position is authoritative.
      d.dy = Math.max(d.dy, e.clientY - d.startY);
      const height = contentRef.current?.offsetHeight ?? 1;
      const velocity = d.dy / Math.max(1, e.timeStamp - d.startTime);
      const isTap = d.dy < TAP_SLOP_PX;
      if (isTap || d.dy > height * DISMISS_DISTANCE_RATIO || velocity > DISMISS_VELOCITY) {
        close();
      } else {
        setOffset(0, true);
      }
    },
    onPointerCancel: () => {
      drag.current = null;
      setOffset(0, true);
    },
  };

  return { contentRef, handlers };
}

function DialogContent({
  className,
  children,
  onOpenAutoFocus,
  ...props
}: React.ComponentProps<typeof DialogPrimitive.Content>) {
  const closeRef = React.useRef<HTMLButtonElement>(null);
  const { contentRef, handlers } = useSheetDrag(() => closeRef.current?.click());

  const handleOpenAutoFocus = React.useCallback(
    (event: Event) => {
      onOpenAutoFocus?.(event);
      if (event.defaultPrevented) return;
      if (
        typeof window !== 'undefined' &&
        (window.matchMedia('(pointer: coarse)').matches || window.innerWidth < 640)
      ) {
        event.preventDefault();
      }
    },
    [onOpenAutoFocus],
  );

  return (
    <DialogPortal data-slot="dialog-portal">
      <DialogOverlay />
      <DialogPrimitive.Content
        ref={contentRef}
        data-slot="dialog-content"
        onOpenAutoFocus={handleOpenAutoFocus}
        className={cn(
          'bg-card text-card-foreground border-border data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed z-50 grid w-full grid-cols-[minmax(0,1fr)] gap-3 overflow-x-hidden overflow-y-auto overscroll-contain border shadow-2xl duration-200 focus:outline-hidden',
          'max-sm:inset-x-0 max-sm:top-auto max-sm:bottom-0 max-sm:max-h-[92dvh] max-sm:max-w-full max-sm:translate-x-0 max-sm:translate-y-0 max-sm:rounded-t-3xl max-sm:rounded-b-none max-sm:border-x-0 max-sm:border-b-0 max-sm:p-4 max-sm:pt-0 max-sm:pb-[calc(1rem+env(safe-area-inset-bottom,0px))] max-sm:data-[state=closed]:slide-out-to-bottom max-sm:data-[state=open]:slide-in-from-bottom',
          'sm:top-[50%] sm:left-[50%] sm:max-h-[90dvh] sm:max-w-lg sm:translate-x-[-50%] sm:translate-y-[-50%] sm:gap-4 sm:rounded-3xl sm:p-6 sm:data-[state=closed]:zoom-out-95 sm:data-[state=open]:zoom-in-95',
          className,
        )}
        {...props}
      >
        <button
          type="button"
          data-slot="dialog-grabber"
          aria-label="Close"
          {...handlers}
          className="sticky top-0 z-10 -mx-4 flex h-6 shrink-0 cursor-grab touch-none items-center justify-center bg-inherit focus-visible:outline-hidden active:cursor-grabbing sm:hidden"
        >
          <span aria-hidden="true" className="bg-muted-foreground/25 h-1.5 w-10 rounded-full" />
        </button>
        {children}
        <DialogPrimitive.Close
          ref={closeRef}
          className="ring-offset-card focus:ring-ring bg-secondary/80 text-muted-foreground hover:bg-secondary hover:text-foreground absolute top-4 right-4 flex size-8 items-center justify-center rounded-full transition-colors focus:ring-2 focus:ring-offset-2 focus:outline-hidden disabled:pointer-events-none max-sm:hidden [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4"
        >
          <XIcon />
          <span className="sr-only">Close</span>
        </DialogPrimitive.Close>
      </DialogPrimitive.Content>
    </DialogPortal>
  );
}

function DialogHeader({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      data-slot="dialog-header"
      className={cn('flex min-w-0 flex-col gap-1 text-left sm:gap-1.5 sm:pr-9', className)}
      {...props}
    />
  );
}

function DialogFooter({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      data-slot="dialog-footer"
      className={cn('flex flex-col-reverse gap-2 sm:flex-row sm:justify-end', className)}
      {...props}
    />
  );
}

function DialogTitle({ className, ...props }: React.ComponentProps<typeof DialogPrimitive.Title>) {
  return (
    <DialogPrimitive.Title
      data-slot="dialog-title"
      className={cn('text-lg leading-none font-semibold', className)}
      {...props}
    />
  );
}

function DialogDescription({
  className,
  ...props
}: React.ComponentProps<typeof DialogPrimitive.Description>) {
  return (
    <DialogPrimitive.Description
      data-slot="dialog-description"
      className={cn('text-muted-foreground text-sm', className)}
      {...props}
    />
  );
}

export {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
  DialogTrigger,
};
