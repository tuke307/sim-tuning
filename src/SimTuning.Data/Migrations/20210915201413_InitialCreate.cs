// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace SimTuning.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auspuff");

            migrationBuilder.DropTable(
                name: "Ausrollen");

            migrationBuilder.DropTable(
                name: "DynoAudio");

            migrationBuilder.DropTable(
                name: "DynoNm");

            migrationBuilder.DropTable(
                name: "DynoPs");

            migrationBuilder.DropTable(
                name: "Geschwindigkeit");

            migrationBuilder.DropTable(
                name: "MotorUeberstroemer");

            migrationBuilder.DropTable(
                name: "TuningPs");

            migrationBuilder.DropTable(
                name: "Vergaser");

            migrationBuilder.DropTable(
                name: "MotorAuslass");

            migrationBuilder.DropTable(
                name: "Dyno");

            migrationBuilder.DropTable(
                name: "Tuning");

            migrationBuilder.DropTable(
                name: "MotorEinlass");

            migrationBuilder.DropTable(
                name: "Environment");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Motor");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Environment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    LuftdruckP = table.Column<double>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    TemperaturT = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    BohrungD = table.Column<double>(nullable: true),
                    BrennraumV = table.Column<double>(nullable: true),
                    DeachsierungL = table.Column<double>(nullable: true),
                    HeizwertU = table.Column<double>(nullable: true),
                    HubL = table.Column<double>(nullable: true),
                    HubraumV = table.Column<double>(nullable: true),
                    KolbenG = table.Column<double>(nullable: true),
                    KurbelgehaeuseV = table.Column<double>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    PleulL = table.Column<double>(nullable: true),
                    ResonanzU = table.Column<double>(nullable: true),
                    VerdichtungV = table.Column<double>(nullable: true),
                    Zuendzeitpunkt = table.Column<double>(nullable: true),
                    ZylinderAnz = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotorAuslass",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    BreiteB = table.Column<double>(nullable: true),
                    DurchmesserD = table.Column<double>(nullable: true),
                    FlaecheA = table.Column<double>(nullable: true),
                    HoeheH = table.Column<double>(nullable: true),
                    LaengeL = table.Column<double>(nullable: true),
                    MotorId = table.Column<int>(nullable: false),
                    SteuerzeitSZ = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorAuslass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorAuslass_Motor_MotorId",
                        column: x => x.MotorId,
                        principalTable: "Motor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotorEinlass",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    BreiteB = table.Column<double>(nullable: true),
                    DurchmesserD = table.Column<double>(nullable: true),
                    FlaecheA = table.Column<double>(nullable: true),
                    HoeheH = table.Column<double>(nullable: true),
                    LaengeL = table.Column<double>(nullable: true),
                    LuftBedarf = table.Column<double>(nullable: true),
                    MotorId = table.Column<int>(nullable: false),
                    SteuerzeitSZ = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorEinlass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorEinlass_Motor_MotorId",
                        column: x => x.MotorId,
                        principalTable: "Motor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotorUeberstroemer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Anzahl = table.Column<double>(nullable: true),
                    BreiteB = table.Column<double>(nullable: true),
                    FlaecheA = table.Column<double>(nullable: true),
                    HoeheH = table.Column<double>(nullable: true),
                    MotorId = table.Column<int>(nullable: false),
                    SteuerzeitSZ = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorUeberstroemer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorUeberstroemer_Motor_MotorId",
                        column: x => x.MotorId,
                        principalTable: "Motor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Beschreibung = table.Column<string>(nullable: true),
                    Deletable = table.Column<bool>(nullable: false),
                    Gewicht = table.Column<double>(nullable: true),
                    MotorId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Motor_MotorId",
                        column: x => x.MotorId,
                        principalTable: "Motor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auspuff",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    AbgasT = table.Column<double>(nullable: true),
                    AbgasV = table.Column<double>(nullable: true),
                    AuslassId = table.Column<int>(nullable: false),
                    DiffusorD = table.Column<double>(nullable: true),
                    DiffusorD1 = table.Column<double>(nullable: true),
                    DiffusorD2 = table.Column<double>(nullable: true),
                    DiffusorD3 = table.Column<double>(nullable: true),
                    DiffusorL = table.Column<double>(nullable: true),
                    DiffusorL1 = table.Column<double>(nullable: true),
                    DiffusorL2 = table.Column<double>(nullable: true),
                    DiffusorL3 = table.Column<double>(nullable: true),
                    DiffusorStage = table.Column<int>(nullable: false),
                    DiffusorW = table.Column<double>(nullable: true),
                    DiffusorW1 = table.Column<double>(nullable: true),
                    DiffusorW2 = table.Column<double>(nullable: true),
                    DiffusorW3 = table.Column<double>(nullable: true),
                    EndrohrD = table.Column<double>(nullable: true),
                    EndrohrL = table.Column<double>(nullable: true),
                    GegenkonusD = table.Column<double>(nullable: true),
                    GegenkonusL = table.Column<double>(nullable: true),
                    GegenKonusW = table.Column<double>(nullable: true),
                    GesamtL = table.Column<double>(nullable: true),
                    KruemmerD = table.Column<double>(nullable: true),
                    KruemmerF = table.Column<double>(nullable: true),
                    KruemmerL = table.Column<double>(nullable: true),
                    KruemmerW = table.Column<double>(nullable: true),
                    MittelteilD = table.Column<double>(nullable: true),
                    MittelteilF = table.Column<double>(nullable: true),
                    MittelteilL = table.Column<double>(nullable: true),
                    ResonanzL = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auspuff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auspuff_MotorAuslass_AuslassId",
                        column: x => x.AuslassId,
                        principalTable: "MotorAuslass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vergaser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    BenzinLuftF = table.Column<double>(nullable: true),
                    DurchmesserD = table.Column<double>(nullable: true),
                    EinlassId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vergaser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vergaser_MotorEinlass_EinlassId",
                        column: x => x.EinlassId,
                        principalTable: "MotorEinlass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dyno",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Active = table.Column<bool>(nullable: true),
                    Beschreibung = table.Column<string>(nullable: true),
                    EnvironmentId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    VehicleId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dyno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dyno_Environment_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dyno_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tuning",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Active = table.Column<bool>(nullable: true),
                    Beschreibung = table.Column<string>(nullable: true),
                    DiagrammeU = table.Column<double>(nullable: true),
                    EnvironmentId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    VehicleId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tuning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tuning_Environment_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tuning_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ausrollen",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Accuracy = table.Column<double>(nullable: true),
                    Altitude = table.Column<double>(nullable: true),
                    AltitudeAccuracy = table.Column<double>(nullable: true),
                    DynoId = table.Column<int>(nullable: false),
                    Heading = table.Column<double>(nullable: true),
                    HeadingAccuracy = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ausrollen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ausrollen_Dyno_DynoId",
                        column: x => x.DynoId,
                        principalTable: "Dyno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynoAudio",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Drehzahl = table.Column<double>(nullable: false),
                    DynoId = table.Column<int>(nullable: true),
                    Zeit = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynoAudio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynoAudio_Dyno_DynoId",
                        column: x => x.DynoId,
                        principalTable: "Dyno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DynoNm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Drehzahl = table.Column<double>(nullable: false),
                    DynoId = table.Column<int>(nullable: true),
                    Nm = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynoNm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynoNm_Dyno_DynoId",
                        column: x => x.DynoId,
                        principalTable: "Dyno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DynoPs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Drehzahl = table.Column<double>(nullable: false),
                    DynoId = table.Column<int>(nullable: true),
                    Ps = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynoPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynoPs_Dyno_DynoId",
                        column: x => x.DynoId,
                        principalTable: "Dyno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Geschwindigkeit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Accuracy = table.Column<double>(nullable: true),
                    Altitude = table.Column<double>(nullable: true),
                    AltitudeAccuracy = table.Column<double>(nullable: true),
                    DynoId = table.Column<int>(nullable: false),
                    Heading = table.Column<double>(nullable: true),
                    HeadingAccuracy = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geschwindigkeit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Geschwindigkeit_Dyno_DynoId",
                        column: x => x.DynoId,
                        principalTable: "Dyno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuningPs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    TuningId = table.Column<int>(nullable: true),
                    X = table.Column<double>(nullable: false),
                    Y = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuningPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuningPs_Tuning_TuningId",
                        column: x => x.TuningId,
                        principalTable: "Tuning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Environment",
                columns: new[] { "Id", "CreatedDate", "LuftdruckP", "Name", "TemperaturT", "UpdatedDate" },
                values: new object[] { 1, null, 1010.0, "Frühling", 10.0, null });

            migrationBuilder.InsertData(
                table: "Environment",
                columns: new[] { "Id", "CreatedDate", "LuftdruckP", "Name", "TemperaturT", "UpdatedDate" },
                values: new object[] { 2, null, 1010.0, "Sommer", 25.0, null });

            migrationBuilder.InsertData(
                table: "Environment",
                columns: new[] { "Id", "CreatedDate", "LuftdruckP", "Name", "TemperaturT", "UpdatedDate" },
                values: new object[] { 3, null, 1010.0, "Herbst", 10.0, null });

            migrationBuilder.InsertData(
                table: "Environment",
                columns: new[] { "Id", "CreatedDate", "LuftdruckP", "Name", "TemperaturT", "UpdatedDate" },
                values: new object[] { 4, null, 1010.0, "Winter", 1.0, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 1, 38.0, null, null, null, null, 42.0, 47600.0, null, 142800.0, "Rh 50 II", null, 5000.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 2, 38.0, null, null, null, null, 42.0, 47600.0, null, 142800.0, "KRo Rh 50", null, 5500.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 3, 40.0, null, null, null, null, 39.5, 49600.0, null, 148800.0, "M53/1 KFR", null, 5750.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 4, 40.0, null, null, null, null, 39.5, 49600.0, null, 148800.0, "M 53/21 KF", null, 5500.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 5, 38.0, 5880.0, null, 2.0, null, 44.0, 49900.0, null, 149700.0, "M 541 KF", 95.0, 5500.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Motor",
                columns: new[] { "Id", "BohrungD", "BrennraumV", "CreatedDate", "DeachsierungL", "HeizwertU", "HubL", "HubraumV", "KolbenG", "KurbelgehaeuseV", "Name", "PleulL", "ResonanzU", "UpdatedDate", "VerdichtungV", "Zuendzeitpunkt", "ZylinderAnz" },
                values: new object[] { 6, 45.0, null, null, null, null, 44.0, 49900.0, null, 209700.0, "M 741/1 KF", null, 6000.0, null, null, null, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 1, null, null, null, null, null, null, 1, null, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 5, null, null, null, null, null, null, 5, 145.0, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 4, null, null, null, null, null, null, 4, null, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 2, null, null, null, null, null, null, 2, null, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 3, null, null, null, null, null, null, 3, 148.0, null });

            migrationBuilder.InsertData(
                table: "MotorAuslass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 6, null, null, null, null, null, null, 6, null, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 4, null, null, null, null, null, null, null, 4, null, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 5, null, null, null, null, null, null, null, 5, 135.0, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 3, null, null, null, null, null, null, null, 3, 136.0, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 2, null, null, null, null, null, null, null, 2, null, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 6, null, null, null, null, null, null, null, 6, null, null });

            migrationBuilder.InsertData(
                table: "MotorEinlass",
                columns: new[] { "Id", "BreiteB", "CreatedDate", "DurchmesserD", "FlaecheA", "HoeheH", "LaengeL", "LuftBedarf", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 1, null, null, null, null, null, null, null, 1, null, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 3, null, null, null, null, null, 3, 122.0, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 6, null, null, null, null, null, 6, null, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 1, null, null, null, null, null, 1, null, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 4, null, null, null, null, null, 4, null, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 2, null, null, null, null, null, 2, null, null });

            migrationBuilder.InsertData(
                table: "MotorUeberstroemer",
                columns: new[] { "Id", "Anzahl", "BreiteB", "CreatedDate", "FlaecheA", "HoeheH", "MotorId", "SteuerzeitSZ", "UpdatedDate" },
                values: new object[] { 5, null, null, null, null, null, 5, 117.0, null });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 5, @"Baujahre: 1980 - 1989
Stückzahl: 360.600", null, false, 79.5, 5, "S51 B1-4", null, });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 3, @"Baujahre: 1968 - 1980
Stückzahl: 375.000", null, false, 80.0, 3, "KR 51/1 (F)", null, });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 2, @"Baujahre: 1959 - 1964
Stückzahl: 164.500", null, false, 73.0, 2, "KR 50", null, });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 1, @"Baujahre: 1960 - 1964
Stückzahl: 515.000", null, false, 53.0, 1, "SR 2 E", null, });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 4, @"Baujahre: 1976 - 1980
Stückzahl: 287.000", null, false, 81.0, 4, "S 50 B1", null, });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Beschreibung", "CreatedDate", "Deletable", "Gewicht", "MotorId", "Name", "UpdatedDate" },
                values: new object[] { 6, @"Baujahre: 1984 - 1988
Stückzahl: 20.000", null, false, 84.0, 6, "S 70 C", null, });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 1, null, null, null, 1, null });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 2, null, null, null, 2, null });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 3, null, null, null, 3, null });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 4, null, null, null, 4, null });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 5, null, null, null, 5, null });

            migrationBuilder.InsertData(
                table: "Vergaser",
                columns: new[] { "Id", "BenzinLuftF", "CreatedDate", "DurchmesserD", "EinlassId", "UpdatedDate" },
                values: new object[] { 6, null, null, null, 6, null });

            migrationBuilder.CreateIndex(
                name: "IX_Auspuff_AuslassId",
                table: "Auspuff",
                column: "AuslassId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ausrollen_DynoId",
                table: "Ausrollen",
                column: "DynoId");

            migrationBuilder.CreateIndex(
                name: "IX_Dyno_EnvironmentId",
                table: "Dyno",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dyno_VehicleId",
                table: "Dyno",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynoAudio_DynoId",
                table: "DynoAudio",
                column: "DynoId");

            migrationBuilder.CreateIndex(
                name: "IX_DynoNm_DynoId",
                table: "DynoNm",
                column: "DynoId");

            migrationBuilder.CreateIndex(
                name: "IX_DynoPs_DynoId",
                table: "DynoPs",
                column: "DynoId");

            migrationBuilder.CreateIndex(
                name: "IX_Geschwindigkeit_DynoId",
                table: "Geschwindigkeit",
                column: "DynoId");

            migrationBuilder.CreateIndex(
                name: "IX_MotorAuslass_MotorId",
                table: "MotorAuslass",
                column: "MotorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotorEinlass_MotorId",
                table: "MotorEinlass",
                column: "MotorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotorUeberstroemer_MotorId",
                table: "MotorUeberstroemer",
                column: "MotorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tuning_EnvironmentId",
                table: "Tuning",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tuning_VehicleId",
                table: "Tuning",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuningPs_TuningId",
                table: "TuningPs",
                column: "TuningId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_MotorId",
                table: "Vehicles",
                column: "MotorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vergaser_EinlassId",
                table: "Vergaser",
                column: "EinlassId",
                unique: true);
        }
    }
}