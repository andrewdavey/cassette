function Get-CSharpFiles() {
	process {
		ls -Include "*.cs" -Exclude "*.designer.cs" -Recurse
	}
}
function Where-License([bool]$IsPresent=$(Throw "please provide the -IsPresent parameter")) {
	process {
		$firstLine = Get-Content $_ -TotalCount 1
		$containsLicense = ($firstLine -imatch '\s*#region\s+License')
		if ($containsLicense -eq $IsPresent) {
			return $_
		}
	}
}
function Apply-License($LicenseHeader=$(Throw "Please provide the path to the license header")) {
	begin {
		if (-not (Test-Path $LicenseHeader)) { Throw "The provided license header path doens't exist" }
	}
	process {
		$LicenseHeader = Resolve-Path $LicenseHeader
		$file = [System.IO.File]::ReadAllText($_.FullName)
		$licensehead = [System.IO.File]::ReadAllText($LicenseHeader)
		Set-Content $_ -Value "$licensehead`r`n$file" -encoding UTF8
		Write-Host "Updated license in file $($_.Name)"
	}
}
Get-CSharpFiles | Where-License $FALSE | Apply-License license-header.txt
