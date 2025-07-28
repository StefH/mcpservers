param(
    [Parameter(Mandatory = $true)]
    [string]$ServerJsonPath,
    
    [Parameter(Mandatory = $true)]
    [string]$DirectoryBuildPropsPath
)

# Extract version from Directory.Build.props
$version = Select-String -Path $DirectoryBuildPropsPath -Pattern '<Version>(.*?)</Version>' | 
    ForEach-Object { $_.Matches[0].Groups[1].Value }

if (-not $version) {
    Write-Error "Could not find version in $DirectoryBuildPropsPath"
    exit 1
}

Write-Host "Found version: $version"

# Update server.json
if (-not (Test-Path $ServerJsonPath)) {
    Write-Error "Server.json not found at: $ServerJsonPath"
    exit 1
}

try {
    $json = Get-Content $ServerJsonPath | Out-String | ConvertFrom-Json
    
    # Update version fields
    if ($json.packages -and $json.packages.Length -gt 0) {
        $json.packages[0].version = $version
        Write-Host "Updated packages[0].version to: $version"
    }
    
    if ($json.version_detail) {
        $json.version_detail.version = $version
        Write-Host "Updated version_detail.version to: $version"
    }
    
    # Save updated JSON
    $json | ConvertTo-Json -Depth 10 | Set-Content $ServerJsonPath -Encoding UTF8
    Write-Host "Successfully updated $ServerJsonPath"
}
catch {
    Write-Error "Failed to update server.json: $_"
    exit 1
}