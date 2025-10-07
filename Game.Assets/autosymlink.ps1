# This automatically finds where Garry's Mod is installed and symlinks the necessary files for Source.NET to run.

# I want to credit this code down here, but I forgot who made the VDF parser, it's been a while
Enum State
{
    Start = 0
    Property = 1
    Object = 2
    Conditional = 3
    Finished = 4
    Closed = 5
};

Class VdfDeserializer
{
    [PSCustomObject] Deserialize([string]$vdfContent)
    {
        if([string]::IsNullOrWhiteSpace($vdfContent)) {
            throw 'Mandatory argument $vdfContent must be a non-empty, non-whitespace object of type [string]';
        }

        [System.IO.TextReader]$reader = [System.IO.StringReader]::new($vdfContent);
        return $this.Deserialize($reader);
    }

    [PSCustomObject] Deserialize([System.IO.TextReader]$txtReader)
    {
        if( !$txtReader ){
            throw 'Mandatory arguments $textReader missing.';
        }
        
        $vdfReader = [VdfTextReader]::new($txtReader);
        $result = [PSCustomObject]@{ };

        try
        {
            if (!$vdfReader.ReadToken())
            {
                throw "Incomplete VDF data.";
            }

            $prop = $this.ReadProperty($vdfReader);
            Add-Member -InputObject $result -MemberType NoteProperty -Name $prop.Key -Value $prop.Value;
        }
        finally 
        {
            if($vdfReader)
            {
                $vdfReader.Close();
            }
        }
        return $result;
    }

    [hashtable] ReadProperty([VdfTextReader]$vdfReader)
    {
        $key=$vdfReader.Value;

        if (!$vdfReader.ReadToken())
        {
            throw "Incomplete VDF data.";
        }

        if ($vdfReader.CurrentState -eq [State]::Property)
        {
            $result = @{
                Key = $key
                Value = $vdfReader.Value
            }
        }
        else
        {
            $result = @{
                Key = $key
                Value = $this.ReadObject($vdfReader);
            }
        }
        return $result;
    }

    [PSCustomObject] ReadObject([VdfTextReader]$vdfReader)
    {
        $result = [PSCustomObject]@{ };

        if (!$vdfReader.ReadToken())
        {
            throw "Incomplete VDF data.";
        }

        while ( ($vdfReader.CurrentState -ne [State]::Object) -or ($vdfReader.Value -ne "}"))
        {
            [hashtable]$prop = $this.ReadProperty($vdfReader);
            
            Add-Member -InputObject $result -MemberType NoteProperty -Name $prop.Key -Value $prop.Value;

            if (!$vdfReader.ReadToken())
            {
                throw "Incomplete VDF data.";
            }
        }

        return $result;
    }     
}

Class VdfTextReader
{
    [string]$Value;
    [State]$CurrentState;

    hidden [ValidateNotNull()][System.IO.TextReader]$_reader;

    hidden [ValidateNotNull()][char[]]$_charBuffer=;
    hidden [ValidateNotNull()][char[]]$_tokenBuffer=;

    hidden [int32]$_charPos;
    hidden [int32]$_charsLen;
    hidden [int32]$_tokensize;
    hidden [bool]$_isQuoted;

    VdfTextReader([System.IO.TextReader]$txtReader)
    {
        if( !$txtReader ){
            throw "Mandatory arguments `$textReader missing.";
        }

        $this._reader = $txtReader;

        $this._charBuffer=[char[]]::new(1024);
        $this._tokenBuffer=[char[]]::new(4096);
    
        $this._charPos=0;
        $this._charsLen=0;
        $this._tokensize=0;
        $this._isQuoted=$false;

        $this.Value="";
        $this.CurrentState=[State]::Start;
    }

    <#
    .SYNOPSIS
        Reads a single token. The value is stored in the $Value property

    .DESCRIPTION
        Returns $true if a token was read, $false otherwise.
    #>
    [bool] ReadToken()
    {
        if (!$this.SeekToken())
        {
            return $false;
        }

        $this._tokenSize = 0;

        while($this.EnsureBuffer())
        {
            [char]$curChar = $this._charBuffer[$this._charPos];

            #No special treatment for escape characters

            #region Quote
            if ($curChar -eq '"' -or (!$this._isQuoted -and [Char]::IsWhiteSpace($curChar)))
            {
                $this.Value = [string]::new($this._tokenBuffer, 0, $this._tokenSize);
                $this.CurrentState = [State]::Property;
                $this._charPos++;
                return $true;
            }
            #endregion Quote

            #region Object Start/End
            if (($curChar -eq '{') -or ($curChar -eq '}'))
            {
                if ($this._isQuoted)
                {
                    $this._tokenBuffer[$this._tokenSize++] = $curChar;
                    $this._charPos++;
                    continue;
                }
                elseif ($this._tokenSize -ne 0)
                {
                    $this.Value = [string]::new($this._tokenBuffer, 0, $this._tokenSize);
                    $this.CurrentState = [State]::Property;
                    return $true;
                }                
                else
                {
                    $this.Value = $curChar.ToString();
                    $this.CurrentState = [State]::Object;
                    $this._charPos++;
                    return $true;
                }
            }
            #endregion Object Start/End

            #region Long Token
            $this._tokenBuffer[$this._tokenSize++] = $curChar;
            $this._charPos++;
            #endregion Long Token            
        }

        return $false;
    }

    [void] Close()
    {
        $this.CurrentState = [State]::Closed;
    }

    <#
    .SYNOPSIS
        Seeks the next token in the buffer.

    .DESCRIPTION
        Returns $true if a token was found, $false otherwise.
    #>
    hidden [bool] SeekToken()
    {
        while($this.EnsureBuffer())
        {
            # Skip Whitespace
            if( [char]::IsWhiteSpace($this._charBuffer[$this._charPos]) )
            {
                $this._charPos++;
                continue;
            }

            # Token
            if ($this._charBuffer[$this._charPos] -eq '"')
            {
                $this._isQuoted = $true;
                $this._charPos++;
                return $true;
            }

            # Comment
            if ($this._charBuffer[$this._charPos] -eq '/')
            {
                $this.SeekNewLine();
                $this._charPos++;
                continue;
            }            

            $this._isQuoted = $false;
            return $true;
        }

        return $false;
    }

    <#
    .SYNOPSIS
        Seeks the next newline in the buffer.

    .DESCRIPTION
        Returns $true if \n was found, $false otherwise.
    #>
    hidden [bool] SeekNewLine()
    {
        while ($this.EnsureBuffer())
        {
            if ($this._charBuffer[++$this._charPos] == '\n')
            {
                return $true;
            }
        }
        return $false;
    }
    
    <#
    .SYNOPSIS
        Refills the buffer if we're at the end.

    .DESCRIPTION
        Returns $false if the stream was empty, $true otherwise.
    #>
    hidden [bool]EnsureBuffer()
    {
        if($this._charPos -lt $this._charsLen -1)
        {
            return $true;
        }

        [int32] $remainingChars = $this._charsLen - $this._charPos;
        $this._charBuffer[0] = $this._charBuffer[($this._charsLen - 1) * $remainingChars]; #A bit of mathgic to improve performance by avoiding a conditional.
        $this._charsLen = $this._reader.Read($this._charBuffer, $remainingChars, 1024 - $remainingChars) + $remainingChars;
        $this._charPos = 0;

        return ($this._charsLen -ne 0);
    }
}

$steamInstallPath = Get-ItemProperty -Path "HKLM:\SOFTWARE\Valve\Steam" -Name "InstallPath" -ErrorAction SilentlyContinue

if (-not $steamInstallPath) {
    $steamInstallPath = Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432NODE\Valve\Steam" -Name "InstallPath" -ErrorAction SilentlyContinue
}

$gmodLocalPath  = "\steamapps\common\GarrysMod"
$gmodAppID      = 4000;

function Link-SDN-Asset {
    param (
        [string]$SDN_ABS,
        [string]$GMOD_ABS,
        [string]$SDN_LOCAL,
        [string]$GMOD_LOCAL
    )

    $linkPath   = Join-Path $SDN_ABS $SDN_LOCAL
    $targetPath = Join-Path $GMOD_ABS $GMOD_LOCAL
	
	try {
        $link = New-Item -ItemType SymbolicLink -Path $linkPath -Target $targetPath -Force -ErrorAction Stop
        Write-Host "symbolic link created for $linkPath <<===>> $targetPath" -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to create symbolic link for $linkPath" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor DarkRed
    }
}

if ($steamInstallPath) {
    $libraryFoldersPath = Join-Path -Path $steamInstallPath.InstallPath -ChildPath "steamapps\libraryfolders.vdf"
    
    if (Test-Path $libraryFoldersPath) {
        $fileContent = Get-Content -Path $libraryFoldersPath
        $vdf = [VdfDeserializer]::new();    
        $result = $vdf.Deserialize($fileContent);

        $result.libraryfolders.PSObject.Properties | ForEach-Object {
            $folder     = $_.Value;
            $folderPath = $folder.path -replace '\\\\', '\';
            $folderApps = $folder.apps;

            $folderApps.PSObject.Properties | ForEach-Object {
                $appID = $_.Name
                if($appID -eq $gmodAppID){
					$gmodAbsPath = Join-Path -Path $folderPath -ChildPath $gmodLocalPath;
					$sdnAbsPath = $PWD.Path;

                    Write-Host "Garry's Mod ($gmodAppID) found!"
                    Write-Host "    Garry's Mod Path             :  $gmodAbsPath"
                    Write-Host "    Source.NET/Game.Assets Path  :  $sdnAbsPath"
					
					Write-Host "Ensure that the Garry's Mod path points to the GarrysMod folder (not garrysmod!), and that the Source.NET path points to the Game.Assets folder.";
					Write-Host 'Verify that this information looks correct, then press any key to continue.';
					$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
					
					# steam.inf used for networking compat
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\steam.inf" -GMOD_LOCAL "garrysmod\steam.inf"
					
					# Garry's Mod content
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\garrysmod_dir.vpk" -GMOD_LOCAL "garrysmod\garrysmod_dir.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\garrysmod_000.vpk" -GMOD_LOCAL "garrysmod\garrysmod_000.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\garrysmod_001.vpk" -GMOD_LOCAL "garrysmod\garrysmod_001.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\garrysmod_002.vpk" -GMOD_LOCAL "garrysmod\garrysmod_002.vpk"
					
					# HL2 content
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_dir.vpk" -GMOD_LOCAL "sourceengine\content_hl2_dir.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_000.vpk" -GMOD_LOCAL "sourceengine\content_hl2_000.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_001.vpk" -GMOD_LOCAL "sourceengine\content_hl2_001.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_002.vpk" -GMOD_LOCAL "sourceengine\content_hl2_002.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_003.vpk" -GMOD_LOCAL "sourceengine\content_hl2_003.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_004.vpk" -GMOD_LOCAL "sourceengine\content_hl2_004.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_005.vpk" -GMOD_LOCAL "sourceengine\content_hl2_005.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\content_hl2_006.vpk" -GMOD_LOCAL "sourceengine\content_hl2_006.vpk"
					
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_misc_dir.vpk" -GMOD_LOCAL "sourceengine\hl2_misc_dir.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_misc_000.vpk" -GMOD_LOCAL "sourceengine\hl2_misc_000.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_misc_001.vpk" -GMOD_LOCAL "sourceengine\hl2_misc_001.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_misc_002.vpk" -GMOD_LOCAL "sourceengine\hl2_misc_002.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_misc_003.vpk" -GMOD_LOCAL "sourceengine\hl2_misc_003.vpk"
					
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_sound_misc_dir.vpk" -GMOD_LOCAL "sourceengine\hl2_sound_misc_dir.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_sound_misc_000.vpk" -GMOD_LOCAL "sourceengine\hl2_sound_misc_000.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_sound_misc_001.vpk" -GMOD_LOCAL "sourceengine\hl2_sound_misc_001.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_sound_misc_002.vpk" -GMOD_LOCAL "sourceengine\hl2_sound_misc_002.vpk"
					
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_dir.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_dir.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_000.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_000.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_001.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_001.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_002.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_002.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_003.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_003.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_004.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_004.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_005.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_005.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_006.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_006.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_007.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_007.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_008.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_008.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_009.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_009.vpk"
					Link-SDN-Asset -SDN_ABS $sdnAbsPath -GMOD_ABS $gmodAbsPath -SDN_LOCAL "hl2\hl2_textures_010.vpk" -GMOD_LOCAL "sourceengine\hl2_textures_010.vpk"
					
					Write-Host "Done!";
					Write-Host -NoNewLine 'Press any key to exit.';
					$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
					
                    exit
                }
            }
        }
    } else {
        Write-Host "File not found: $libraryFoldersPath"
    }
} else {
    Write-Host "Steam install path not found."
}

Write-Host "Something went wrong!";
Write-Host -NoNewLine 'Press any key to exit.';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');