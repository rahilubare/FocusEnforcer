# Check for Administrator privileges
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "This script must be run as Administrator to install the Windows Service!"
    Write-Warning "Please right-click this script and select 'Run with PowerShell' as Administrator."
    Start-Sleep -Seconds 5
    Exit
}

$serviceName = "FocusEnforcerService"
$servicePath = "$PSScriptRoot\FocusEnforcer.Service\bin\Debug\net8.0\FocusEnforcer.Service.exe"

# Stop and Remove existing service if it exists
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping existing service..."
    Stop-Service $serviceName
    $service = Get-WmiObject -Class Win32_Service -Filter "Name='$serviceName'"
    if ($service) {
        Write-Host "Removing existing service..."
        $service.delete()
        Start-Sleep -Seconds 2
    }
}

# Create new service
Write-Host "Creating FocusEnforcer Service..."
Write-Host "Path: $servicePath"

New-Service -Name $serviceName -BinaryPathName $servicePath -Description "FocusEnforcer Background Service" -DisplayName "Focus Enforcer Service" -StartupType Automatic

# Start service
Write-Host "Starting Service..."
Start-Service $serviceName

Write-Host "---------------------------------------------------"
Write-Host "Service Installed and Started Successfully!"
Write-Host "You can now use the Dashboard to block websites."
Write-Host "---------------------------------------------------"
Start-Sleep -Seconds 5
