# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/MF.CostumeFramework.Reloaded/*" -Force -Recurse
dotnet publish "./MF.CostumeFramework.Reloaded.csproj" -c Release -o "$env:RELOADEDIIMODS/MF.CostumeFramework.Reloaded" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location