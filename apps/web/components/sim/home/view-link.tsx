'use client';

import React from 'react';
import type { NavTab } from '../pwa-shell';
import { navViewHref } from './nav-view';

export type NavigateFn = (view: NavTab) => void;

/**
 * Real link to a top-level view, so it can be opened in a new tab or copied. A plain click is
 * routed through the shell's guarded navigation instead of a full page load.
 */
export function ViewLink({
  view,
  onNavigate,
  onClick,
  ...props
}: Omit<React.ComponentProps<'a'>, 'href'> & { view: NavTab; onNavigate: NavigateFn }) {
  return (
    <a
      href={navViewHref(view)}
      onClick={(event) => {
        onClick?.(event);
        if (
          event.defaultPrevented ||
          event.button !== 0 ||
          event.metaKey ||
          event.ctrlKey ||
          event.shiftKey ||
          event.altKey
        ) {
          return;
        }
        event.preventDefault();
        onNavigate(view);
      }}
      {...props}
    />
  );
}
