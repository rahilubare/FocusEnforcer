# Self-elevate
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Start-Process powershell.exe "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$serviceName = "FocusEnforcerService"
# Absolute path to ensure no resolution errors
$serviceExe = "c:\tmp\FocusEnforcer\FocusEnforcer.Service\bin\Debug\net8.0\FocusEnforcer.Service.exe"

Write-Host "Installing Service from: $serviceExe"

if (!(Test-Path $serviceExe)) {
    Write-Error "Executable not found at $serviceExe. Did you build the solution?"
    Read-Host "Press Enter to exit..."
    exit
}

Stop-Service $serviceName -ErrorAction SilentlyContinue
sc.exe delete $serviceName

Start-Sleep -Seconds 2

# Use sc.exe for more reliable service creation than New-Service in some contexts
sc.exe create $serviceName binPath= "$serviceExe" start= auto DisplayName= "Focus Enforcer Service"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Service Created. Starting..."
    sc.exe start $serviceName
    
    Start-Sleep -Seconds 2
    $s = Get-Service $serviceName
    Write-Host "Service Status: $($s.Status)"
} else {
    Write-Error "Failed to create service."
}

Read-Host "Press Enter to finish..."
