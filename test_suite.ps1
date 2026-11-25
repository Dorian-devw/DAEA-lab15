$ErrorActionPreference = "Continue"

Get-Process -Name "13Serpentarios.Api","GatewayApi","LaboratoriosApi" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

function Get-HttpStatus($url, $method="GET", $headers=$null, $body=$null) {
    try {
        $params = @{
            Uri = $url
            Method = $method
            UseBasicParsing = $true
        }
        if ($headers) { $params.Headers = $headers }
        if ($body) { 
            $params.Body = $body
            $params.ContentType = "application/json"
        }
        $resp = Invoke-WebRequest @params
        return [int]$resp.StatusCode
    } catch [System.Net.WebException] {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        return 502
    } catch {
        return 500
    }
}

Write-Host "============================================================"
Write-Host "INICIANDO SERVICIOS..."
Write-Host "============================================================"

Write-Host "Iniciando Microservicio 2 (LaboratoriosApi) en puerto 5056..."
$pMS2 = Start-Process dotnet -ArgumentList "run --project LaboratoriosApi" -PassThru
Start-Sleep -Seconds 5

Write-Host "Iniciando GatewayApi en puerto 5000..."
$pGW = Start-Process dotnet -ArgumentList "run --project GatewayApi" -PassThru
Start-Sleep -Seconds 5

Write-Host "Iniciando Microservicio 1 (13Serpentarios.Api) en puerto 5055..."
$pMS1 = Start-Process dotnet -ArgumentList "run --project 13Serpentarios.Api" -PassThru
Start-Sleep -Seconds 7

Write-Host "`n============================================================"
Write-Host "EJECUTANDO BATERIA DE PRUEBAS DE LA SEMANA 15"
Write-Host "============================================================"

# Prueba 1: 401 directo
$cDirectMS2 = Get-HttpStatus -url "http://localhost:5056/api/laboratorios"
Write-Host "[OK] 1. Llamada directa a MS2 sin Token -> Codigo: $cDirectMS2 (Esperado: 401)"

# Prueba 2: 401 por Gateway
$cGWNoToken = Get-HttpStatus -url "http://localhost:5000/gateway/laboratorios"
Write-Host "[OK] 2. Llamada a MS2 por Gateway sin Token -> Codigo: $cGWNoToken (Esperado: 401)"

# Prueba 3: 201 Created por Gateway (Registro de usuario)
$randomMail = "herpeto_$(Get-Random)@serpentario.pe"
$bodyReg = @{
    correo = $randomMail
    clave = "Clave123*"
} | ConvertTo-Json

try {
    $resReg = Invoke-RestMethod -Uri "http://localhost:5000/gateway/auth/registro" -Method POST -Body $bodyReg -ContentType "application/json"
    Write-Host "[OK] 3. POST /gateway/auth/registro (201 Created) -> Usuario registrado: $($resReg.correo), Rol: $($resReg.rol)"
    
    # Asignar rol Herpetologo para pruebas de escritura
    $conn = New-Object System.Data.SqlClient.SqlConnection("Server=localhost\SQLEXPRESS; Database=Serpentario13DB; Integrated Security=True; TrustServerCertificate=True;")
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE dbo.Usuarios SET Rol = 'Herpetologo' WHERE Correo = '$randomMail';"
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()

    # Prueba 4: 200 OK en Login
    $bodyLog = @{
        correo = $randomMail
        clave = "Clave123*"
    } | ConvertTo-Json

    $resLog = Invoke-RestMethod -Uri "http://localhost:5000/gateway/auth/login" -Method POST -Body $bodyLog -ContentType "application/json"
    $token = $resLog.token
    Write-Host "[OK] 4. POST /gateway/auth/login (200 OK) -> Token JWT generado correctamente!"

    $headers = @{ Authorization = "Bearer $token" }

    # Prueba 5: 200 OK GET Laboratorios (MS2 con Token)
    $cLabs = Get-HttpStatus -url "http://localhost:5000/gateway/laboratorios" -headers $headers
    Write-Host "[OK] 5. GET /gateway/laboratorios con Token -> Codigo: $cLabs (Esperado: 200)"

    # Prueba 6: 200 OK GET Especies (MS1 publico)
    $cEspecies = Get-HttpStatus -url "http://localhost:5000/gateway/especies"
    Write-Host "[OK] 6. GET /gateway/especies publico -> Codigo: $cEspecies (Esperado: 200)"

    # Prueba 7: 201 Created POST Especies (MS1 con Token y Rol Herpetologo)
    $randNum = Get-Random
    $bodyNuevaEspecie = @{
        nombreCientifico = "Micrurus andinus $randNum"
        nombreComun = "Coralina Andina"
        familia = "Elapidae"
        regionOrigen = "Amazonas"
        tipoVenenoDominante = "Neurotoxico"
        nivelPeligrosidad = 4
        activa = $true
    } | ConvertTo-Json
    $cCrearEsp = Get-HttpStatus -url "http://localhost:5000/gateway/especies" -method "POST" -headers $headers -body $bodyNuevaEspecie
    Write-Host "[OK] 7. POST /gateway/especies con rol Herpetologo -> Codigo: $cCrearEsp (Esperado: 201)"

    # Prueba 8: 204 No Content PUT Especie
    $bodyUpdate = @{
        nombreCientifico = "Bothrops atrox"
        nombreComun = "Jergon de la Selva (Actualizado)"
        familia = "Viperidae"
        regionOrigen = "Amazonia"
        tipoVenenoDominante = "Hemorragico"
        nivelPeligrosidad = 5
        activa = $true
    } | ConvertTo-Json
    $cPutEsp = Get-HttpStatus -url "http://localhost:5000/gateway/especies/1" -method "PUT" -headers $headers -body $bodyUpdate
    Write-Host "[OK] 8. PUT /gateway/especies/1 con rol Herpetologo -> Codigo: $cPutEsp (Esperado: 204)"

    # Prueba 9: 403 Forbidden (Registrar usuario normal TecnicoLaboratorio e intentar crear especie)
    $mailTecnico = "tecnico_$(Get-Random)@serpentario.pe"
    $bodyRegTec = @{ correo = $mailTecnico; clave = "Clave123*" } | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5000/gateway/auth/registro" -Method POST -Body $bodyRegTec -ContentType "application/json" | Out-Null
    $resLogTec = Invoke-RestMethod -Uri "http://localhost:5000/gateway/auth/login" -Method POST -Body $bodyRegTec -ContentType "application/json"
    $tokenTecnico = $resLogTec.token
    $headersTecnico = @{ Authorization = "Bearer $tokenTecnico" }

    $cForbidden = Get-HttpStatus -url "http://localhost:5000/gateway/especies" -method "POST" -headers $headersTecnico -body $bodyNuevaEspecie
    Write-Host "[OK] 9. POST /gateway/especies con rol TecnicoLaboratorio -> Codigo: $cForbidden (Esperado: 403 Forbidden)"

    # Prueba 10: 404 Not Found
    $c404 = Get-HttpStatus -url "http://localhost:5000/gateway/especies/99999"
    Write-Host "[OK] 10. GET /gateway/especies/99999 -> Codigo: $c404 (Esperado: 404)"

    # Prueba 11: 502 Bad Gateway (Detener MS2)
    Write-Host "`nDeteniendo Microservicio 2 (LaboratoriosApi) para forzar 502 Bad Gateway..."
    Stop-Process -Id $pMS2.Id -Force -ErrorAction SilentlyContinue
    Get-Process -Name "LaboratoriosApi" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3

    $c502 = Get-HttpStatus -url "http://localhost:5000/gateway/laboratorios" -headers $headers
    Write-Host "[OK] 11. GET /gateway/laboratorios con MS2 detenido -> Codigo: $c502 (Esperado: 502 Bad Gateway)"

    $cEspeciesVivas = Get-HttpStatus -url "http://localhost:5000/gateway/especies"
    Write-Host "[OK] 12. GET /gateway/especies con MS2 detenido -> Codigo: $cEspeciesVivas (Esperado: 200 OK - MS1 sigue activo)"

} catch {
    Write-Host "Error durante las pruebas: $_"
} finally {
    Write-Host "`nLimpiando procesos..."
    Stop-Process -Id $pGW.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $pMS1.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $pMS2.Id -Force -ErrorAction SilentlyContinue
    Get-Process -Name "13Serpentarios.Api","GatewayApi","LaboratoriosApi" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "Pruebas finalizadas."
}
