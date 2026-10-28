import { Variants, Transition } from 'framer-motion';

export const ANIMATION_CONFIG = {
  durations: {
    fast: 0.2,
    normal: 0.4,
    slow: 0.6,
  },

  easings: {
    smooth: [0.25, 0.25, 0.25, 1] as const,
    gentle: [0.4, 0.0, 0.2, 1] as const,
  },

  stagger: {
    fast: 0.05,
    normal: 0.1,
  },

  reducedMotion: {
    duration: 0.01,
    ease: 'linear' as const,
  },
} as const;

export const TRANSITIONS: Record<string, Transition> = {
  smooth: {
    duration: ANIMATION_CONFIG.durations.normal,
    ease: ANIMATION_CONFIG.easings.smooth,
  },
  gentle: {
    duration: ANIMATION_CONFIG.durations.normal,
    ease: ANIMATION_CONFIG.easings.gentle,
  },
  fast: {
    duration: ANIMATION_CONFIG.durations.fast,
    ease: ANIMATION_CONFIG.easings.gentle,
  },
};

export const ANIMATION_VARIANTS = {
  fadeIn: {
    hidden: { opacity: 0 },
    visible: { opacity: 1, transition: TRANSITIONS.smooth },
  } as Variants,

  fadeInUp: {
    hidden: { opacity: 0, y: 15 },
    visible: { opacity: 1, y: 0, transition: TRANSITIONS.smooth },
  } as Variants,

  scaleIn: {
    hidden: { opacity: 0, scale: 0.95 },
    visible: { opacity: 1, scale: 1, transition: TRANSITIONS.smooth },
  } as Variants,

  staggerContainer: {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: ANIMATION_CONFIG.stagger.normal,
        delayChildren: 0.1,
      },
    },
  } as Variants,

  staggerItem: {
    hidden: { opacity: 0, y: 10 },
    visible: { opacity: 1, y: 0, transition: TRANSITIONS.gentle },
  } as Variants,

  pageTransition: {
    initial: { opacity: 0 },
    in: { opacity: 1 },
    out: { opacity: 0 },
  } as Variants,
} as const;

export const UI_ANIMATIONS = {
  card: {
    hover: { y: -2, transition: TRANSITIONS.fast },
  },

  button: {
    hover: { scale: 1.02, transition: TRANSITIONS.fast },
    tap: { scale: 0.98 },
  },

  image: {
    hover: { scale: 1.02, transition: TRANSITIONS.smooth },
  },
} as const;
