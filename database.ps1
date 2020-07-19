#requires -version 2
<#
.SYNOPSIS
  <Overview of script>
.DESCRIPTION
  creates docker containers using the latest images of sqlserver and mysql for testing purposes
.PARAMETER StoreNumber
    integer 
.INPUTS
  <Inputs if any, otherwise state None>
.OUTPUTS
  Logs are stored in the event viewer
.NOTES
  Version:        1.0
  Author:         Joseph McNeal Jr
  Creation Date:  2019-07-31
  Purpose/Change: Initial script development
  
.EXAMPLE
  <Example goes here. Repeat this attribute for more than one example>
#>

#---------------------------------------------------------[Initialisations]--------------------------------------------------------
[CmdletBinding()]
PARAM ( 
  [Parameter(Mandatory = $False)][Alias('n', 'name')][string]$DockerInstanceName = "mysql",
  [Parameter(Mandatory = $False)][Alias('u', 'user')][string]$DbUser = "test",
  [Parameter(Mandatory = $False)][Alias('p', 'pw')][string]$Password = "Password12!",
  [Parameter(Mandatory = $False)][Alias('v', 'version')][string]$MySqlVersion = "latest",
  [Parameter(Mandatory = $False)][Alias('d', 'database')][string]$DatabaseName = "sys",
  [switch]$Add = $false
)


#Set Error Action to Silently Continue
$ErrorActionPreference = "Stop"

#----------------------------------------------------------[Declarations]----------------------------------------------------------
# Log Source
[String]$sEventSource = "Powershell Script"

#Script Version
$sScriptVersion = "1.0"

# Current User
$sCurrentUser = whoami
$sComputerName = $env:computername


#-----------------------------------------------------------[Functions]------------------------------------------------------------

function Log {
  Param(
    [Parameter(Mandatory = $True)]$message,
    [Parameter(Mandatory = $False)][bool]$WriteToHost = $True
    
  ) 
  $sEventSourceTest = $Null
  $sEventSourceTest = Get-EventLog -list | Where-Object { $_.logdisplayname -eq $sEventSource }
  If ($Null -eq $sEventSourceTest) {
    Remove-EventLog -Source $sEventSource -ErrorAction SilentlyContinue
    New-EventLog -LogName Application -Source $sEventSource -ErrorAction SilentlyContinue
  }
  Write-EventLog -LogName "Application" -Source $sEventSource -EventID 3001 -Message $message
  if ($WriteToHost) {
    Write-Host $message
  }
}



function RequestElevation {

  # Self-elevate the script if required
  if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {
    if ([int](Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty BuildNumber) -ge 6000) {
      $CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
      Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine
      Exit
    }
  }

}

function StopAndRemoveAllDockerContainer {
  docker stop $(docker ps -a -q)
  docker rm $(docker ps -a -q)
}

function SpinUpMySqlContainer {
  docker stop $DockerInstanceName
  docker rm $DockerInstanceName
  docker run -p 3306:3306 --name $DockerInstanceName --hostname $DockerInstanceName -e MYSQL_ROOT_PASSWORD=$Password -e MYSQL_DATABASE=$DatabaseName -e MYSQL_USER=$DbUser -e MYSQL_PASSWORD=$Password -d mysql:$MySqlVersion 
  $dockerIpAddress = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $DockerInstanceName  
  Write-Output "Docker container $DockerInstanceName IpAddress is $dockerIpAddress"
}

function SpinUpSqlServerContainer {
  $containerName = "mssqlserver";
  $imageName = "mcr.microsoft.com/mssql/server:2019-latest"
  if ($IsWindows) {
    $imageName = "microsoft/mssql-server-windows-developer:2017-latest"
  }
  docker stop $containerName
  docker rm $containerName
  docker run -p 1433:1433 --name $containerName --hostname $containerName -e ACCEPT_EULA=Y -e "SA_PASSWORD=$Password" -d $imageName
  $dockerIpAddress = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $containerName  
  Write-Output "Docker container $containerName IpAddress is $dockerIpAddress"
}

StopAndRemoveAllDockerContainer

SpinUpMySqlContainer
SpinUpSqlServerContainer
