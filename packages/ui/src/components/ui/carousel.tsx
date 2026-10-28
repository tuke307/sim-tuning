'use client';

import * as React from 'react';
import useEmblaCarousel, { type UseEmblaCarouselType } from 'embla-carousel-react';
import { ArrowLeft, ArrowRight } from 'lucide-react';

import { cn } from '@sim-tuning/ui/lib/utils';
import { Button } from '@sim-tuning/ui/components/ui/button';

type CarouselApi = UseEmblaCarouselType[1];
type UseCarouselParameters = Parameters<typeof useEmblaCarousel>;
type CarouselOptions = UseCarouselParameters[0];
type CarouselPlugin = UseCarouselParameters[1];

type CarouselProps = {
  opts?: CarouselOptions;
  plugins?: CarouselPlugin;
  orientation?: 'horizontal' | 'vertical';
  setApi?: (api: CarouselApi) => void;
};

type CarouselContextProps = {
  carouselRef: ReturnType<typeof useEmblaCarousel>[0];
  api: ReturnType<typeof useEmblaCarousel>[1];
  scrollPrev: () => void;
  scrollNext: () => void;
  canScrollPrev: boolean;
  canScrollNext: boolean;
  selectedIndex: number;
  scrollSnaps: number[];
  scrollToIndex: (index: number) => void;
} & CarouselProps;

const CarouselContext = React.createContext<CarouselContextProps | null>(null);

function useCarousel() {
  const context = React.useContext(CarouselContext);

  if (!context) {
    throw new Error('useCarousel must be used within a <Carousel />');
  }

  return context;
}

const EMPTY_SNAPSHOT = '0|0|0|0';

function Carousel({
  orientation = 'horizontal',
  opts,
  setApi,
  plugins,
  className,
  children,
  ...props
}: React.ComponentProps<'div'> & CarouselProps) {
  const [carouselRef, api] = useEmblaCarousel(
    {
      ...opts,
      axis: orientation === 'horizontal' ? 'x' : 'y',
    },
    plugins,
  );
  // Embla is an external store: subscribe to its events instead of mirroring them into state.
  const subscribe = React.useCallback(
    (onChange: () => void) => {
      if (!api) return () => {};
      api.on('reInit', onChange);
      api.on('select', onChange);
      return () => {
        api.off('reInit', onChange);
        api.off('select', onChange);
      };
    },
    [api],
  );
  const snapshot = React.useSyncExternalStore(
    subscribe,
    () =>
      api
        ? `${Number(api.canScrollPrev())}|${Number(api.canScrollNext())}|${api.selectedScrollSnap()}|${api.scrollSnapList().length}`
        : EMPTY_SNAPSHOT,
    () => EMPTY_SNAPSHOT,
  );
  const [prevFlag = 0, nextFlag = 0, selectedIndex = 0, snapCount = 0] = snapshot
    .split('|')
    .map(Number);
  const canScrollPrev = prevFlag === 1;
  const canScrollNext = nextFlag === 1;
  const scrollSnaps = React.useMemo(
    () => (api && snapCount > 0 ? api.scrollSnapList() : []),
    [api, snapCount],
  );

  const scrollPrev = React.useCallback(() => {
    api?.scrollPrev();
  }, [api]);

  const scrollNext = React.useCallback(() => {
    api?.scrollNext();
  }, [api]);

  const scrollToIndex = React.useCallback(
    (index: number) => {
      api?.scrollTo(index);
    },
    [api],
  );

  const handleKeyDown = React.useCallback(
    (event: React.KeyboardEvent<HTMLDivElement>) => {
      if (event.key === 'ArrowLeft') {
        event.preventDefault();
        scrollPrev();
      } else if (event.key === 'ArrowRight') {
        event.preventDefault();
        scrollNext();
      }
    },
    [scrollPrev, scrollNext],
  );

  React.useEffect(() => {
    if (!api || !setApi) return;
    setApi(api);
  }, [api, setApi]);

  return (
    <CarouselContext.Provider
      value={{
        carouselRef,
        api: api,
        opts,
        orientation: orientation || (opts?.axis === 'y' ? 'vertical' : 'horizontal'),
        scrollPrev,
        scrollNext,
        canScrollPrev,
        canScrollNext,
        selectedIndex,
        scrollSnaps,
        scrollToIndex,
      }}
    >
      <div
        onKeyDownCapture={handleKeyDown}
        className={cn('relative w-full', className)}
        role="region"
        aria-roledescription="carousel"
        data-slot="carousel"
        {...props}
      >
        {children}
      </div>
    </CarouselContext.Provider>
  );
}

function CarouselContent({ className, ...props }: React.ComponentProps<'div'>) {
  const { carouselRef, orientation } = useCarousel();

  return (
    <div ref={carouselRef} className="overflow-hidden" data-slot="carousel-content">
      <div
        className={cn('flex', orientation === 'horizontal' ? '-ml-4' : '-mt-4 flex-col', className)}
        {...props}
      />
    </div>
  );
}

function CarouselItem({ className, ...props }: React.ComponentProps<'div'>) {
  const { orientation } = useCarousel();

  return (
    <div
      role="group"
      aria-roledescription="slide"
      data-slot="carousel-item"
      className={cn(
        'min-w-0 shrink-0 grow-0 basis-full',
        orientation === 'horizontal' ? 'pl-4' : 'pt-4',
        className,
      )}
      {...props}
    />
  );
}

function CarouselPrevious({
  className,
  variant = 'outline',
  size = 'icon',
  ...props
}: React.ComponentProps<typeof Button>) {
  const { orientation, scrollPrev, canScrollPrev } = useCarousel();

  return (
    <Button
      data-slot="carousel-previous"
      variant={variant}
      size={size}
      className={cn(
        'absolute size-10 rounded-full',
        orientation === 'horizontal'
          ? 'top-1/2 left-0 -translate-y-1/2 z-20'
          : '-top-12 left-1/2 -translate-x-1/2 rotate-90',
        className,
      )}
      disabled={!canScrollPrev}
      onClick={scrollPrev}
      {...props}
    >
      <ArrowLeft className="w-5 h-5" />
      <span className="sr-only">Previous slide</span>
    </Button>
  );
}

function CarouselNext({
  className,
  variant = 'outline',
  size = 'icon',
  ...props
}: React.ComponentProps<typeof Button>) {
  const { orientation, scrollNext, canScrollNext } = useCarousel();

  return (
    <Button
      data-slot="carousel-next"
      variant={variant}
      size={size}
      className={cn(
        'absolute size-10 rounded-full',
        orientation === 'horizontal'
          ? 'top-1/2 right-0 -translate-y-1/2 z-20'
          : '-bottom-12 left-1/2 -translate-x-1/2 rotate-90',
        className,
      )}
      disabled={!canScrollNext}
      onClick={scrollNext}
      {...props}
    >
      <ArrowRight className="w-5 h-5" />
      <span className="sr-only">Next slide</span>
    </Button>
  );
}

/**
 * CarouselIndicators Component
 * Displays dots representing each slide for navigation
 */
function CarouselIndicators({
  className,
  showIndicators = true,
}: {
  className?: string;
  showIndicators?: boolean;
}) {
  const { selectedIndex, scrollSnaps, scrollToIndex } = useCarousel();

  if (!showIndicators || scrollSnaps.length <= 1) {
    return null;
  }

  return (
    <div
      className={cn('flex justify-center gap-2 mt-4', className)}
      role="tablist"
      aria-label="Slide indicators"
    >
      {scrollSnaps.map((_, index) => (
        <button
          key={index}
          onClick={() => scrollToIndex(index)}
          className={cn(
            'h-2 rounded-full transition-all duration-300 cursor-pointer',
            selectedIndex === index ? 'w-8 bg-primary' : 'w-2 bg-primary/30 hover:bg-primary/50',
          )}
          role="tab"
          aria-selected={selectedIndex === index}
          aria-label={`Go to slide ${index + 1}`}
        />
      ))}
    </div>
  );
}

/**
 * ServiceCarouselCard Component
 * Pre-styled card for service/product showcase carousels
 */
function ServiceCarouselCard({
  title,
  description,
  imageSrc,
  imageAlt,
  buttonLabel,
  onButtonClick,
  className,
  aiBadgeLabel,
}: {
  title: string;
  description: string;
  imageSrc: string;
  imageAlt: string;
  buttonLabel?: string;
  onButtonClick?: () => void;
  className?: string;
  aiBadgeLabel?: string;
}) {
  return (
    <CarouselItem className={cn('basis-full md:basis-1/2 lg:basis-1/3', className)}>
      <div className="relative h-125 rounded overflow-hidden shadow-2xl bg-secondary/30 border border-foreground/10 group cursor-pointer">
        {/* Background Image */}
        <img
          src={imageSrc}
          alt={imageAlt}
          className="absolute inset-0 w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        {aiBadgeLabel && (
          <span className="absolute top-2 right-2 z-10 inline-flex items-center gap-1 rounded px-2 py-0.5 bg-black/60 text-white text-[0.625rem] leading-none font-medium backdrop-blur-sm pointer-events-none select-none">
            ✦ {aiBadgeLabel}
          </span>
        )}

        {/* Dark Overlay */}
        <div className="absolute inset-0 bg-linear-to-b from-scrim/30 via-scrim/45 to-scrim/70" />

        {/* Content Container */}
        <div className="absolute inset-0 flex flex-col justify-between p-8">
          {/* Title and Description */}
          <div>
            <h3 className="text-3xl font-semibold leading-tight mb-2">{title}</h3>
            <p className="text-base leading-relaxed">{description}</p>
          </div>

          {/* Button (optional) */}
          {buttonLabel ? (
            <Button
              onClick={onButtonClick}
              variant="secondary"
              className="w-fit bg-primary/80 hover:bg-primary border-0 font-medium"
            >
              {buttonLabel}
            </Button>
          ) : null}
        </div>
      </div>
    </CarouselItem>
  );
}

export {
  type CarouselApi,
  Carousel,
  CarouselContent,
  CarouselItem,
  CarouselPrevious,
  CarouselNext,
  CarouselIndicators,
  ServiceCarouselCard,
};
