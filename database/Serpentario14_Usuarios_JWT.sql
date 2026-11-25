-- ============================================================================
-- SCRIPT SEMANA 14: TABLA DE USUARIOS Y ROLES DEL NEGOCIO
-- Base de datos: Serpentario13DB
-- Roles del negocio: 'Herpetologo' y 'TecnicoLaboratorio'
-- ============================================================================

USE Serpentario13DB;
GO

-- 1. Creación de la tabla dbo.Usuarios si no existe
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
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
    PRINT 'Tabla dbo.Usuarios creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla dbo.Usuarios ya existe.';
END
GO

-- 2. Inserción de usuarios semilla con roles del negocio
-- Contraseña de prueba para ambos: "Clave123*"
-- NOTA: También puedes registrarlos por la API (/api/auth/registro) y luego
-- actualizar el rol con: UPDATE dbo.Usuarios SET Rol = 'Herpetologo' WHERE Correo = '...';

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo = 'herpetologo@serpentario.pe')
BEGIN
    INSERT INTO dbo.Usuarios (Correo, ClaveHash, Rol)
    VALUES (
        'herpetologo@serpentario.pe',
        '$2a$11$0jN50x3h89vEwS/eX1w8.eI8eG7YqZ3o4JgN128Z4vL0F6l2mP4w2',
        'Herpetologo'
    );
    PRINT 'Usuario Herpetologo insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo = 'laboratorio@serpentario.pe')
BEGIN
    INSERT INTO dbo.Usuarios (Correo, ClaveHash, Rol)
    VALUES (
        'laboratorio@serpentario.pe',
        '$2a$11$0jN50x3h89vEwS/eX1w8.eI8eG7YqZ3o4JgN128Z4vL0F6l2mP4w2',
        'TecnicoLaboratorio'
    );
    PRINT 'Usuario TecnicoLaboratorio insertado.';
END
GO

-- 3. Verificación
SELECT UsuarioId, Correo, Rol, FechaRegistro FROM dbo.Usuarios;
GO
