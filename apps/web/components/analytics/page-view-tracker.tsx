'use client';

import { useEffect } from 'react';
import { usePathname } from 'next/navigation';
import { trackEvent, trackPageview } from '@/lib/analytics/track';

/**
 * Sends an anonymous pageview whenever the route pathname changes. Uses only the pathname
 * (never search params) so tokens in auth links like /reset-password?token=… are never sent.
 * Also reports PWA installs via the browser's `appinstalled` event (no device information read).
 */
export function PageViewTracker() {
  const pathname = usePathname();

  useEffect(() => {
    if (pathname) trackPageview(pathname);
  }, [pathname]);

  useEffect(() => {
    const onInstalled = () => trackEvent('pwa_installed');
    window.addEventListener('appinstalled', onInstalled);
    return () => window.removeEventListener('appinstalled', onInstalled);
  }, []);

  return null;
}
