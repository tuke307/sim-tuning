'use client';

import Link from 'next/link';
import { Button } from '@sim-tuning/ui/components/ui/button';

export default function NotFound() {
  return (
    <div className="bg-background flex min-h-dvh items-center justify-center p-4">
      <div className="w-full max-w-sm space-y-6 text-center">
        <div className="space-y-2">
          <h1 className="text-[1.75rem] font-bold">Page not found</h1>
          <p className="text-muted-foreground text-[0.9375rem]">
            This page doesn&apos;t exist. Your vehicles are still right where you left them.
          </p>
        </div>
        <Button size="lg" asChild>
          <Link href="/">Back to SimTuning</Link>
        </Button>
      </div>
    </div>
  );
}
