export interface Point2D {
  x: number;
  y: number;
}

/**
 * DBSCAN as implemented by the `DBSCAN` NuGet package v2 used (viceroypenguin/DBSCAN, MIT):
 * linear neighbour search with a strict `distance < epsilon` (the point itself included),
 * clusters seeded in input order and expanded breadth-first. Matching that order keeps the
 * cluster numbering ("Graph 1", "Graph 2", …) identical to v2. Noise points are dropped.
 *
 * Neighbour search uses a uniform grid (cell = epsilon) but returns candidates in input order,
 * so results are identical to the O(N²) list search.
 */
export function dbscan<T extends Point2D>(
  points: readonly T[],
  epsilon: number,
  minimumPointsPerCluster: number,
): T[][] {
  const n = points.length;
  const visited = new Uint8Array(n);
  const clustered = new Uint8Array(n);

  const cells = new Map<string, number[]>();
  const cellOf = (p: Point2D) => [Math.floor(p.x / epsilon), Math.floor(p.y / epsilon)] as const;
  points.forEach((p, i) => {
    const [cx, cy] = cellOf(p);
    const key = `${cx}:${cy}`;
    const bucket = cells.get(key);
    if (bucket) bucket.push(i);
    else cells.set(key, [i]);
  });

  const search = (index: number): number[] => {
    const p = points[index]!;
    const [cx, cy] = cellOf(p);
    const found: number[] = [];
    for (let dx = -1; dx <= 1; dx++) {
      for (let dy = -1; dy <= 1; dy++) {
        for (const j of cells.get(`${cx + dx}:${cy + dy}`) ?? []) {
          const q = points[j]!;
          const ddx = q.x - p.x;
          const ddy = q.y - p.y;
          if (Math.sqrt(ddx * ddx + ddy * ddy) < epsilon) found.push(j);
        }
      }
    }
    return found.sort((a, b) => a - b);
  };

  const clusters: T[][] = [];
  for (let i = 0; i < n; i++) {
    if (visited[i]) continue;
    visited[i] = 1;
    const candidates = search(i);
    if (candidates.length < minimumPointsPerCluster) continue;

    const members: T[] = [points[i]!];
    clustered[i] = 1;
    const queue = candidates.slice();
    for (let head = 0; head < queue.length; head++) {
      const j = queue[head]!;
      if (!visited[j]) {
        visited[j] = 1;
        const neighbours = search(j);
        if (neighbours.length >= minimumPointsPerCluster) queue.push(...neighbours);
      }
      if (!clustered[j]) {
        clustered[j] = 1;
        members.push(points[j]!);
      }
    }
    clusters.push(members);
  }
  return clusters;
}
