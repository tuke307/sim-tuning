/**
 * Process-wide "unsaved changes" guard shared by the shell navigation and editors.
 *
 * An editor with a dirty draft registers a guard; anything that would unmount it (tab change,
 * vehicle switch) awaits `confirmLeave()` first and only proceeds when it resolves to `true`.
 */
export type LeaveGuard = () => Promise<boolean>;

let activeGuard: LeaveGuard | null = null;

/** Registers the guard for the current dirty editor. Returns an unregister function. */
export function setLeaveGuard(guard: LeaveGuard): () => void {
  activeGuard = guard;
  return () => {
    if (activeGuard === guard) activeGuard = null;
  };
}

/** Resolves `true` when navigation may proceed (no guard, or the guard allowed it). */
export async function confirmLeave(): Promise<boolean> {
  return activeGuard ? activeGuard() : true;
}

export function hasLeaveGuard(): boolean {
  return activeGuard !== null;
}
