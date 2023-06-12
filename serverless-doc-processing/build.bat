cdk synth
copy Makefile cdk.out
sam build -t .\cdk.out\ServerlessDocProcessingStack.template.json