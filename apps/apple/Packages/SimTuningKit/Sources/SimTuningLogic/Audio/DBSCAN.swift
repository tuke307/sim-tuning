import Foundation

public protocol Point2D: Sendable {
    var x: Double { get }
    var y: Double { get }
}

/// Port of `packages/common/src/audio/dbscan.ts` — DBSCAN as implemented by the `DBSCAN` NuGet
/// package v2 used (viceroypenguin/DBSCAN, MIT): linear neighbour search with a strict
/// `distance < epsilon` (the point itself included), clusters seeded in input order and expanded
/// breadth-first. Noise points are dropped. A uniform grid (cell = epsilon) accelerates the search
/// but candidates are returned in input order, so results equal the O(N²) list search.
public enum DBSCAN {
    private struct Cell: Hashable {
        let x: Int
        let y: Int
    }

    public static func cluster<T: Point2D>(_ points: [T], epsilon: Double, minimumPointsPerCluster: Int) -> [[T]] {
        let n = points.count
        var visited = [Bool](repeating: false, count: n)
        var clustered = [Bool](repeating: false, count: n)

        func cellOf(_ p: T) -> Cell {
            Cell(x: Int((p.x / epsilon).rounded(.down)), y: Int((p.y / epsilon).rounded(.down)))
        }
        var cells: [Cell: [Int]] = [:]
        for (i, p) in points.enumerated() {
            cells[cellOf(p), default: []].append(i)
        }

        func search(_ index: Int) -> [Int] {
            let p = points[index]
            let c = cellOf(p)
            var found: [Int] = []
            for dx in -1...1 {
                for dy in -1...1 {
                    guard let bucket = cells[Cell(x: c.x + dx, y: c.y + dy)] else { continue }
                    for j in bucket {
                        let q = points[j]
                        let ddx = q.x - p.x
                        let ddy = q.y - p.y
                        if (ddx * ddx + ddy * ddy).squareRoot() < epsilon { found.append(j) }
                    }
                }
            }
            return found.sorted()
        }

        var clusters: [[T]] = []
        for i in 0..<n {
            if visited[i] { continue }
            visited[i] = true
            let candidates = search(i)
            if candidates.count < minimumPointsPerCluster { continue }

            var members: [T] = [points[i]]
            clustered[i] = true
            var queue = candidates
            var head = 0
            while head < queue.count {
                let j = queue[head]
                if !visited[j] {
                    visited[j] = true
                    let neighbours = search(j)
                    if neighbours.count >= minimumPointsPerCluster { queue.append(contentsOf: neighbours) }
                }
                if !clustered[j] {
                    clustered[j] = true
                    members.append(points[j])
                }
                head += 1
            }
            clusters.append(members)
        }
        return clusters
    }
}
