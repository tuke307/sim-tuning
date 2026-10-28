# SimTuning domain API contract (v3)

Single source for the REST surface implemented by `apps/api` and consumed by `apps/web`
(`lib/data/api-repository.ts`) and `apps/apple` (`SimTuningKit`). Payload and response types are
the Zod schemas / DTOs in `packages/common/src/schemas/sim.ts`. All routes are under `/api`, return
the standard `ApiResponse<T>` envelope (`{ success, data, message? }`) and require authentication
(cookie + CSRF for browsers, bearer for iOS). Values are always in field base units.

## Ownership rules (deny by default)

- Rows with `userId = null` and a `systemKey` are **shared starter content** (v2 seed). Every
  signed-in user can read them; nobody can update or delete them through the API.
- All other rows belong to exactly one user. Reading/updating/deleting another user's row returns
  `404` (never `403`, so ids cannot be probed).
- A dyno belongs to the vehicle's owner. Dynos of system vehicles cannot be created.

## Vehicles

| Method | Path                      | Body                                                                    | Response                                                                                                       |
| ------ | ------------------------- | ----------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| GET    | `/vehicles`               | –                                                                       | `VehicleDto[]` (system vehicles first, then own, by name)                                                      |
| GET    | `/vehicles/:id`           | –                                                                       | `VehicleDto`                                                                                                   |
| POST   | `/vehicles`               | `VehicleInputDto`                                                       | `VehicleDto` (creates motor + einlass/vergaser + auslass/auspuff + ueberstroemer, missing parts created empty) |
| PATCH  | `/vehicles/:id`           | `VehicleInputDto` (partial nested allowed: only provided fields change) | `VehicleDto`                                                                                                   |
| DELETE | `/vehicles/:id`           | –                                                                       | `null` (cascades motor parts and dyno)                                                                         |
| POST   | `/vehicles/:id/duplicate` | `{ name?: string }`                                                     | `VehicleDto` (deep copy owned by the caller; works for system vehicles)                                        |

`VehicleDto.motor` is always fully nested (`einlass.vergaser`, `auslass.auspuff`, `ueberstroemer`).

## Environments

| Method | Path                | Body                           | Response                                                |
| ------ | ------------------- | ------------------------------ | ------------------------------------------------------- |
| GET    | `/environments`     | –                              | `EnvironmentDto[]` (system first)                       |
| POST   | `/environments`     | `EnvironmentInputDto`          | `EnvironmentDto`                                        |
| PATCH  | `/environments/:id` | `Partial<EnvironmentInputDto>` | `EnvironmentDto`                                        |
| DELETE | `/environments/:id` | –                              | `null` (dynos keep running with `environmentId = null`) |

## Dyno

One dyno per vehicle (v2 1:1).

| Method | Path                        | Body               | Response                                                                                      |
| ------ | --------------------------- | ------------------ | --------------------------------------------------------------------------------------------- |
| GET    | `/vehicles/:vehicleId/dyno` | –                  | `DynoDto \| null`                                                                             |
| PUT    | `/vehicles/:vehicleId/dyno` | `DynoInputDto`     | `DynoDto` (create or update)                                                                  |
| PUT    | `/dynos/:id/data`           | `DynoDataInputDto` | `DynoDto` (each provided array **replaces** the stored series, in order; omitted arrays stay) |
| DELETE | `/dynos/:id`                | –                  | `null`                                                                                        |
| GET    | `/dynos/:id/export`         | –                  | `DynoExportDto`                                                                               |
| POST   | `/dynos/import`             | `DynoExportDto`    | `VehicleDto` (creates a new owned vehicle with the imported dyno)                             |

Limits: max `MAX_DYNO_POINTS` (20 000) entries per series; request bodies up to 5 MB.
