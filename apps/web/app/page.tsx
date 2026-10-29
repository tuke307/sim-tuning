import { Suspense } from 'react';
import { PwaShell } from '@/components/sim/pwa-shell';
import RootLoading from './loading';

export default function HomePage() {
  // The shell reads the active view from the URL (`useSearchParams`), which needs a Suspense
  // boundary so the page can still be rendered statically.
  return (
    <Suspense fallback={<RootLoading />}>
      <PwaShell />
    </Suspense>
  );
}
