# Check for Administrator privileges
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Running as Administrator to fix permissions..."
    Start-Process powershell.exe "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$dataDir = "C:\ProgramData\FocusEnforcer"
$dbFile = "$dataDir\data.db"

Write-Host "Stopping Services to release locks..."
Stop-Service "FocusEnforcerService" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "FocusEnforcer.UI" -Force -ErrorAction SilentlyContinue

# Create directory if missing (it should exist)
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force
}

# Grant "Users" group Full Control to the directory
$acl = Get-Acl $dataDir
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Users","FullControl","ContainerInherit,ObjectInherit","None","Allow")
$acl.AddAccessRule($rule)
Set-Acl $dataDir $acl
Write-Host "Updated Directory Permissions."

if (Test-Path $dbFile) {
    # Grant "Users" group Full Control to the file explicitly
    $acl = Get-Acl $dbFile
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Users","FullControl","Allow")
    $acl.AddAccessRule($rule)
    Set-Acl $dbFile $acl
    Write-Host "Updated Database File Permissions."
}

Write-Host "Restarting Service..."
Start-Service "FocusEnforcerService"

Write-Host "--------------------------------------------------------"
Write-Host "Permissions Fixed. You can now use the Dashboard."
Write-Host "--------------------------------------------------------"
Start-Sleep -Seconds 5
