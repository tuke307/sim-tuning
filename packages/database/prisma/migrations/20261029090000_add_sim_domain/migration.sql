-- CreateTable
CREATE TABLE "Environment" (
    "id" TEXT NOT NULL,
    "userId" TEXT,
    "systemKey" TEXT,
    "name" TEXT NOT NULL,
    "temperaturT" DOUBLE PRECISION,
    "luftdruckP" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Environment_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Vehicle" (
    "id" TEXT NOT NULL,
    "userId" TEXT,
    "systemKey" TEXT,
    "name" TEXT NOT NULL,
    "beschreibung" TEXT,
    "gewicht" DOUBLE PRECISION,
    "motorId" TEXT,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Vehicle_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Motor" (
    "id" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "bohrungD" DOUBLE PRECISION,
    "brennraumV" DOUBLE PRECISION,
    "deachsierungL" DOUBLE PRECISION,
    "heizwertU" DOUBLE PRECISION,
    "hubL" DOUBLE PRECISION,
    "hubraumV" DOUBLE PRECISION,
    "kolbenG" DOUBLE PRECISION,
    "kurbelgehaeuseV" DOUBLE PRECISION,
    "pleulL" DOUBLE PRECISION,
    "resonanzU" DOUBLE PRECISION,
    "verdichtungV" DOUBLE PRECISION,
    "zuendzeitpunkt" DOUBLE PRECISION,
    "zylinderAnz" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Motor_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Einlass" (
    "id" TEXT NOT NULL,
    "motorId" TEXT NOT NULL,
    "breiteB" DOUBLE PRECISION,
    "durchmesserD" DOUBLE PRECISION,
    "flaecheA" DOUBLE PRECISION,
    "hoeheH" DOUBLE PRECISION,
    "laengeL" DOUBLE PRECISION,
    "luftBedarf" DOUBLE PRECISION,
    "steuerzeitSZ" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Einlass_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Vergaser" (
    "id" TEXT NOT NULL,
    "einlassId" TEXT NOT NULL,
    "benzinLuftF" DOUBLE PRECISION,
    "durchmesserD" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Vergaser_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Auslass" (
    "id" TEXT NOT NULL,
    "motorId" TEXT NOT NULL,
    "breiteB" DOUBLE PRECISION,
    "durchmesserD" DOUBLE PRECISION,
    "flaecheA" DOUBLE PRECISION,
    "hoeheH" DOUBLE PRECISION,
    "laengeL" DOUBLE PRECISION,
    "steuerzeitSZ" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Auslass_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Auspuff" (
    "id" TEXT NOT NULL,
    "auslassId" TEXT NOT NULL,
    "abgasT" DOUBLE PRECISION,
    "abgasV" DOUBLE PRECISION,
    "diffusorD" DOUBLE PRECISION,
    "diffusorD1" DOUBLE PRECISION,
    "diffusorD2" DOUBLE PRECISION,
    "diffusorD3" DOUBLE PRECISION,
    "diffusorL" DOUBLE PRECISION,
    "diffusorL1" DOUBLE PRECISION,
    "diffusorL2" DOUBLE PRECISION,
    "diffusorL3" DOUBLE PRECISION,
    "diffusorStage" INTEGER NOT NULL DEFAULT 0,
    "diffusorW" DOUBLE PRECISION,
    "diffusorW1" DOUBLE PRECISION,
    "diffusorW2" DOUBLE PRECISION,
    "diffusorW3" DOUBLE PRECISION,
    "endrohrD" DOUBLE PRECISION,
    "endrohrL" DOUBLE PRECISION,
    "gegenkonusD" DOUBLE PRECISION,
    "gegenkonusL" DOUBLE PRECISION,
    "gegenKonusW" DOUBLE PRECISION,
    "gesamtL" DOUBLE PRECISION,
    "kruemmerD" DOUBLE PRECISION,
    "kruemmerF" DOUBLE PRECISION,
    "kruemmerL" DOUBLE PRECISION,
    "kruemmerW" DOUBLE PRECISION,
    "mittelteilD" DOUBLE PRECISION,
    "mittelteilF" DOUBLE PRECISION,
    "mittelteilL" DOUBLE PRECISION,
    "resonanzL" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Auspuff_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Ueberstroemer" (
    "id" TEXT NOT NULL,
    "motorId" TEXT NOT NULL,
    "anzahl" DOUBLE PRECISION,
    "breiteB" DOUBLE PRECISION,
    "flaecheA" DOUBLE PRECISION,
    "hoeheH" DOUBLE PRECISION,
    "steuerzeitSZ" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Ueberstroemer_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Dyno" (
    "id" TEXT NOT NULL,
    "vehicleId" TEXT NOT NULL,
    "environmentId" TEXT,
    "name" TEXT NOT NULL,
    "beschreibung" TEXT,
    "active" BOOLEAN NOT NULL DEFAULT true,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Dyno_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Drehzahl" (
    "id" TEXT NOT NULL,
    "dynoId" TEXT NOT NULL,
    "position" INTEGER NOT NULL,
    "zeit" DOUBLE PRECISION NOT NULL,
    "drehzahl" DOUBLE PRECISION NOT NULL,

    CONSTRAINT "Drehzahl_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "DynoPs" (
    "id" TEXT NOT NULL,
    "dynoId" TEXT NOT NULL,
    "position" INTEGER NOT NULL,
    "drehzahl" DOUBLE PRECISION NOT NULL,
    "ps" DOUBLE PRECISION NOT NULL,

    CONSTRAINT "DynoPs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Ausrollen" (
    "id" TEXT NOT NULL,
    "dynoId" TEXT NOT NULL,
    "position" INTEGER NOT NULL,
    "latitude" DOUBLE PRECISION NOT NULL,
    "longitude" DOUBLE PRECISION NOT NULL,
    "speed" DOUBLE PRECISION,
    "accuracy" DOUBLE PRECISION,
    "altitude" DOUBLE PRECISION,
    "altitudeAccuracy" DOUBLE PRECISION,
    "heading" DOUBLE PRECISION,
    "headingAccuracy" DOUBLE PRECISION,
    "zeitpunkt" TIMESTAMP(3),

    CONSTRAINT "Ausrollen_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Geschwindigkeit" (
    "id" TEXT NOT NULL,
    "dynoId" TEXT NOT NULL,
    "position" INTEGER NOT NULL,
    "latitude" DOUBLE PRECISION NOT NULL,
    "longitude" DOUBLE PRECISION NOT NULL,
    "speed" DOUBLE PRECISION,
    "accuracy" DOUBLE PRECISION,
    "altitude" DOUBLE PRECISION,
    "altitudeAccuracy" DOUBLE PRECISION,
    "heading" DOUBLE PRECISION,
    "headingAccuracy" DOUBLE PRECISION,
    "zeitpunkt" TIMESTAMP(3),

    CONSTRAINT "Geschwindigkeit_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "Environment_systemKey_key" ON "Environment"("systemKey");

-- CreateIndex
CREATE INDEX "Environment_userId_idx" ON "Environment"("userId");

-- CreateIndex
CREATE UNIQUE INDEX "Vehicle_systemKey_key" ON "Vehicle"("systemKey");

-- CreateIndex
CREATE UNIQUE INDEX "Vehicle_motorId_key" ON "Vehicle"("motorId");

-- CreateIndex
CREATE INDEX "Vehicle_userId_idx" ON "Vehicle"("userId");

-- CreateIndex
CREATE UNIQUE INDEX "Einlass_motorId_key" ON "Einlass"("motorId");

-- CreateIndex
CREATE UNIQUE INDEX "Vergaser_einlassId_key" ON "Vergaser"("einlassId");

-- CreateIndex
CREATE UNIQUE INDEX "Auslass_motorId_key" ON "Auslass"("motorId");

-- CreateIndex
CREATE UNIQUE INDEX "Auspuff_auslassId_key" ON "Auspuff"("auslassId");

-- CreateIndex
CREATE UNIQUE INDEX "Ueberstroemer_motorId_key" ON "Ueberstroemer"("motorId");

-- CreateIndex
CREATE UNIQUE INDEX "Dyno_vehicleId_key" ON "Dyno"("vehicleId");

-- CreateIndex
CREATE INDEX "Drehzahl_dynoId_position_idx" ON "Drehzahl"("dynoId", "position");

-- CreateIndex
CREATE INDEX "DynoPs_dynoId_position_idx" ON "DynoPs"("dynoId", "position");

-- CreateIndex
CREATE INDEX "Ausrollen_dynoId_position_idx" ON "Ausrollen"("dynoId", "position");

-- CreateIndex
CREATE INDEX "Geschwindigkeit_dynoId_position_idx" ON "Geschwindigkeit"("dynoId", "position");

-- AddForeignKey
ALTER TABLE "Environment" ADD CONSTRAINT "Environment_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Vehicle" ADD CONSTRAINT "Vehicle_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Vehicle" ADD CONSTRAINT "Vehicle_motorId_fkey" FOREIGN KEY ("motorId") REFERENCES "Motor"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Einlass" ADD CONSTRAINT "Einlass_motorId_fkey" FOREIGN KEY ("motorId") REFERENCES "Motor"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Vergaser" ADD CONSTRAINT "Vergaser_einlassId_fkey" FOREIGN KEY ("einlassId") REFERENCES "Einlass"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Auslass" ADD CONSTRAINT "Auslass_motorId_fkey" FOREIGN KEY ("motorId") REFERENCES "Motor"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Auspuff" ADD CONSTRAINT "Auspuff_auslassId_fkey" FOREIGN KEY ("auslassId") REFERENCES "Auslass"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Ueberstroemer" ADD CONSTRAINT "Ueberstroemer_motorId_fkey" FOREIGN KEY ("motorId") REFERENCES "Motor"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Dyno" ADD CONSTRAINT "Dyno_vehicleId_fkey" FOREIGN KEY ("vehicleId") REFERENCES "Vehicle"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Dyno" ADD CONSTRAINT "Dyno_environmentId_fkey" FOREIGN KEY ("environmentId") REFERENCES "Environment"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Drehzahl" ADD CONSTRAINT "Drehzahl_dynoId_fkey" FOREIGN KEY ("dynoId") REFERENCES "Dyno"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "DynoPs" ADD CONSTRAINT "DynoPs_dynoId_fkey" FOREIGN KEY ("dynoId") REFERENCES "Dyno"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Ausrollen" ADD CONSTRAINT "Ausrollen_dynoId_fkey" FOREIGN KEY ("dynoId") REFERENCES "Dyno"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Geschwindigkeit" ADD CONSTRAINT "Geschwindigkeit_dynoId_fkey" FOREIGN KEY ("dynoId") REFERENCES "Dyno"("id") ON DELETE CASCADE ON UPDATE CASCADE;

