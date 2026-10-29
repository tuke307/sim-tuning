import { NAV_VIEW_PARAM } from '@/lib/navigation/nav-state';
import type { NavTab } from '../pwa-shell';

const NAV_VIEWS: readonly NavTab[] = [
  'home',
  'vehicles',
  'motor',
  'einlass',
  'auslass',
  'dyno',
  'profile',
];

/** Reads the top-level view from `?m=`; missing or unknown values open Home. */
export function parseNavView(raw: string | null | undefined): NavTab {
  return raw && (NAV_VIEWS as readonly string[]).includes(raw) ? (raw as NavTab) : 'home';
}

/** URL of a top-level view. Only `m` is set, so a module always opens on its default sub-tab. */
export function navViewHref(view: NavTab): string {
  return view === 'home' ? '/' : `/?${NAV_VIEW_PARAM}=${view}`;
}
