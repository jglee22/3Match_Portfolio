# Unity Android 빌드가 멈춘 뒤 에디터를 강제 종료했을 때 실행하세요.
# PowerShell: .\Tools\KillStuckAndroidBuild.ps1

Write-Host "Gradle / Java 프로세스 종료 중..." -ForegroundColor Yellow

Get-Process -Name "java" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "gradle" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "UnityShaderCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "완료. Unity를 다시 실행한 뒤 빌드하세요." -ForegroundColor Green
Write-Host "Tip: Build And Run 대신 Build APK만 먼저 시도하세요."
