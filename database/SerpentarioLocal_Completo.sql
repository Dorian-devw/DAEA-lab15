-- ============================================================================
-- SCRIPT COMPLETO PARA SQL SERVER LOCAL (SQLEXPRESS / SSMS)
-- Base de Datos: Serpentario13DB
-- Tema: Serpentario Bio-Farmacéutico y Banco de Venenos
-- ============================================================================

IF DB_ID(N'Serpentario13DB') IS NULL
BEGIN
    CREATE DATABASE Serpentario13DB;
    PRINT 'Base de datos Serpentario13DB creada exitosamente.';
END
GO

USE Serpentario13DB;
GO

-- 1. Limpieza de tablas previas si existen (respetando llaves foráneas)
IF OBJECT_ID(N'dbo.LotesVeneno', N'U') IS NOT NULL
    DROP TABLE dbo.LotesVeneno;
GO

IF OBJECT_ID(N'dbo.EspeciesSerpiente', N'U') IS NOT NULL
    DROP TABLE dbo.EspeciesSerpiente;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL
    DROP TABLE dbo.Usuarios;
GO

-- 2. Creación de la tabla de Especies de Serpientes
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

-- 3. Creación de la tabla de Lotes de Veneno Extraído
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

-- 4. Creación de la tabla de Usuarios y Roles JWT
CREATE TABLE dbo.Usuarios (
    UsuarioId INT IDENTITY(1,1) NOT NULL,
    Correo NVARCHAR(150) NOT NULL,
    ClaveHash NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
    FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (UsuarioId),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Herpetologo', N'TecnicoLaboratorio'))
);
GO

-- 5. Inserción de datos semilla en Especies
INSERT INTO dbo.EspeciesSerpiente
    (NombreCientifico, NombreComun, Familia, RegionOrigen, TipoVenenoDominante, NivelPeligrosidad, Activa)
VALUES
    (N'Bothrops atrox', N'Jergón de la Selva', N'Viperidae', N'Amazonía Peruana', N'Hemorrágico y Necrotizante', 5, 1),
    (N'Lachesis muta', N'Shushupe', N'Viperidae', N'Selva Alta y Selva Baja', N'Proteolítico y Coagulante', 5, 1),
    (N'Micrurus spixii', N'Coral Amazónica Gigante', N'Elapidae', N'Cuenca del Marañón', N'Neurotóxico post-sináptico', 4, 1),
    (N'Bothrocophias hyoprora', N'Jergoncillo de Nariz Chata', N'Viperidae', N'Loreto y Madre de Dios', N'Miotóxico y Hemotóxico', 4, 1),
    (N'Crotalus durissus terrificus', N'Cascabel Sudamericana', N'Viperidae', N'Puno y Madre de Dios', N'Neurotóxico y Miotóxico', 5, 1);
GO

-- 6. Inserción de datos semilla en Lotes de Veneno
INSERT INTO dbo.LotesVeneno
    (CodigoLote, EspecieId, FechaExtraccion, FechaVencimiento, CantidadMiligramos, PurezaPorcentaje, Presentacion, TemperaturaAlmacenamientoC, EstadoCalidad, UsoDestino, Observaciones)
VALUES
    (N'LOT-2026-BATR-01', 1, '2026-01-10', '2028-01-10', 450.00, 96.50, N'Liofilizado', -20.00, N'Aprobado', N'Producción de Suero Antiofídico Polivalente', N'Extracción controlada en terrario bioclimático.'),
    (N'LOT-2026-LMUT-02', 2, '2026-02-05', '2027-08-05', 180.50, 94.20, N'Cristalizado', -80.00, N'Aprobado', N'Aislamiento de L-aminoácido oxidasas para investigación', N'Muestra de alta pureza en ultracongelación.'),
    (N'LOT-2026-MSPI-03', 3, '2026-03-01', '2027-03-01', 65.00, 98.10, N'Liofilizado', -20.00, N'En Cuarentena', N'Ensayos de caracterización de neurotoxinas', N'Volumen limitado extraído por microcapilar.'),
    (N'LOT-2026-CDUR-04', 5, '2026-03-15', '2028-03-15', 310.00, 95.80, N'Liofilizado', -20.00, N'Aprobado', N'Producción de antiveneno crotálico', N'Crotoxina concentrada.');
GO

-- 7. Inserción de usuarios semilla para pruebas de autenticación
-- Contraseña para ambos: "Clave123*"
INSERT INTO dbo.Usuarios (Correo, ClaveHash, Rol)
VALUES
    ('herpetologo@serpentario.pe', '$2a$11$0jN50x3h89vEwS/eX1w8.eI8eG7YqZ3o4JgN128Z4vL0F6l2mP4w2', 'Herpetologo'),
    ('laboratorio@serpentario.pe', '$2a$11$0jN50x3h89vEwS/eX1w8.eI8eG7YqZ3o4JgN128Z4vL0F6l2mP4w2', 'TecnicoLaboratorio');
GO

-- 8. Verificación final
SELECT 'Especies creadas:' AS Entidad, COUNT(*) AS Total FROM dbo.EspeciesSerpiente
UNION ALL
SELECT 'Lotes creados:', COUNT(*) FROM dbo.LotesVeneno
UNION ALL
SELECT 'Usuarios creados:', COUNT(*) FROM dbo.Usuarios;
GO
