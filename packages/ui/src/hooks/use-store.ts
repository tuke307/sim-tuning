import { useSyncExternalStore } from 'react';

const subscribeNoop = () => () => {};

/**
 * This hook fix hydration when use persist to save hook data to localStorage
 */
export const useStore = <T, F>(
  store: (callback: (state: T) => unknown) => unknown,
  callback: (state: T) => F,
) => {
  const result = store(callback) as F;
  const isHydrated = useSyncExternalStore(
    subscribeNoop,
    () => true,
    () => false,
  );

  return isHydrated ? result : undefined;
};
