robocopy "C:\Users\erlendd\Dropbox\Mine program\C#\JetReader\ReaderJS\Src" "C:\Code\ReaderJs\Src" /E /is /it > nul

cd "C:\Code\ReaderJs\Src\"
C:

call grunt build

robocopy "C:\Code\ReaderJs\Build" "C:\Users\erlendd\Dropbox\Mine program\C#\JetReader\ReaderJS\Build" /E /is /it > nul