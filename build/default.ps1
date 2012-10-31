Set-StrictMode -Version Latest

Task default -Depends ShowMessage

Task ShowMessage -Description "Displays a simple test message" {
    Write-Host -Fore magenta "Hi from psake"
}
