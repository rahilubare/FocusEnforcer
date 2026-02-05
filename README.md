# FocusEnforcer Deployment & Verification Guide

## 1. Build Instructions
Prerequisites: .NET 8.0 SDK.

1.  Navigate to the solution directory:
    ```powershell
    cd c:\tmp\FocusEnforcer
    ```
2.  Restore dependencies:
    ```powershell
    dotnet restore
    ```
3.  Build the solution:
    ```powershell
    dotnet build -c Release
    ```
4.  Publish the Service (Standalone):
    ```powershell
    dotnet publish FocusEnforcer.Service -c Release -o .\publish\service
    ```
5.  Publish the UI:
    ```powershell
    dotnet publish FocusEnforcer.UI -c Release -o .\publish\ui
    ```

## 2. Installation (Service)
**WARNING**: Requires Administrator privileges.

1.  Open PowerShell as Administrator.
2.  Create the Windows Service:
    ```powershell
    New-Service -Name "FocusEnforcerService" -BinaryPathName "C:\tmp\FocusEnforcer\publish\service\FocusEnforcer.Service.exe" -Description "Enforces productivity rules by blocking processes and websites." -DisplayName "Focus Enforcer Service" -StartupType Automatic
    ```
3.  Start the Service:
    ```powershell
    Start-Service "FocusEnforcerService"
    ```
4.  Verify it is running:
    ```powershell
    Get-Service "FocusEnforcerService"
    ```

## 3. Configuration & Usage
### Dashboard (UI)
1.  Run `FocusEnforcer.UI.exe` from the publish folder.
2.  **Add Rule**:
    *   Select "Process" or "Website".
    *   Enter "notepad" (for process) or "facebook.com" (for website).
    *   Click "Add Rule".
3.  The UI sends a command to the Service via Named Pipes ("REFRESH").
4.  The Service picks up the new rule from the shared SQLite database (`C:\ProgramData\FocusEnforcer\data.db`).

### Manual Configuration (Advanced)
The application uses a SQLite database. You can inspect it using any SQLite viewer (if not encrypted yet) at:
`C:\ProgramData\FocusEnforcer\data.db`

## 4. Security Considerations
*   **System Privileges**: The Service runs as SYSTEM. This is necessary to kill processes from other users and edit the `hosts` file.
*   **Anti-Tampering**:
    *   **Process**: Usage of `protected` prefix or Critical Process attribute (BSOD on kill) is possible but risky. Currently, the service automatically restarts if set to "Automatic" and recovered by Windows SCM.
    *   **File Permissions**: The installation directory and `ProgramData` database should be ACL-locked so only SYSTEM and Administrators can write to them.
*   **Encryption**: Sensitive config (passwords) are hashed. Blocklist data is encrypted using DPAPI (Machine Scope) before database insertion (implemented in `CryptoHelper`).

## 5. Troubleshooting
*   **Service won't start**: Check Event Viewer -> Windows Logs -> Application.
*   **Blocking not working**:
    *   Ensure the process name matches (e.g., `notepad` vs `notepad.exe`).
    *   Ensure flushing DNS cache (`ipconfig /flushdns`) for website blocks.
*   **IPC Error**: If the UI complains about connection, ensure the Service is running.

## 6. Uninstall
```powershell
Stop-Service "FocusEnforcerService"
sc.exe delete "FocusEnforcerService"
```
"# FocusEnforcer" 
