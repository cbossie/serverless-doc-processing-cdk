dotnet lambda package -pl ..\functions\src\InitializeProcessing\ -o .\functions\InitializeProcessing.zip
dotnet lambda package -pl ..\functions\src\SubmitToTextract\ -o .\functions\SubmitToTextract.zip
dotnet lambda package -pl ..\functions\src\ProcessTextractResults\ -o .\functions\ProcessTextractResults.zip
dotnet lambda package -pl ..\functions\src\RestartStepFunction\ -o .\functions\RestartStepFunction.zip