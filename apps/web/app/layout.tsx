import type { Metadata, Viewport } from 'next';
import { Suspense } from 'react';

import '@sim-tuning/ui/globals.css';

import { PageViewTracker } from '@/components/analytics/page-view-tracker';
import { Providers } from '@/providers/Providers';
import RootLoading from './loading';
import { SITE_URL } from '../lib/config/env.config';

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  viewportFit: 'cover',
  // Dark ("drafting table at night") is the default theme regardless of the system setting.
  themeColor: '#111614',
};

export const metadata: Metadata = {
  title: {
    default: 'SimTuning — Two-Stroke Tuning',
    template: '%s | SimTuning',
  },
  description:
    'Simulate, calculate and measure your two-stroke engine. Works offline on iPhone and the web.',
  applicationName: 'SimTuning',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'black',
    title: 'SimTuning',
  },
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  metadataBase: new URL(SITE_URL),
  icons: {
    icon: '/icon.svg',
    apple: '/icon-192.svg',
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="antialiased" suppressHydrationWarning>
        <Suspense fallback={<RootLoading />}>
          <Providers>{children}</Providers>
        </Suspense>
        <PageViewTracker />
      </body>
    </html>
  );
}
