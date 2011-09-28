cls
$env:SEE_MASK_NOZONECHECKS = 1
$Clients = New-Object Collections.Generic.List[string];
$xmlDoc = New-Object Xml.XmlDocument;
try {
  $loc = Get-Location;
  $xmlDoc.Load("$loc\ClientList.xml");
  #$xmlDoc.Load("C:\Users\aminor\Desktop\Jenzabar-PowerFaids Maintenance\trunk\config\ClientList.xml");
  $machineName = [System.Environment]::MachineName;
  
  foreach ($node in $xmlDoc.DocumentElement.SelectNodes("clients/name")) {
    $Clients.Add($node.InnerText.Trim().ToUpper());
  }
  
  if ($Clients.Contains($machineName))
  {
  }
  Else
  	{
		Remove-Item env:\SEE_MASK_NOZONECHECKS
		Exit
	}
}
catch {

}

$WinInst45 = "\\hadbsvc.fhtc.lan\Jenzabar\SQL2008\x86\redist\Windows Installer\x86\INSTMSI45XP.EXE"
$NETFrmWk35SP1x86 = "\\hadbsvc.fhtc.lan\Jenzabar\NET_Frmwk\dotNetFx35setupx86.exe"
$NETFrmWk35SP1x64 = "\\hadbsvc.fhtc.lan\Jenzabar\NET_Frmwk\dotNetFx35setupx64.exe"
$NETFrmWk4Full = "\\hadbsvc.fhtc.lan\Jenzabar\NET_Frmwk\dotNetFx40_Full_x86_x64.exe"
$OSinfo = get-wmiobject Win32_OperatingSystem
If ($OSinfo.Version -eq '6.1.7601')
	{
		If (Test-Path 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full')
			{
				$keyprop = Get-ItemProperty 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full'
				If ($keyprop.Install -ge '1')
					{
					}
				Else
					{
						$process = [System.Diagnostics.Process]::Start($NETFrmWk4Full, "/q")
						$process.WaitForExit()
						Restart-Computer -Force
					}
			}
		Else
			{
				$process = [System.Diagnostics.Process]::Start($NETFrmWk4Full,"/q")
				$process.WaitForExit()
				Restart-Computer -Force
			}
	}
Else
	{
	$msiVers = (Get-Command C:\Windows\System32\msi.dll).FileVersionInfo.FileVersion
	If ($msiVers -ge '4.5')
		{
		}
	Else
		{
			$process = [System.Diagnostics.Process]::Start($WinInst45,"/quiet /norestart /overwriteoem /nobackup /forceappsclose")
			$process.WaitForExit()
			Restart-Computer -Force		
		}
	If (Test-Path 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v3.5')
	{
		$keyprop = Get-ItemProperty 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v3.5'
		If ($keyprop.SP -ge '1')
			{
			}
		Else
			{
				$process = [System.Diagnostics.Process]::Start($NETFrmWk35SP1x86,"/q")
				$process.WaitForExit()
				Restart-Computer -Force
			}
	}
	
	Else
		{
			$process = [System.Diagnostics.Process]::Start($NETFrmWk35SP1x86,"/q")
			$process.WaitForExit()
			Restart-Computer -Force
		}
	If (Test-Path 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full')
		{
			$keyprop = Get-ItemProperty 'HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full'
			If ($keyprop.Install -ge '1')
				{
				}
			Else
				{
					$process = [System.Diagnostics.Process]::Start($NETFrmWk4Full,"/q")
					$process.WaitForExit()
					Restart-Computer -Force
				}
		}
	Else
		{
			$process = [System.Diagnostics.Process]::Start($NETFrmWk4Full,"/q")
			$process.WaitForExit()
			Restart-Computer -Force
		}
	}
#Launch Damon's EXE HERE
Remove-Item env:\SEE_MASK_NOZONECHECKS