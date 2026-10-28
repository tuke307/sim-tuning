/**
 * Least-squares polynomial fit via Householder QR on the Vandermonde matrix — the same approach
 * as MathNet `Fit.PolynomialFunc` used by v2. Returns coefficients c₀…c_order (ascending powers).
 */
export function polyfit(xs: ArrayLike<number>, ys: ArrayLike<number>, order: number): number[] {
  const m = xs.length;
  const n = order + 1;
  if (m !== ys.length) throw new Error('x and y must have the same length');
  if (m < n) throw new Error(`Need at least ${n} points for a polynomial of order ${order}`);

  const a: number[][] = [];
  const b: number[] = [];
  for (let i = 0; i < m; i++) {
    const row = new Array<number>(n);
    let p = 1;
    for (let j = 0; j < n; j++) {
      row[j] = p;
      p *= xs[i]!;
    }
    a.push(row);
    b.push(ys[i]!);
  }

  for (let k = 0; k < n; k++) {
    let norm = 0;
    for (let i = k; i < m; i++) norm += a[i]![k]! ** 2;
    norm = Math.sqrt(norm);
    if (norm === 0) continue;
    const alpha = a[k]![k]! > 0 ? -norm : norm;
    const vk = new Array<number>(m - k);
    for (let i = k; i < m; i++) vk[i - k] = a[i]![k]!;
    vk[0]! -= alpha;
    let vnorm2 = 0;
    for (const value of vk) vnorm2 += value * value;
    if (vnorm2 === 0) continue;
    for (let j = k; j < n; j++) {
      let dot = 0;
      for (let i = k; i < m; i++) dot += vk[i - k]! * a[i]![j]!;
      const f = (2 * dot) / vnorm2;
      for (let i = k; i < m; i++) a[i]![j]! -= f * vk[i - k]!;
    }
    let dot = 0;
    for (let i = k; i < m; i++) dot += vk[i - k]! * b[i]!;
    const f = (2 * dot) / vnorm2;
    for (let i = k; i < m; i++) b[i]! -= f * vk[i - k]!;
  }

  const coefficients = new Array<number>(n).fill(0);
  for (let k = n - 1; k >= 0; k--) {
    let sum = b[k]!;
    for (let j = k + 1; j < n; j++) sum -= a[k]![j]! * coefficients[j]!;
    coefficients[k] = sum / a[k]![k]!;
  }
  return coefficients;
}

/** Evaluates ascending-power coefficients with Horner's scheme. */
export function polyval(coefficients: readonly number[], x: number): number {
  let y = 0;
  for (let i = coefficients.length - 1; i >= 0; i--) y = y * x + coefficients[i]!;
  return y;
}
