-- ============================================================================
-- Base de datos: Serpentario13DB
-- Tema: Serpentario Bio-Farmacéutico y Banco de Venenos
-- ============================================================================

IF DB_ID(N'Serpentario13DB') IS NULL
BEGIN
    CREATE DATABASE Serpentario13DB;
END
GO

USE Serpentario13DB;
GO

IF OBJECT_ID(N'dbo.LotesVeneno', N'U') IS NOT NULL
    DROP TABLE dbo.LotesVeneno;
GO

IF OBJECT_ID(N'dbo.EspeciesSerpiente', N'U') IS NOT NULL
    DROP TABLE dbo.EspeciesSerpiente;
GO

CREATE TABLE dbo.EspeciesSerpiente (
    EspecieId INT IDENTITY(1,1) NOT NULL,
    NombreCientifico NVARCHAR(120) NOT NULL,
    NombreComun NVARCHAR(100) NOT NULL,
    Familia NVARCHAR(60) NOT NULL,
    RegionOrigen NVARCHAR(120) NOT NULL,
    TipoVenenoDominante NVARCHAR(60) NOT NULL,
    NivelPeligrosidad INT NOT NULL,
    Activa BIT NOT NULL CONSTRAINT DF_EspeciesSerpiente_Activa DEFAULT (1),
    FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_EspeciesSerpiente_FechaRegistro DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_EspeciesSerpiente PRIMARY KEY CLUSTERED (EspecieId),
    CONSTRAINT UQ_EspeciesSerpiente_NombreCientifico UNIQUE (NombreCientifico),
    CONSTRAINT CK_EspeciesSerpiente_NivelPeligrosidad CHECK (NivelPeligrosidad BETWEEN 1 AND 5)
);
GO

CREATE TABLE dbo.LotesVeneno (
    LoteId INT IDENTITY(1,1) NOT NULL,
    CodigoLote NVARCHAR(30) NOT NULL,
    EspecieId INT NOT NULL,
    FechaExtraccion DATE NOT NULL,
    FechaVencimiento DATE NOT NULL,
    CantidadMiligramos DECIMAL(10, 2) NOT NULL,
    PurezaPorcentaje DECIMAL(5, 2) NOT NULL,
    Presentacion NVARCHAR(40) NOT NULL,
    TemperaturaAlmacenamientoC DECIMAL(5, 2) NOT NULL,
    EstadoCalidad NVARCHAR(30) NOT NULL,
    UsoDestino NVARCHAR(120) NOT NULL,
    Observaciones NVARCHAR(300) NULL,
    CONSTRAINT PK_LotesVeneno PRIMARY KEY CLUSTERED (LoteId),
    CONSTRAINT UQ_LotesVeneno_CodigoLote UNIQUE (CodigoLote),
    CONSTRAINT FK_LotesVeneno_EspeciesSerpiente FOREIGN KEY (EspecieId)
        REFERENCES dbo.EspeciesSerpiente (EspecieId),
    CONSTRAINT CK_LotesVeneno_Cantidad CHECK (CantidadMiligramos >= 0),
    CONSTRAINT CK_LotesVeneno_Pureza CHECK (PurezaPorcentaje BETWEEN 0 AND 100),
    CONSTRAINT CK_LotesVeneno_Fechas CHECK (FechaVencimiento > FechaExtraccion)
);
GO

-- ============================================================================
-- Datos semilla de ejemplo
-- ============================================================================

INSERT INTO dbo.EspeciesSerpiente
    (NombreCientifico, NombreComun, Familia, RegionOrigen, TipoVenenoDominante, NivelPeligrosidad, Activa)
VALUES
    (N'Bothrops atrox', N'Jergón de la selva', N'Viperidae', N'Amazonía Peruana', N'Hemotóxico', 5, 1),
    (N'Lachesis muta', N'Shushupe', N'Viperidae', N'Cuenca Amazónica', N'Proteolítico y Hemotóxico', 5, 1),
    (N'Crotalus durissus', N'Cascabel sudamericana', N'Viperidae', N'Sabanas Neotropicales', N'Neurotóxico y Miotóxico', 5, 1),
    (N'Micrurus surinamensis', N'Coral acuática amazónica', N'Elapidae', N'Ríos Amazónicos', N'Neurotóxico', 4, 1),
    (N'Bothrops pictus', N'Jergón de costa', N'Viperidae', N'Costa Central y Sur del Perú', N'Hemotóxico', 4, 0),
    (N'Bothrops jararaca', N'Jararaca sudamericana', N'Viperidae', N'Mata Atlántica (Brasil y Paraguay)', N'Hemotóxico (Inhibidores ECA)', 5, 1),
    (N'Dendroaspis polylepis', N'Mamba negra', N'Elapidae', N'África Subsahariana', N'Neurotóxico (Dendrotoxinas)', 5, 1),
    (N'Ophiophagus hannah', N'Cobra real', N'Elapidae', N'Sur y Sudeste Asiático', N'Neurotóxico y Cardiotóxico', 5, 1),
    (N'Bitis arietans', N'Víbora bufadora africana', N'Viperidae', N'Sabanas de África', N'Citotóxico y Hemotóxico', 4, 1),
    (N'Agkistrodon contortrix', N'Cabeza de cobre', N'Viperidae', N'América del Norte', N'Hemotóxico (Contortrostatina)', 3, 1),
    (N'Daboia russelii', N'Víbora de Russell', N'Viperidae', N'Subcontinente Indio', N'Hemotóxico y Nefrotóxico', 5, 1),
    (N'Bungarus multicinctus', N'Krait rayada china', N'Elapidae', N'Sudeste de China y Taiwán', N'Neurotóxico (Alfa-Bungarotoxina)', 5, 1);
GO

INSERT INTO dbo.LotesVeneno
    (CodigoLote, EspecieId, FechaExtraccion, FechaVencimiento, CantidadMiligramos, PurezaPorcentaje, Presentacion, TemperaturaAlmacenamientoC, EstadoCalidad, UsoDestino, Observaciones)
VALUES
    (N'VEN-BA-2026-001', 1, '2026-02-10', '2028-02-10', 450.50, 96.40, N'Liofilizado', -20.00, N'Aprobado', N'Suero Antiofídico Polivalente', N'Lote estándar de referencia para inmunización equina.'),
    (N'VEN-BA-2026-002', 1, '2026-05-18', '2028-05-18', 310.00, 92.10, N'Liofilizado', -20.00, N'Aprobado', N'Aislamiento de Metaloproteinasas', N'Extracción en serpentario bioseguro nivel 3.'),
    (N'VEN-LM-2026-001', 2, '2026-03-04', '2028-03-04', 620.75, 95.80, N'Liofilizado', -80.00, N'Aprobado', N'Suero Antilachésico Monovalente', N'Alta actividad coagulante verificada por cromatografía.'),
    (N'VEN-CD-2026-001', 3, '2026-06-12', '2028-06-12', 280.25, 89.50, N'Líquido Ultracongelado', -80.00, N'Cuarentena', N'Investigación Analgésica (Crotoxina)', N'Pendiente de ensayo de esterilidad final.'),
    (N'VEN-MS-2026-001', 4, '2026-07-01', '2027-07-01', 85.40, 97.20, N'Liofilizado', -80.00, N'Aprobado', N'Antiveneno Antimicrúrico', N'Bajo rendimiento por extracción; uso restringido a calibración.'),
    (N'VEN-BJ-2026-001', 6, '2026-04-15', '2028-04-15', 390.00, 98.10, N'Liofilizado', -20.00, N'Aprobado', N'Síntesis de Péptidos Antihipertensivos', N'Fracción rica en inhibidores de la enzima convertidora de angiotensina.'),
    (N'VEN-BJ-2026-002', 6, '2026-07-22', '2028-07-22', 215.60, 88.40, N'Líquido Ultracongelado', -80.00, N'Cuarentena', N'Fraccionamiento de Bradicinina', N'En evaluación por espectrofotometría UV.'),
    (N'VEN-DP-2026-001', 7, '2026-05-03', '2028-05-03', 145.80, 95.30, N'Liofilizado', -80.00, N'Aprobado', N'Investigación de Mambalginas (Analgésico)', N'Aislamiento de péptidos bloqueadores de canales ASIC.'),
    (N'VEN-OH-2026-001', 8, '2026-06-19', '2028-06-19', 510.20, 94.60, N'Liofilizado', -20.00, N'Aprobado', N'Antiveneno Monovalente Asiático', N'Rendimiento elevado de extracción en ejemplar adulto.'),
    (N'VEN-BAF-2026-001', 9, '2026-01-25', '2028-01-25', 0.00, 91.20, N'Liofilizado', -20.00, N'Agotado', N'Suero Polivalente Africano', N'Stock transferido íntegramente a planta de inmunización.'),
    (N'VEN-AC-2026-001', 10, '2026-04-28', '2028-04-28', 195.40, 96.80, N'Liofilizado', -80.00, N'Aprobado', N'Estudio Antiangiogénico Oncológico', N'Contortrostatina purificada para ensayos celulares in vitro.'),
    (N'VEN-DR-2026-001', 11, '2026-08-05', '2028-08-05', 340.00, 86.00, N'Líquido Ultracongelado', -80.00, N'Cuarentena', N'Reactivo de Coagulación Diagnóstica (RVV)', N'Pendiente validación de activador del Factor X.');
GO

