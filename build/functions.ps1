function exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        write-output "##teamcity[buildStatus text='MSBuild Error - see build log for details' status='ERROR']"
        throw ("Exec: " + $errorMessage)
    }
}