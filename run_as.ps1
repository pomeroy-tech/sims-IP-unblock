 <#

.SYNOPSIS

Run command as another user.

.DESCRIPTION

Run batch or PowerShell command as another user.

.PARAMETER Command

The batch command you'd like to execute as another user.

.PARAMETER ScriptBlock

The PowerShell command you'd like to execute as another user.

.PARAMETER Username

Run the command as what user.

.PARAMETER Password

Password of the user.

.PARAMETER Credential

PowerShell credential of the user, it can be generated by `Get-Credential`.

.PARAMETER Wait

Wait command to complete or not.
Command output would not be displayed if it is not specified.

#>

Param (
    [Parameter(Mandatory = $true, ParameterSetName = "bat-user-password")]
    [Parameter(Mandatory = $true, ParameterSetName = "bat-credential")]
    [ValidateNotNullOrEmpty()]
    [String]
    $Command,

    [Parameter(Mandatory = $true, ParameterSetName = "ps-user-password")]
    [Parameter(Mandatory = $true, ParameterSetName = "ps-credential")]
    [ScriptBlock]
    $ScriptBlock,

    [Parameter(Mandatory = $true, ParameterSetName = "bat-user-password")]
    [Parameter(Mandatory = $true, ParameterSetName = "ps-user-password")]
    [ValidateNotNullOrEmpty()]
    [String]
    $Username,

    [Parameter(Mandatory = $true, ParameterSetName = "bat-user-password")]
    [Parameter(Mandatory = $true, ParameterSetName = "ps-user-password")]
    [ValidateNotNullOrEmpty()]
    [String]
    $Password,

    [Parameter(Mandatory = $true, ParameterSetName = "bat-credential")]
    [Parameter(Mandatory = $true, ParameterSetName = "ps-credential")]
    [PSCredential]
    $Credential,

    [Switch]
    $Wait
)

$IsCurrentAdminUser = $([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')

# Find a dir that every user have full access to
$TempDir = "$env:SystemDrive\Users\Public\run_as"
if (-not (Test-Path -Path $TempDir)) {
    $null = New-Item -Path $TempDir -ItemType Directory
    attrib +h $TempDir
}

# Generate a uniq id for problem tracking
$ExecId = Get-Random -Maximum 99999999 -Minimum 10000000

# Temp files
$UserScriptPrefix = "$TempDir\$ExecId-UserScript"
$UserStdOut = "$TempDir\$ExecId-UserStdOut.log"
$UserErrOut = "$TempDir\$ExecId-UserErrOut.log"
$WaitFile = "$TempDir\$ExecId-Running"

$ExecScript = "$TempDir\$ExecId-Exec.ps1"
$CmdToExec = "Start-Process"

if ($PsCmdlet.ParameterSetName.StartsWith('bat')) {
    $UserScript = $UserScriptPrefix + '.bat'
    $Command |Out-File -FilePath $UserScript -Encoding ascii

    $CmdToExec += " cmd.exe -ArgumentList '/c $UserScript'"
} elseif ($PsCmdlet.ParameterSetName.StartsWith('ps')) {
    $UserScript = $UserScriptPrefix + '.ps1'
    $ScriptBlock |Out-File -FilePath $UserScript -Encoding ascii

    $CmdToExec += " PowerShell.exe -ArgumentList '-file $UserScript'"
}

if ($PsCmdlet.ParameterSetName.EndsWith('user-password')) {
    $SecPassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
    $Credential = New-Object -TypeName System.Management.Automation.PSCredential ($Username, $SecPassword)
}

$CmdToExec += " -WorkingDirectory $env:SystemDrive\"

if ($Wait) {
    # Redirect output only if -Wait flag is set
    $CmdToExec += " -RedirectStandardError $UserErrOut"
    $CmdToExec += " -RedirectStandardOutput $UserStdOut"

    if ($IsCurrentAdminUser) {
        # -Wait parameter of Start-Process only works with admin users
        # Using it with non-admin users will get an "Access is denied" error
        $CmdToExec += " -Wait"
    }
}

$script = @'
Param($Cred)
"" | Out-File -FilePath {0}

try {{
    {1} -Credential $Cred
}} catch {{
    Write-Host $_
}} finally {{
    Remove-Item -Path {0} -Force -Confirm:$false
}}
'@ -f $WaitFile, $CmdToExec

$Script |Out-File -FilePath $ExecScript -Encoding ascii

try {
    & $ExecScript -Cred $Credential
} catch {
    Write-Host $_
} finally {
    if ($Wait) {
        if (-not $IsCurrentAdminUser) {
            # Impelment the wait by file monitoring for non-admin users
            do {
                Start-Sleep -Seconds 1
            } while (Test-Path -Path $WaitFile)
    
            # Wait output are write to files completely
            Start-Sleep -Seconds 1
        }

        # Read command output from files
        if (Test-Path -Path $UserStdOut) {
            Get-Content -Path $UserStdOut
        }

        if (Test-Path -Path $UserErrOut) {
            Get-Content -Path $UserErrOut
        }
    }

    Remove-Item -Path "$TempDir\$ExecId-*" -Force -Confirm:$false -ErrorAction SilentlyContinue
}