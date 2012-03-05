[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

$pkgDefContent = @"

; Visual Basic
[`$RootKey`$\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}\FileExtensions\.coffee]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}\FileExtensions\.sass]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}\FileExtensions\.scss]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}\FileExtensions\.combine]
"DefaultBuildAction"="Content"

; C#
[`$RootKey`$\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\FileExtensions\.coffee]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\FileExtensions\.sass]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\FileExtensions\.scss]
"DefaultBuildAction"="Content"

[`$RootKey`$\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\FileExtensions\.combine]
"DefaultBuildAction"="Content"

"@

$PkgDefExists = Test-Path "$env:PROGRAMFILES\Microsoft Visual Studio 10.0\Common7\IDE\Extensions\SassAndCoffee.pkgdef" 

If ($PkgDefExists -eq $False) {
    $dialogResult = [System.Windows.Forms.MessageBox]::Show(
        "SassAndCoffee's file definitions aren't installed - this makes using Sass/SCSS and CoffeeScript files less frustrating in VS. Do you want to install it?", 
        "Missing SaC PkgDef", 
        [Windows.Forms.MessageBoxButtons]::YesNo,
        [Windows.Forms.MessageBoxIcon]::Question)
        
    If ($dialogResult -eq [Windows.Forms.DialogResult]::Yes) {
        $pkgDefContent | out-file "$env:TEMP\SassAndCoffee.pkgdef"
        
        $psi = new-object System.Diagnostics.ProcessStartInfo "cmd"
        $psi.Verb = "runas"
        $psi.Arguments = '/C copy "%TEMP%\SassAndCoffee.pkgdef" "%PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\IDE\Extensions"'
        [System.Diagnostics.Process]::Start($psi)
    }
}
