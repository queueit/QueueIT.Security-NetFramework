Set-Location -Path .. -PassThru

& GitVersion /updateassemblyinfo

$ver = & GitVersion.exe /output json /showvariable MajorMinorPatch

Set-Location -Path Build -PassThru

& .\Build.bat

"Version: " + $ver

#using simple text replacement
$con = Get-Content .\QueueIT.Security.nuspec
$con | % { $_.Replace("<version></version>", "<version>" + $ver + "</version>") } | Set-Content .\QueueIT.Security.nuspec

"Con " + $con

& nuget pack QueueIT.Security.nuspec

$loc = "QueueIT.Security." + $ver + ".nupkg"

"Location " + $loc

& nuget push $loc 87c4bafe-830a-4147-9e8f-422bf70a69ea -Source https://www.myget.org/F/tessitura/api/v2/package