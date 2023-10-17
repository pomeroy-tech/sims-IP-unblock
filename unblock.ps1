Param (
    [Parameter(Mandatory = $true, ParameterSetName = "IP")]
    [ValidateNotNullOrEmpty()]
    [String]
    $IP
}

if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) 
{
    if ([int](Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty BuildNumber) -ge 6000) 
    {
     $CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
     Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine
     Exit
    }
}

Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList "C:\Program Files\Bitvise SSH Server\BssStat.exe -u $IP"