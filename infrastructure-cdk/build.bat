dotnet lambda package -pl ..\functions\src\ProcessingFunctions\ -o .\functions\ProcessingFunctions.zip
dotnet lambda package -pl ..\functions\src\SubmitToTextract\ -o .\functions\SubmitToTextract.zip
dotnet lambda package -pl ..\functions\src\ProcessTextractQueryResults\ -o .\functions\ProcessTextractQueryResults.zip
dotnet lambda package -pl ..\functions\src\RestartStepFunction\ -o .\functions\RestartStepFunction.zip
dotnet lambda package -pl ..\functions\src\ProcessTextractExpenseResults\ -o .\functions\ProcessTextractExpenseResults.zip