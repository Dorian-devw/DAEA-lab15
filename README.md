# DAEA - Laboratorio 15: Microservicios con API Gateway y Ocelot
**Desarrollo de Aplicaciones Empresariales Avanzadas**  
**Tema:** Serpentario Bio-Farmacéutico y Banco de Venenos  
**Autor:** Frank Sinca (`Dorian-devw`)  
**Plataforma:** .NET 10 (C#) / ASP.NET Core / Entity Framework Core 10 / Ocelot 25.x / SQL Server

---

## 1. Introducción y Arquitectura de la Solución

En este proyecto se implementa un ecosistema de **microservicios desacoplados** gobernados por un **API Gateway (Ocelot)**, el cual funge como **único punto de entrada (Single Point of Entry)** hacia todos los servicios internos. El cliente (frontend, aplicaciones móviles o consumidores externos) desconoce la infraestructura interna, los nombres de host y los puertos de los microservicios, comunicándose exclusivamente a través del puerto expuesto por el Gateway.

### Diagrama Arquitectónico

```
                           +-------------------------------------+
                           |      Cliente HTTP / Frontend        |
                           |   (Postman / Thunder Client / UI)   |
                           +-------------------------------------+
                                              |
                                              | Solicitudes HTTP (Puerto 5000 / Nube)
                                              v
                           +-------------------------------------+
                           |       API Gateway (Ocelot)          |
                           |           "GatewayApi"              |
                           |   - Valida JWT ("Bearer")           |
                           |   - Enrutamiento Upstream/Downstream|
                           |   - Puerto Local: 5000              |
                           +-------------------------------------+
                                     /                 \
        /gateway/auth, /gateway/especies                 /gateway/laboratorios
        /gateway/lotesveneno                            /api/laboratorios
                                  /                       \
                                 v                         v
       +------------------------------------+   +------------------------------------+
       |  Microservicio 1 (S14 en Capas)    |   |  Microservicio 2 (Nuevo Servicio)  |
       |      "13Serpentarios.Api"          |   |        "LaboratoriosApi"           |
       | - Especies de Serpientes           |   | - Centros Farmacéuticos Aliados    |
       | - Lotes de Veneno y Extracciones   |   | - Convenios de Investigación       |
       | - Autenticación JWT y Roles        |   | - Datos en memoria (sin BD)        |
       | - SQL Server (MonsterASP / Local)  |   | - Valida Token JWT con misma clave |
       | - Puerto Local: 5055               |   | - Puerto Local: 5056               |
       +------------------------------------+   +------------------------------------+
```

---

## 2. Descripción de Componentes

### 2.1 Microservicio 1: API de Serpentario (`13Serpentarios.Api`)
* **Propósito:** Mantiene la lógica completa desarrollada en la Semana 14 en una arquitectura limpia en cuatro capas (`13Serpentarios.Entidades`, `13Serpentarios.Datos`, `13Serpentarios.Negocio` y `13Serpentarios.Api`).
* **Entidades Gestionadas:**
  * `EspeciesSerpiente`: Catálogo taxonómico de ofidios con su región de origen, tipo de veneno dominante y nivel de peligrosidad (escala 1 a 5).
  * `LotesVeneno`: Control de trazabilidad, stock en miligramos, porcentaje de pureza, cadena de frío (temperatura de almacenamiento) y fecha de caducidad.
  * `Usuarios`: Cuentas de usuarios con contraseñas encriptadas mediante **BCrypt** y asignación de roles estrictamente en el servidor.
* **Roles de Negocio:**
  * `Herpetologo`: Autorizado para dar de alta, modificar y eliminar especies ofídicas en el serpentario, así como gestionar la baja y descarte de lotes de veneno.
  * `TecnicoLaboratorio`: Encargado del registro de extracciones, control de calidad y actualización de parámetros de almacenamiento de los lotes.
* **Persistencia:** Entity Framework Core (Database-First) sobre Microsoft SQL Server.
* **Puerto Local:** `http://localhost:5055`

### 2.2 Microservicio 2: Alianzas y Laboratorios Farmacéuticos (`LaboratoriosApi`)
* **Propósito:** Microservicio independiente y desacoplado que administra el directorio de laboratorios farmacéuticos e institutos toxicológicos aliados (ej. Instituto Butantan, Instituto Clodomiro Picado, VenoTech Pharma, SnakeVenom Latam) que solicitan muestras biológicas para la formulación de sueros antiofídicos e investigación farmacológica.
* **Almacenamiento:** Datos gestionados en memoria (`List<Laboratorio>`), sin base de datos ni dependencia de clases con el Microservicio 1.
* **Seguridad:** Valida obligatoriamente el token JWT con la misma clave secreta, emisor (`SerpentarioApi`) y audiencia (`SerpentarioClients`) que el Microservicio 1. Todos sus endpoints exigen autenticación (`.RequireAuthorization()`).
* **Endpoints:**
  * `GET /api/laboratorios`: Listado completo de laboratorios aliados (requiere Token).
  * `GET /api/laboratorios/{id}`: Detalle técnico y contacto de un laboratorio por ID (requiere Token).
* **Puerto Local:** `http://localhost:5056`

### 2.3 API Gateway con Ocelot (`GatewayApi`)
* **Propósito:** Actúa como proxy inverso inteligente y frontera perimetral de seguridad.
* **Tecnología:** Proyecto ASP.NET Core con **Ocelot (v25.x)** y **Microsoft.AspNetCore.Authentication.JwtBearer (v10.x)**.
* **Validación de Token:** Inspecciona y valida la firma del token JWT en el esquema `"Bearer"` antes de reenviar solicitudes protegidas. Si el token falta o es inválido, rechaza inmediatamente la petición con un código `401 Unauthorized`.
* **Puerto Local:** `http://localhost:5000`

---

## 3. Tabla Maestra de Rutas (Upstream / Downstream)

| # | Upstream (Cliente → Gateway) | Método HTTP | Downstream (Gateway → Servicio) | Requiere Token | Explicación del Flujo |
| :-: | :--- | :---: | :--- | :---: | :--- |
| **a** | `/gateway/auth/{everything}` | `POST` | `localhost:5055 /api/auth/{everything}` | **No** | Login y registro públicos en el Microservicio 1 para emisión de tokens JWT. |
| **b** | `/gateway/auth/perfil` | `GET` | `localhost:5055 /api/auth/perfil` | **Sí** | Consulta del perfil autenticado a partir de los Claims del token (`User.Claims`). |
| **c** | `/gateway/especies{everything}` | `GET` | `localhost:5055 /api/especies{everything}` | **No** | Catálogo taxonómico público de especies ofídicas (lista y por ID). |
| **d** | `/gateway/especies{everything}` | `POST`, `PUT`, `DELETE` | `localhost:5055 /api/especies{everything}` | **Sí** | Escritura de especies. El Gateway valida el JWT y el MS1 valida el rol `Herpetologo`. |
| **e** | `/gateway/lotesveneno{everything}` | `GET`, `POST`, `PUT`, `DELETE` | `localhost:5055 /api/lotesveneno{everything}` | **Sí** | Trazabilidad del banco de venenos. Información técnica de uso interno restringido. |
| **f** | `/gateway/laboratorios{everything}` | `GET` | `localhost:5056 /api/laboratorios{everything}` | **Sí** | Catálogo del Microservicio 2. El Gateway valida el JWT antes de reenviar a MS2. |

---

## 4. Matriz de Códigos de Estado HTTP Verificados

Todas las pruebas se realizan **exclusivamente por el puerto del Gateway**:

| Código | Método | Ruta en el Gateway | Token | Causa / Comportamiento |
| :---: | :---: | :--- | :---: | :--- |
| **200 OK** | `GET` | `/gateway/especies` | No | Retorna el catálogo público de serpientes. |
| **200 OK** | `GET` | `/gateway/laboratorios` | Sí | Retorna la lista de laboratorios en memoria tras validar el JWT en Gateway y MS2. |
| **200 OK** | `POST` | `/gateway/auth/login` | No | Autenticación correcta que devuelve el Token JWT firmado. |
| **201 Created** | `POST` | `/gateway/auth/registro` | No | Crea un nuevo usuario en la base de datos con rol asignado por el servidor. |
| **201 Created** | `POST` | `/gateway/especies` | Sí (`Herpetologo`) | Da de alta una nueva especie ofídica en la base de datos. |
| **204 No Content** | `PUT` | `/gateway/especies/1` | Sí (`Herpetologo`) | Actualiza los datos de la especie sin retornar cuerpo de respuesta. |
| **400 Bad Request** | `POST` | `/gateway/especies` | Sí (`Herpetologo`) | Datos faltantes o que incumplen las Data Annotations del DTO. |
| **401 Unauthorized** | `GET` | `/gateway/laboratorios` | **Sin Token** | **El Gateway intercepta la solicitud**, valida que no existe el Bearer token y corta la petición con 401 antes de reenviarla. |
| **401 Unauthorized** | `GET` | `http://localhost:5056/api/laboratorios` | **Sin Token** | Llamada directa al Microservicio 2 que comprueba su propia protección JWT. |
| **403 Forbidden** | `POST` | `/gateway/especies` | Sí (`TecnicoLaboratorio`) | El Gateway valida el token con éxito (200), pero el Microservicio 1 deniega la acción por no tener el rol `Herpetologo`. |
| **404 Not Found** | `GET` | `/gateway/especies/99999` | No | Recurso inexistente en la base de datos. |
| **502 Bad Gateway** | `GET` | `/gateway/laboratorios` | Sí | Al detener el Microservicio 2, Ocelot no puede conectar con el downstream y responde 502, mientras el Microservicio 1 sigue activo y funcionando. |

---

## 5. Respuestas a Preguntas de Sustentación (Video de Sustentación)

### 1. ¿Qué cambia para una aplicación cliente al usar el gateway en lugar de llamar directamente a cada servicio?
* **Punto único de acceso (Single Point of Entry):** El cliente solo necesita conocer una única URL base (`http://localhost:5000` o dominio en la nube), evitando lidiar con múltiples hosts, IPs o puertos cambiantes de cada microservicio.
* **Frontera de seguridad unificada:** Centraliza la terminación SSL/TLS, la validación preliminar de tokens JWT, políticas de CORS y rate-limiting en un único lugar.
* **Desacoplamiento y evolución transparente:** Los microservicios internos pueden escalarse, migrarse de servidor o refactorizarse sin alterar los contratos de ruta expuestos hacia los clientes.

### 2. En tu tabla de rutas elige una ruta y explica su upstream y su downstream: qué parte cambia entre uno y otro y por qué.
* **Ruta de ejemplo:**
  * **Upstream:** `/gateway/laboratorios{everything}` (Método `GET`, Token Bearer requerido)
  * **Downstream:** `/api/laboratorios{everything}` (en `localhost:5056` o host público del MS2)
* **¿Qué cambia y por qué?**
  * El prefijo expuesto hacia el cliente es `/gateway/`, mientras que internamente el microservicio atiende bajo `/api/`. Esto enmascara la topología interna del servicio.
  * El comodín `{everything}` captura cualquier sub-ruta adicional (como `/1` para obtener un laboratorio por ID o query strings) y la reenvía íntegra al servicio de destino.
  * Cambia el destino de red: la petición recibida en el puerto `5000` del gateway se retransmite internamente hacia el puerto `5056`.

### 3. ¿Dónde se valida el token en tu solución? ¿Por qué se valida en el gateway y también en los servicios, y qué desventaja trae repetirlo?
* **¿Dónde se valida?** Se valida **dos veces**: primero en el **Gateway** (`AuthenticationOptions` con esquema `"Bearer"`) y luego en el **Microservicio downstream** (`UseAuthentication()` y `UseAuthorization()`).
* **¿Por qué se valida en ambos?**
  * **En el Gateway:** Actúa como primer filtro de defensa (Edge Security). Si la solicitud carece de token o está expirado, es rechazada en la frontera (401), ahorrando procesamiento innecesario y tráfico de red en los microservicios internos.
  * **En el Microservicio:** Implementa el principio de *Zero Trust* (seguridad en profundidad). Si una solicitud entrara por la red interna o por un servicio alterno, el microservicio no asume confianza ciega. Además, el microservicio necesita el token para inspeccionar los **Claims de roles del negocio** (`User.IsInRole("Herpetologo")`), responsabilidad que no le compete al Gateway.
* **¿Qué desventaja trae repetirlo?**
  * **Mayor latencia y consumo de CPU:** Cada petición requiere deserializar e inspeccionar la firma criptográfica HMAC-SHA256 en dos nodos distintos.
  * **Acoplamiento de secretos:** La clave simétrica, el emisor y la audiencia deben estar perfectamente sincronizados en la configuración de todos los proyectos. Modificar la clave requiere reiniciar o redistribuir secretos en toda la infraestructura.

### 4. Al detener el microservicio 2, ¿qué respondió el gateway y qué siguió funcionando? ¿Qué desventaja de los microservicios notaste al hacer este laboratorio?
* **Comportamiento observado:**
  * Al apagar el proceso de `LaboratoriosApi`, el Gateway respondió inmediatamente con un código **`502 Bad Gateway`** al intentar acceder a `/gateway/laboratorios`.
  * Paralelamente, el Microservicio 1 continuó operando con normalidad: las rutas `/gateway/especies` y `/gateway/auth/login` respondieron `200 OK`, demostrando **aislamiento de fallos** (la caída de un servicio secundario no colapsa el negocio principal).
* **Desventajas notadas en la práctica:**
  * **Complejidad operativa:** Mayor esfuerzo en gestión de puertos, procesos independientes, sincronización de secretos y monitoreo distribuido.
  * **Gestión de fallos en cascada:** Si no se configuran patrones como *Circuit Breaker*, peticiones repetidas a un servicio caído pueden saturar los hilos del Gateway.

---

## 6. Instrucciones de Ejecución Local

### Paso 1: Configurar la Base de Datos Local
1. Abre SQL Server Management Studio (SSMS) y conéctate a tu instancia (`localhost\SQLEXPRESS` o `.\SQLEXPRESS`).
2. Abre y ejecuta el script unificado:
   👉 **`database/SerpentarioLocal_Completo.sql`**
3. Esto crea la base de datos `Serpentario13DB`, tablas con llaves foráneas y registros iniciales.

### Paso 2: Ejecutar los Tres Proyectos
Abre tres terminales independientes y levanta los servicios:

```bash
# Terminal 1: Microservicio 1 (S14 en Capas)
dotnet run --project 13Serpentarios.Api
# Escuchando en: http://localhost:5055

# Terminal 2: Microservicio 2 (Laboratorios)
dotnet run --project LaboratoriosApi
# Escuchando en: http://localhost:5056

# Terminal 3: API Gateway (Ocelot)
dotnet run --project GatewayApi
# Escuchando en: http://localhost:5000
```

### Paso 3: Probar los Endpoints
Utiliza el archivo interactivo **`DAEA-lab15.http`** desde VS Code (con la extensión REST Client o Thunder Client) o Postman para ejecutar todas las pruebas directamente contra `http://localhost:5000`.

---

## 7. Despliegue en la Nube

### Alternativas de Hosting Gratuito y Justificación
1. **MonsterASP.NET (Recomendado):**
   * Proveedor especializado en ASP.NET Core (.NET 10) y Microsoft SQL Server.
   * Permite desplegar las tres aplicaciones web en sitios independientes y mantener la base de datos SQL Server sin cambiar de motor relacional ni adaptar migraciones de EF Core.
2. **Render.com / Railway:**
   * Soporta contenedores Docker o builds nativos de ASP.NET Core.
   * La base de datos gratuita requerida sería PostgreSQL (ej. Supabase o Neon), lo que obligaría a sustituir `Microsoft.EntityFrameworkCore.SqlServer` por `Npgsql.EntityFrameworkCore.PostgreSQL`.

### Configuración en Producción (`ocelot.Production.json`)
El Gateway incluye la plantilla `ocelot.Production.json`, configurada con protocolo HTTPS en el puerto `443` apuntando a los dominios públicos:

```json
{
  "DownstreamPathTemplate": "/api/laboratorios{everything}",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    {
      "Host": "serpentario-laboratorios.monsterasp.net",
      "Port": 443
    }
  ],
  "UpstreamPathTemplate": "/gateway/laboratorios{everything}",
  "UpstreamHttpMethod": [ "Get" ],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer",
    "AllowedScopes": []
  }
}
```

### Gestión Segura de Secretos y Cadenas de Conexión
Siguiendo las buenas prácticas y las pautas de evaluación, no se suben claves ni contraseñas reales al repositorio. En los paneles de hosting de cada servicio se configuran como **Variables de Entorno**:

* `ConnectionStrings__DefaultConnection`: Cadena de conexión hacia la base de datos SQL Server en la nube.
* `Jwt__Key`: Clave secreta simétrica idéntica en el Gateway y ambos microservicios.
* `Jwt__Issuer`: `SerpentarioApi`
* `Jwt__Audience`: `SerpentarioClients`
