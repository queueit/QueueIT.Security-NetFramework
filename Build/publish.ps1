Set-Location -Path .. -PassThru

& GitVersion /updateassemblyinfo

Set-Location -Path Build -PassThru

& .\Build.bat

$ver = & GitVersion.exe /output json /showvariable MajorMinorPatch

"Version: " + $ver

#using simple text replacement
$con = Get-Content .\QueueIT.Security.nuspec
$con | % { $_.Replace("<version>1.0.0</version>", "<version>" + $ver + "</version>") } | Set-Content .\QueueIT.Security.nuspec

& nuget pack QueueIT.Security.nuspec

$loc = "QueueIT.Security." + $ver + ".nupkg"

& nuget push $loc 87c4bafe-830a-4147-9e8f-422bf70a69ea -Source https://www.myget.org/F/tessitura/api/v2/package