dotnet lambda package -pl .\src\InitializeProcessing\ -o .\functions\InitializeProcessing.zip
dotnet lambda package -pl .\src\SubmitToTextract\ -o .\functions\SubmitToTextract.zip
dotnet lambda package -pl .\src\ProcessTextractResults\ -o .\functions\ProcessTextractResults.zip
dotnet lambda package -pl .\src\RestartStepFunction\ -o .\functions\RestartStepFunction.zip