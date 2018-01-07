: Create/Update build folder

RMDIR /S /Q "Builded"
RMDIR /S /Q "SapphireEmu/bin"
RMDIR /S /Q "SapphireEmu/obj"

IF EXIST Builded (
	echo msgbox "Can't remove 'Builded' folder!" > %tmp%\tmp.vbs
	cscript /nologo %tmp%\tmp.vbs
	del %tmp%\tmp.vbs
)

if not exist "Builded\" mkdir "Builded"
if not exist "Builded\Logs\" mkdir "Builded\Logs"
if not exist "Builded\Data\" mkdir "Builded\Data"
if not exist "Builded\Data\Bin" mkdir "Builded\Data\Bin"
if not exist "Builded\Data\Base" mkdir "Builded\Data\Base"

xcopy /s "DefaultData\Reference\CSharp" "Builded\Data\Bin" /y
xcopy /s "DefaultData\Reference\Native" "Builded\Data\Bin" /y
xcopy /s "DefaultData\Database" "Builded\Data\Base" /y

echo 252480 >Builded\steam_appid.txt