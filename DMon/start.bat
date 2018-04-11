@echo off
cd "C:\Users\1115027\Documents\Visual Studio 2013\Projects\Projects\DMon\bin\Release\"
start DMon.exe "Z:\Glen_Anderson" "80" "N" "6"
start DMon.exe "Z:\Sean_Duggan" "80" "N" "6"
cd "C:\Users\1115027\Documents\Visual Studio 2013\Projects\Projects\Time\bin\Release\"
REM needs text file on desktop called times.txt
start Time.exe
exit
