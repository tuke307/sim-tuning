import Image from 'next/image';

export interface BrandLogoProps {
  /** `mark` drops the TUNING wordmark for small sizes where it would not be legible. */
  variant?: 'full' | 'mark';
  /** Rendered width and height in CSS px. */
  size: number;
  className?: string;
  /** Decorative when the brand name is already visible next to it. */
  decorative?: boolean;
}

/** SimTuning logo (vector source: `assets/brand`, copied by `scripts/generate-brand-assets.swift`). */
export default function BrandLogo({
  variant = 'full',
  size,
  className,
  decorative = false,
}: BrandLogoProps) {
  return (
    <Image
      src={variant === 'full' ? '/logo.svg' : '/logo-mark.svg'}
      alt={decorative ? '' : 'SimTuning'}
      width={size}
      height={size}
      unoptimized
      priority
      className={className}
    />
  );
}
