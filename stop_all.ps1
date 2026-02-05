# Self-elevate
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Start-Process powershell.exe "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Write-Host "Stopping FocusEnforcerService..."
Stop-Service "FocusEnforcerService" -Force -ErrorAction SilentlyContinue
sc.exe stop "FocusEnforcerService"

Write-Host "Killing Processes..."
Stop-Process -Name "FocusEnforcer.Service" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "FocusEnforcer.UI" -Force -ErrorAction SilentlyContinue

Write-Host "All FocusEnforcer components stopped."
Start-Sleep -Seconds 2
