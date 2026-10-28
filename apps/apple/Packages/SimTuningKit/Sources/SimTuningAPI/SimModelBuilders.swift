import Foundation

/// Pure builders/mergers that turn contract inputs into fully nested DTOs — Swift port of
/// `apps/web/lib/data/model.ts`. Used by the offline (guest) store; the API produces the same
/// shapes server-side.
public enum SimModel {
    public typealias IdFactory = (String) -> String

    public static func generateId(_ prefix: String) -> String {
        "\(prefix)-\(UUID().uuidString.lowercased())"
    }

    /// Applies a tri-state patch: `nil` keeps, `.some(nil)` clears, `.some(v)` sets.
    @inline(__always)
    static func apply<T>(_ target: inout T?, _ patch: T??) {
        if let value = patch { target = value }
    }

    public static func emptyMotor(id: IdFactory, name: String = "Motor", at: String = ISO8601.string()) -> MotorDto {
        let vergaser = VergaserDto(id: id("verg"), createdAt: at, updatedAt: at)
        let einlass = EinlassDto(id: id("einl"), createdAt: at, updatedAt: at, vergaser: vergaser)
        let auspuff = AuspuffDto(id: id("ausp"), createdAt: at, updatedAt: at, diffusorStage: 0)
        let auslass = AuslassDto(id: id("ausl"), createdAt: at, updatedAt: at, auspuff: auspuff)
        let ueberstroemer = UeberstroemerDto(id: id("ueb"), createdAt: at, updatedAt: at)
        return MotorDto(id: id("mot"), createdAt: at, updatedAt: at, name: name, einlass: einlass,
                        auslass: auslass, ueberstroemer: ueberstroemer)
    }

    static func merge(_ base: VergaserDto, _ input: VergaserInputDto) -> VergaserDto {
        var next = base
        apply(&next.benzinLuftF, input.benzinLuftF)
        apply(&next.durchmesserD, input.durchmesserD)
        return next
    }

    static func merge(_ base: EinlassDto, _ input: EinlassInputDto) -> EinlassDto {
        var next = base
        apply(&next.breiteB, input.breiteB)
        apply(&next.durchmesserD, input.durchmesserD)
        apply(&next.flaecheA, input.flaecheA)
        apply(&next.hoeheH, input.hoeheH)
        apply(&next.laengeL, input.laengeL)
        apply(&next.luftBedarf, input.luftBedarf)
        apply(&next.steuerzeitSZ, input.steuerzeitSZ)
        return next
    }

    static func merge(_ base: AuspuffDto, _ input: AuspuffInputDto) -> AuspuffDto {
        var next = base
        apply(&next.abgasT, input.abgasT)
        apply(&next.abgasV, input.abgasV)
        apply(&next.diffusorD, input.diffusorD)
        apply(&next.diffusorD1, input.diffusorD1)
        apply(&next.diffusorD2, input.diffusorD2)
        apply(&next.diffusorD3, input.diffusorD3)
        apply(&next.diffusorL, input.diffusorL)
        apply(&next.diffusorL1, input.diffusorL1)
        apply(&next.diffusorL2, input.diffusorL2)
        apply(&next.diffusorL3, input.diffusorL3)
        if let stage = input.diffusorStage { next.diffusorStage = stage }
        apply(&next.diffusorW, input.diffusorW)
        apply(&next.diffusorW1, input.diffusorW1)
        apply(&next.diffusorW2, input.diffusorW2)
        apply(&next.diffusorW3, input.diffusorW3)
        apply(&next.endrohrD, input.endrohrD)
        apply(&next.endrohrL, input.endrohrL)
        apply(&next.gegenkonusD, input.gegenkonusD)
        apply(&next.gegenkonusL, input.gegenkonusL)
        apply(&next.gegenKonusW, input.gegenKonusW)
        apply(&next.gesamtL, input.gesamtL)
        apply(&next.kruemmerD, input.kruemmerD)
        apply(&next.kruemmerF, input.kruemmerF)
        apply(&next.kruemmerL, input.kruemmerL)
        apply(&next.kruemmerW, input.kruemmerW)
        apply(&next.mittelteilD, input.mittelteilD)
        apply(&next.mittelteilF, input.mittelteilF)
        apply(&next.mittelteilL, input.mittelteilL)
        apply(&next.resonanzL, input.resonanzL)
        return next
    }

    static func merge(_ base: AuslassDto, _ input: AuslassInputDto) -> AuslassDto {
        var next = base
        apply(&next.breiteB, input.breiteB)
        apply(&next.durchmesserD, input.durchmesserD)
        apply(&next.flaecheA, input.flaecheA)
        apply(&next.hoeheH, input.hoeheH)
        apply(&next.laengeL, input.laengeL)
        apply(&next.steuerzeitSZ, input.steuerzeitSZ)
        return next
    }

    static func merge(_ base: UeberstroemerDto, _ input: UeberstroemerInputDto) -> UeberstroemerDto {
        var next = base
        apply(&next.anzahl, input.anzahl)
        apply(&next.breiteB, input.breiteB)
        apply(&next.flaecheA, input.flaecheA)
        apply(&next.hoeheH, input.hoeheH)
        apply(&next.steuerzeitSZ, input.steuerzeitSZ)
        return next
    }

    /// Applies a (partial) motor input onto a motor DTO, creating missing nested parts.
    public static func mergeMotor(_ base: MotorDto, _ input: MotorInputDto?, id: IdFactory) -> MotorDto {
        guard let input else { return base }
        let at = ISO8601.string()
        let empty = emptyMotor(id: id, name: base.name, at: at)
        let einlassBase = base.einlass ?? empty.einlass!
        let auslassBase = base.auslass ?? empty.auslass!
        var einlass = einlassBase
        if let e = input.einlass {
            einlass = merge(einlassBase, e)
            if let v = e.vergaser {
                var vergaser = merge(einlassBase.vergaser ?? empty.einlass!.vergaser!, v)
                vergaser.updatedAt = at
                einlass.vergaser = vergaser
            }
            einlass.updatedAt = at
        }
        var auslass = auslassBase
        if let a = input.auslass {
            auslass = merge(auslassBase, a)
            if let p = a.auspuff {
                var auspuff = merge(auslassBase.auspuff ?? empty.auslass!.auspuff!, p)
                auspuff.updatedAt = at
                auslass.auspuff = auspuff
            }
            auslass.updatedAt = at
        }
        let ueberstroemerBase = base.ueberstroemer ?? empty.ueberstroemer!
        var ueberstroemer = ueberstroemerBase
        if let u = input.ueberstroemer {
            ueberstroemer = merge(ueberstroemerBase, u)
            ueberstroemer.updatedAt = at
        }

        var next = base
        if let name = input.name { next.name = name }
        apply(&next.bohrungD, input.bohrungD)
        apply(&next.brennraumV, input.brennraumV)
        apply(&next.deachsierungL, input.deachsierungL)
        apply(&next.heizwertU, input.heizwertU)
        apply(&next.hubL, input.hubL)
        apply(&next.hubraumV, input.hubraumV)
        apply(&next.kolbenG, input.kolbenG)
        apply(&next.kurbelgehaeuseV, input.kurbelgehaeuseV)
        apply(&next.pleulL, input.pleulL)
        apply(&next.resonanzU, input.resonanzU)
        apply(&next.verdichtungV, input.verdichtungV)
        apply(&next.zuendzeitpunkt, input.zuendzeitpunkt)
        apply(&next.zylinderAnz, input.zylinderAnz)
        next.einlass = einlass
        next.auslass = auslass
        next.ueberstroemer = ueberstroemer
        next.updatedAt = at
        return next
    }

    public static func buildVehicle(_ input: VehicleInputDto, id: IdFactory, systemKey: String? = nil,
                                    dynoId: String? = nil) -> VehicleDto {
        let at = ISO8601.string()
        let vehicleId = id("veh")
        let name = input.name ?? ""
        let motor = mergeMotor(emptyMotor(id: id, name: input.motor?.name ?? name, at: at), input.motor, id: id)
        return VehicleDto(
            id: vehicleId, createdAt: at, updatedAt: at, systemKey: systemKey, isSystem: systemKey != nil,
            name: name, beschreibung: input.beschreibung ?? nil, gewicht: input.gewicht ?? nil,
            motor: motor, dynoId: dynoId
        )
    }

    public static func mergeVehicle(_ base: VehicleDto, _ input: VehicleInputDto, id: IdFactory) -> VehicleDto {
        var next = base
        if let name = input.name { next.name = name }
        apply(&next.beschreibung, input.beschreibung)
        apply(&next.gewicht, input.gewicht)
        next.motor = mergeMotor(base.motor ?? emptyMotor(id: id, name: base.name), input.motor, id: id)
        next.updatedAt = ISO8601.string()
        return next
    }

    /// Converts a vehicle DTO back into an input payload (for duplicate/export). Every field is
    /// set explicitly (value or `null`), like the TS `strip` helper.
    public static func vehicleToInput(_ vehicle: VehicleDto) -> VehicleInputDto {
        var input = VehicleInputDto(name: vehicle.name, beschreibung: .some(vehicle.beschreibung),
                                    gewicht: .some(vehicle.gewicht))
        guard let m = vehicle.motor else { return input }
        var motor = MotorInputDto(name: m.name)
        motor.bohrungD = .some(m.bohrungD)
        motor.brennraumV = .some(m.brennraumV)
        motor.deachsierungL = .some(m.deachsierungL)
        motor.heizwertU = .some(m.heizwertU)
        motor.hubL = .some(m.hubL)
        motor.hubraumV = .some(m.hubraumV)
        motor.kolbenG = .some(m.kolbenG)
        motor.kurbelgehaeuseV = .some(m.kurbelgehaeuseV)
        motor.pleulL = .some(m.pleulL)
        motor.resonanzU = .some(m.resonanzU)
        motor.verdichtungV = .some(m.verdichtungV)
        motor.zuendzeitpunkt = .some(m.zuendzeitpunkt)
        motor.zylinderAnz = .some(m.zylinderAnz)
        if let e = m.einlass {
            motor.einlass = EinlassInputDto(
                breiteB: .some(e.breiteB), durchmesserD: .some(e.durchmesserD), flaecheA: .some(e.flaecheA),
                hoeheH: .some(e.hoeheH), laengeL: .some(e.laengeL), luftBedarf: .some(e.luftBedarf),
                steuerzeitSZ: .some(e.steuerzeitSZ),
                vergaser: e.vergaser.map {
                    VergaserInputDto(benzinLuftF: .some($0.benzinLuftF), durchmesserD: .some($0.durchmesserD))
                }
            )
        }
        if let a = m.auslass {
            motor.auslass = AuslassInputDto(
                breiteB: .some(a.breiteB), durchmesserD: .some(a.durchmesserD), flaecheA: .some(a.flaecheA),
                hoeheH: .some(a.hoeheH), laengeL: .some(a.laengeL), steuerzeitSZ: .some(a.steuerzeitSZ),
                auspuff: a.auspuff.map(auspuffToInput)
            )
        }
        if let u = m.ueberstroemer {
            motor.ueberstroemer = UeberstroemerInputDto(
                anzahl: .some(u.anzahl), breiteB: .some(u.breiteB), flaecheA: .some(u.flaecheA),
                hoeheH: .some(u.hoeheH), steuerzeitSZ: .some(u.steuerzeitSZ)
            )
        }
        input.motor = motor
        return input
    }

    static func auspuffToInput(_ p: AuspuffDto) -> AuspuffInputDto {
        var i = AuspuffInputDto()
        i.abgasT = .some(p.abgasT)
        i.abgasV = .some(p.abgasV)
        i.diffusorD = .some(p.diffusorD)
        i.diffusorD1 = .some(p.diffusorD1)
        i.diffusorD2 = .some(p.diffusorD2)
        i.diffusorD3 = .some(p.diffusorD3)
        i.diffusorL = .some(p.diffusorL)
        i.diffusorL1 = .some(p.diffusorL1)
        i.diffusorL2 = .some(p.diffusorL2)
        i.diffusorL3 = .some(p.diffusorL3)
        i.diffusorStage = p.diffusorStage
        i.diffusorW = .some(p.diffusorW)
        i.diffusorW1 = .some(p.diffusorW1)
        i.diffusorW2 = .some(p.diffusorW2)
        i.diffusorW3 = .some(p.diffusorW3)
        i.endrohrD = .some(p.endrohrD)
        i.endrohrL = .some(p.endrohrL)
        i.gegenkonusD = .some(p.gegenkonusD)
        i.gegenkonusL = .some(p.gegenkonusL)
        i.gegenKonusW = .some(p.gegenKonusW)
        i.gesamtL = .some(p.gesamtL)
        i.kruemmerD = .some(p.kruemmerD)
        i.kruemmerF = .some(p.kruemmerF)
        i.kruemmerL = .some(p.kruemmerL)
        i.kruemmerW = .some(p.kruemmerW)
        i.mittelteilD = .some(p.mittelteilD)
        i.mittelteilF = .some(p.mittelteilF)
        i.mittelteilL = .some(p.mittelteilL)
        i.resonanzL = .some(p.resonanzL)
        return i
    }

    public static func buildEnvironment(_ input: EnvironmentInputDto, id: IdFactory) -> EnvironmentDto {
        let at = ISO8601.string()
        return EnvironmentDto(id: id("env"), createdAt: at, updatedAt: at, systemKey: nil, isSystem: false,
                              name: input.name ?? "", temperaturT: input.temperaturT ?? nil,
                              luftdruckP: input.luftdruckP ?? nil)
    }

    public static func upsertDyno(_ existing: DynoDto?, vehicleId: String, input: DynoInputDto,
                                  id: IdFactory) -> DynoDto {
        let at = ISO8601.string()
        var dyno = existing ?? DynoDto(id: id("dyno"), createdAt: at, updatedAt: at, vehicleId: vehicleId,
                                       environmentId: nil, name: input.name, beschreibung: nil, active: true)
        dyno.name = input.name
        apply(&dyno.beschreibung, input.beschreibung)
        if let active = input.active { dyno.active = active }
        apply(&dyno.environmentId, input.environmentId)
        dyno.updatedAt = at
        return dyno
    }

    /// Each provided series replaces the stored one; omitted series stay untouched.
    public static func applyDynoData(_ dyno: DynoDto, _ data: DynoDataInputDto) -> DynoDto {
        var next = dyno
        if let drehzahl = data.drehzahl { next.drehzahl = drehzahl }
        if let dynoPs = data.dynoPs { next.dynoPs = dynoPs }
        if let ausrollen = data.ausrollen { next.ausrollen = ausrollen }
        if let geschwindigkeit = data.geschwindigkeit { next.geschwindigkeit = geschwindigkeit }
        next.updatedAt = ISO8601.string()
        return next
    }

    public static func buildDynoExport(vehicle: VehicleDto, dyno: DynoDto) -> DynoExportDto {
        DynoExportDto(
            exportedAt: ISO8601.string(),
            vehicle: vehicleToInput(vehicle),
            dyno: .init(name: dyno.name, beschreibung: .some(dyno.beschreibung), active: dyno.active,
                        drehzahl: dyno.drehzahl, dynoPs: dyno.dynoPs, ausrollen: dyno.ausrollen,
                        geschwindigkeit: dyno.geschwindigkeit)
        )
    }
}

// MARK: - Memberwise initialisers for nested DTOs (all values default to null)

public extension VergaserDto {
    init(id: String, createdAt: String, updatedAt: String) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, benzinLuftF: nil, durchmesserD: nil)
    }
}

public extension EinlassDto {
    init(id: String, createdAt: String, updatedAt: String, vergaser: VergaserDto?) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, breiteB: nil, durchmesserD: nil,
                  flaecheA: nil, hoeheH: nil, laengeL: nil, luftBedarf: nil, steuerzeitSZ: nil, vergaser: vergaser)
    }
}

public extension AuspuffDto {
    init(id: String, createdAt: String, updatedAt: String, diffusorStage: Int) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, abgasT: nil, abgasV: nil, diffusorD: nil,
                  diffusorD1: nil, diffusorD2: nil, diffusorD3: nil, diffusorL: nil, diffusorL1: nil,
                  diffusorL2: nil, diffusorL3: nil, diffusorStage: diffusorStage, diffusorW: nil,
                  diffusorW1: nil, diffusorW2: nil, diffusorW3: nil, endrohrD: nil, endrohrL: nil,
                  gegenkonusD: nil, gegenkonusL: nil, gegenKonusW: nil, gesamtL: nil, kruemmerD: nil,
                  kruemmerF: nil, kruemmerL: nil, kruemmerW: nil, mittelteilD: nil, mittelteilF: nil,
                  mittelteilL: nil, resonanzL: nil)
    }
}

public extension AuslassDto {
    init(id: String, createdAt: String, updatedAt: String, auspuff: AuspuffDto?) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, breiteB: nil, durchmesserD: nil,
                  flaecheA: nil, hoeheH: nil, laengeL: nil, steuerzeitSZ: nil, auspuff: auspuff)
    }
}

public extension UeberstroemerDto {
    init(id: String, createdAt: String, updatedAt: String) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, anzahl: nil, breiteB: nil, flaecheA: nil,
                  hoeheH: nil, steuerzeitSZ: nil)
    }
}

public extension MotorDto {
    init(id: String, createdAt: String, updatedAt: String, name: String, einlass: EinlassDto?,
         auslass: AuslassDto?, ueberstroemer: UeberstroemerDto?) {
        self.init(id: id, createdAt: createdAt, updatedAt: updatedAt, name: name, bohrungD: nil, brennraumV: nil,
                  deachsierungL: nil, heizwertU: nil, hubL: nil, hubraumV: nil, kolbenG: nil,
                  kurbelgehaeuseV: nil, pleulL: nil, resonanzU: nil, verdichtungV: nil, zuendzeitpunkt: nil,
                  zylinderAnz: nil, einlass: einlass, auslass: auslass, ueberstroemer: ueberstroemer)
    }
}
