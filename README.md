# DAEA - Laboratorio 15: Microservicios con API Gateway y Ocelot
**Desarrollo de Aplicaciones Empresariales Avanzadas**  
**Tema:** Serpentario Bio-Farmacéutico y Banco de Venenos

---

## 1. Arquitectura de la Solución

Esta solución implementa una arquitectura basada en microservicios desacoplados donde un **API Gateway (Ocelot)** actúa como **punto único de entrada** hacia los servicios internos del ecosistema. El cliente desconoce las URLs y puertos directos de los microservicios, comunicándose exclusivamente a través del puerto del Gateway.

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
                           |   - Puerto: http://localhost:5000   |
                           +-------------------------------------+
                                     /                 \
                                    /                   \
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
       | - Puerto: http://localhost:5055    |   | - Puerto: http://localhost:5056    |
       +------------------------------------+   +------------------------------------+
```

---

## 2. Descripción de Microservicios

### Microservicio 1: API de Serpentario y Banco de Venenos (`13Serpentarios.Api`)
* **Propósito:** Mantiene la lógica completa desarrollada en la Semana 14 en arquitectura limpia en 4 capas (`Entidades`, `Datos`, `Negocio`, `Api`).
* **Entidades Gestionadas:** 
  * `EspeciesSerpiente`: Catálogo taxonómico y nivel de peligrosidad (1 a 5).
  * `LotesVeneno`: Control de trazabilidad, pureza, temperatura de cadena de frío y vencimiento.
  * `Usuarios`: Cuentas con contraseñas encriptadas con **BCrypt** y asignación estricta de roles por servidor.
* **Roles de Negocio:**
  * `Herpetologo`: Alta, modificación y baja de taxones ofídicos y descarte de lotes.
  * `TecnicoLaboratorio`: Control de calidad, pureza y mantenimiento de lotes.
* **Puerto Local:** `http://localhost:5055`

### Microservicio 2: Alianzas y Laboratorios Farmacéuticos (`LaboratoriosApi`)
* **Propósito:** Microservicio ligero e independiente que administra el directorio de laboratorios e institutos toxicológicos aliados (por ejemplo: Instituto Butantan, Instituto Clodomiro Picado, VenoTech Pharma) que solicitan muestras de veneno para el desarrollo de sueros antiofídicos.
* **Almacenamiento:** Datos en memoria (`List<Laboratorio>`), sin dependencia de base de datos ni clases compartidas con el Microservicio 1.
* **Seguridad:** Valida obligatoriamente el token JWT con el mismo emisor (`SerpentarioApi`), audiencia (`SerpentarioClients`) y clave simétrica que el Microservicio 1. Exige autenticación en todos sus endpoints (`.RequireAuthorization()`).
* **Endpoints:**
  * `GET /api/laboratorios`: Retorna el catálogo completo de laboratorios aliados (requiere Token).
  * `GET /api/laboratorios/{id}`: Retorna el detalle técnico y contacto de un laboratorio por ID (requiere Token).
* **Puerto Local:** `http://localhost:5056`

---

## 3. Tabla de Enrutamiento del Gateway (Ocelot)

| # | Upstream (Cliente → Gateway) | Método HTTP | Downstream (Gateway → Microservicio) | Requiere Token | Explicación del Flujo |
| :-: | :--- | :---: | :--- | :---: | :--- |
| **a** | `/gateway/auth/{everything}` | `POST` | `localhost:5055 /api/auth/{everything}` | **No** | Registro y Login públicos en el Microservicio 1. Permite obtener el JWT. |
| **b** | `/gateway/auth/perfil` | `GET` | `localhost:5055 /api/auth/perfil` | **Sí** | Consulta del perfil autenticado a partir de los Claims del token. |
| **c** | `/gateway/especies{everything}` | `GET` | `localhost:5055 /api/especies{everything}` | **No** | Catálogo taxonómico público de especies ofídicas (lista y por ID). |
| **d** | `/gateway/especies{everything}` | `POST`, `PUT`, `DELETE` | `localhost:5055 /api/especies{everything}` | **Sí** | Modificación de especies. El Gateway valida el JWT y el MS1 valida el rol `Herpetologo`. |
| **e** | `/gateway/lotesveneno{everything}` | `GET`, `POST`, `PUT`, `DELETE` | `localhost:5055 /api/lotesveneno{everything}` | **Sí** | Trazabilidad del banco de venenos. Información interna protegida. |
| **f** | `/gateway/laboratorios{everything}` | `GET` | `localhost:5056 /api/laboratorios{everything}` | **Sí** | Catálogo del Microservicio 2. El Gateway valida el token antes de reenviar. |

---

## 4. Respuestas y Códigos de Estado Verificados

Todas las pruebas se ejecutan **exclusivamente a través de la dirección del Gateway** (`http://localhost:5000` o la URL en la nube):

| Código | Método | Ruta en el Gateway | Token | Causa / Comportamiento |
| :---: | :---: | :--- | :---: | :--- |
| **200 OK** | `GET` | `/gateway/especies` | No | Retorna el catálogo público de serpientes. |
| **200 OK** | `GET` | `/gateway/laboratorios` | Sí | Retorna la lista de laboratorios en memoria tras validar el JWT en Gateway y MS2. |
| **200 OK** | `POST` | `/gateway/auth/login` | No | Autenticación correcta que devuelve el Token JWT firmado. |
| **201 Created** | `POST` | `/gateway/auth/registro` | No | Crea un nuevo usuario en la base de datos con rol asignado por servidor. |
| **201 Created** | `POST` | `/gateway/especies` | Sí (`Herpetologo`) | Crea una nueva especie con éxito. |
| **204 No Content** | `PUT` | `/gateway/especies/1` | Sí (`Herpetologo`) | Actualiza los datos de la especie sin retornar cuerpo. |
| **401 Unauthorized** | `GET` | `/gateway/laboratorios` | **Sin Token** | **El Gateway intercepta la solicitud**, valida que no hay Bearer token y corta la petición con 401 antes de reenviarla. |
| **401 Unauthorized** | `GET` | `http://localhost:5056/api/laboratorios` | **Sin Token** | Llamada directa al Microservicio 2 que comprueba su propia protección JWT. |
| **403 Forbidden** | `POST` | `/gateway/especies` | Sí (`TecnicoLaboratorio`) | El Gateway valida el token con éxito (200), pero el Microservicio 1 deniega la acción por no tener el rol `Herpetologo`. |
| **404 Not Found** | `GET` | `/gateway/especies/99999` | No | Recurso inexistente en la base de datos de especies. |
| **502 Bad Gateway** | `GET` | `/gateway/laboratorios` | Sí | Al apagar el Microservicio 2, Ocelot no puede conectar con el downstream y responde 502, mientras el Microservicio 1 continúa funcionando con normalidad. |

---

## 5. Preguntas de Sustentación para el Video (Mínimo 10 min)

### 1. ¿Qué cambia para una aplicación cliente al usar el gateway en lugar de llamar directamente a cada servicio?
* **Punto único de acceso (Single Point of Entry):** El cliente (web o móvil) solo necesita conocer una única URL base (`https://mi-gateway...`), desacoplándose de la topología interna, direcciones IP y puertos individuales de cada microservicio.
* **Seguridad unificada:** El cliente se autentica una sola vez contra el Gateway. Las políticas de CORS, rate limiting, validación inicial de tokens y redirecciones se centralizan en una única frontera.
* **Evolución y refactorización transparente:** Se pueden mover servicios a otros puertos, servidores o tecnologías downstream sin romper las rutas que consume el cliente.

### 2. En tu tabla de rutas elige una ruta y explica su upstream y su downstream: qué parte cambia entre uno y otro y por qué.
* **Ruta elegida:**
  * **Upstream:** `/gateway/laboratorios{everything}` (con `GET` y Token obligatorio)
  * **Downstream:** `/api/laboratorios{everything}` (en `localhost:5056` o host del MS2)
* **¿Qué cambia y por qué?**
  * El prefijo `/gateway/` se sustituye por `/api/`.
  * El comodín `{everything}` captura cualquier segmento adicional (como `/1` para obtener un laboratorio por ID o query strings como `?pais=Peru`) y lo concatena dinámicamente a la ruta interna.
  * Se cambia el destino de red: la solicitud pública recibida en el puerto 5000 es reenviada internamente a la dirección privada del microservicio 2 (`localhost:5056`). Esto previene exponer rutas de backend y asegura que todas las peticiones pasen por el filtro de seguridad de Ocelot.

### 3. ¿Dónde se valida el token en tu solución? ¿Por qué se valida en el gateway y también en los servicios, y qué desventaja trae repetirlo?
* **¿Dónde se valida?** Se valida **dos veces**: primero en el **Gateway** (`AuthenticationOptions` con esquema `"Bearer"`) y nuevamente en los **Microservicios** (`UseAuthentication()` y `UseAuthorization()`).
* **¿Por qué se valida en ambos?**
  * **En el Gateway:** Actúa como primer filtro de defensa (Edge Security). Si el token no existe, está expirado o fue firmado con una clave incorrecta, el Gateway rechaza la petición inmediatamente con **401 Unauthorized**, ahorrando tráfico de red y consumo de CPU en los microservicios downstream.
  * **En los Microservicios:** Cumple con el principio de *Zero Trust* (seguridad en profundidad). Si una solicitud burlara el gateway o proviniera de la red interna, el microservicio no confiará a ciegas y además necesita el token para inspeccionar los **Claims de roles de negocio** (`User.IsInRole("Herpetologo")`), algo que Ocelot no evalúa.
* **¿Qué desventaja trae repetirlo?**
  * **Sobrecarga de cómputo (Latencia):** Cada petición debe deserializar y verificar la firma criptográfica HMAC-SHA256 dos veces en el ciclo de vida del request.
  * **Acoplamiento de secretos:** La misma clave secreta, emisor y audiencia deben distribuirse y mantenerse sincronizados en los appsettings/variables de entorno de todos los proyectos. Si se rota la clave, debe actualizarse en todos simultáneamente.

### 4. Al detener el microservicio 2, ¿qué respondió el gateway y qué siguió funcionando? ¿Qué desventaja de los microservicios notaste al hacer este laboratorio?
* **Comportamiento observado:**
  * Al apagar el proceso de `LaboratoriosApi`, el Gateway respondió inmediatamente con un código **`502 Bad Gateway`** al intentar invocar `/gateway/laboratorios`.
  * Sin embargo, el Microservicio 1 continuó respondiendo perfectamente a `/gateway/especies` (200 OK) y `/gateway/auth/login` (200 OK), demostrando el **aislamiento de fallos** (la caída de un servicio secundario no derrumba el sistema principal).
* **Desventajas notadas en la práctica:**
  * **Complejidad operativa:** Gestionar múltiples procesos, mapeo de puertos, orquestación de arranque y archivos de configuración separados requiere mayor esfuerzo que una arquitectura monolítica.
  * **Sincronización de seguridad:** Distribuir y sincronizar las llaves JWT y la caducidad entre el gateway y los servicios downstream genera riesgo de inconsistencias si no se cuenta con un servidor de identidad centralizado (como Keycloak o IdentityServer).

---

## 6. Guía de Ejecución y Pruebas Locales

### Ejecución de los 3 Proyectos en Terminales Separadas

**Terminal 1 (Microservicio 1 - S14):**
```bash
dotnet run --project 13Serpentarios.Api
# Escuchando en: http://localhost:5055
```

**Terminal 2 (Microservicio 2 - Laboratorios):**
```bash
dotnet run --project LaboratoriosApi
# Escuchando en: http://localhost:5056
```

**Terminal 3 (Gateway Ocelot):**
```bash
dotnet run --project GatewayApi
# Escuchando en: http://localhost:5000
```

### Ejecución de Pruebas Automatizadas
Puedes ejecutar el script PowerShell incluido que realiza la batería completa y comprueba los códigos de estado automáticamente:
```powershell
powershell -ExecutionPolicy Bypass -File .\test_suite.ps1
```

O abrir el archivo interactivo [DAEA-lab15.http](file:///c:/Users/ACER/Desktop/DAEA-sem15/DAEA-lab15.http) con Thunder Client o REST Client.

---

## 7. Despliegue en la Nube

### Alternativas de Hosting Gratuito
1. **MonsterASP.NET (Recomendado):**
   * Ofrece hosting gratuito para aplicaciones ASP.NET Core (.NET 10) y bases de datos MS SQL Server.
   * Permite desplegar las 3 aplicaciones en sitios independientes y conectarse a la base de datos SQL Server sin cambiar de motor de base de datos ni scripts EF Core.
2. **Render.com / Railway:**
   * Soporta contenedores Docker o compilación .NET directa.
   * Requiere base de datos PostgreSQL gratuita (Neon / Supabase), lo cual obligaría a cambiar el proveedor a `Npgsql.EntityFrameworkCore.PostgreSQL`.

### Configuración de `ocelot.Production.json`
En la nube, el archivo `ocelot.Production.json` (incluido en `GatewayApi`) redirige hacia las URLs públicas con HTTPS en el puerto 443:
```json
{
  "DownstreamPathTemplate": "/api/laboratorios{everything}",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    {
      "Host": "tu-microservicio2.monsterasp.net",
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

### Variables de Entorno en el Proveedor Cloud
No se suben contraseñas ni cadenas de conexión al repositorio Git:
* `ConnectionStrings__DefaultConnection`: Cadena de conexión hacia la BD de la nube.
* `Jwt__Key`: Clave secreta idéntica en Gateway y en ambos microservicios.
* `Jwt__Issuer`: `SerpentarioApi`
* `Jwt__Audience`: `SerpentarioClients`
