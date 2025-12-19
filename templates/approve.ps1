[CmdletBinding()]
param ()

$ErrorActionPreference = 'Stop'

Get-ChildItem -Directory (Join-Path $PSScriptRoot tests *.received) |
    % {
        Get-ChildItem -File -Recurse $_ |
            Add-Member NoteProperty -Name SrcBasePath -Value $_.FullName -PassThru |
            Add-Member ScriptProperty -Name DestBasePath -Value { [IO.Path]::ChangeExtension($this.SrcBasePath, '.verified') } -PassThru |
            Add-Member ScriptProperty -Name RelativePath -Value { Resolve-Path -Relative -RelativeBasePath $this.SrcBasePath $this } -PassThru |
            Add-Member ScriptProperty -Name DestPath -Value { Join-Path $this.DestBasePath $this.RelativePath } -PassThru |
            % {
                $dirPath = Split-Path -Parent $_.DestPath
                Write-Verbose "Creating: $dirPath"
                New-Item -ItemType Directory $dirPath -Force | Out-Null
                Write-Verbose "Copying: $_ to $($_.DestPath)"
                Copy-Item $_ $_.DestPath -Force
            }
    }
