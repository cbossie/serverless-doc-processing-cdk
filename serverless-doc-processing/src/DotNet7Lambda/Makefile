build-DotNet7Lambda:
	dotnet clean src/DotNet7Lambda
	dotnet publish src/DotNet7Lambda/DotNet7Lambda.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)