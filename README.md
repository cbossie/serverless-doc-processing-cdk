# Serverless Document Analysis with .NET

## Introduction
This is a sample project that is meant to show a full end-to-end serverless application using entirely AWS Serverless services. It uses [Amazon Textract](https://aws.amazon.com/pm/textract) to analyze a PDF file. You can preconfigure natural language queries that Textract will attempt to answer (e.g. 'What is the date of service of this invoice'). Also it will submit the document for expense document analysis, and return data about the document as a set of expense metadata documents.

This demonstrates how you can use .NET for an end to end serverless document processing solution in AWS. This README will detail the solution in full. The service can be deployed into an AWS account, and because it is self contained, can serve as an addon to an existing application.

## What specifically does this sample demonstrate?
This solution is meant to be useful in real-world scenario, in which multiple technologies, techniques, and services are used. Specifically, this document analysis tool showcases the technologies listed below.

1. ### AWS Lambda with .NET
    - Custom runtime functions using .NET 8.0
    - Observability implemented using [Powertools for AWS Lambda (.NET)](https://docs.powertools.aws.dev/lambda/dotnet/)
    - [Lambda Annotations Framework](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md) to implement dependency injection, with source generation to automatically create the "Main" method.
    
1. ### Infrastructure as Code with CDK
    - All infrastructure is expressed with [AWS CDK (C#/.NET)](https://docs.aws.amazon.com/cdk/v2/guide/work-with-cdk-csharp.html) with .NET 8.0.

1. ### Serverless AWS Services
    - Data and configuration are stored in an [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) table. Data access uses the [.NET Object persistence model](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html) to simplify data access with POCO objects.
    - The Lambda functions are orchestrated using an [AWS Step Function](https://aws.amazon.com/step-functions/) standard workflow. Standard workflow was chosen because it supports the [Wait for Callback (task token)](https://docs.aws.amazon.com/step-functions/latest/dg/connect-to-services.html#connect-to-services-integration-patterns) integration pattern.
    - [Amazon Textract](https://aws.amazon.com/dynamodb/) provides document analysis (standard and expense) capabilities.
    - An [Amazon Event Bridge rule](https://docs.aws.amazon.com/eventbridge/latest/userguide/eb-rules.html) is used to automatically trigger the workflow when a document is uploaded to an [Amazon S3](https://aws.amazon.com./s3) bucket.
    - Feedback is provided to the client application through the use of two [Amazon SQS](https://aws.amazon.com/sqs) queues

## Overview of the solution

This is an overview of the process. The names of the resources are generic, since each deployment will yield resources with different physical names (to avoid resource name collission). Sone design decisions are noted below, but there are alternate ways of accomplishing some of the items.
![Overview of serverless document analysis](/assets/doc-analysis-overview.jpg)

This application is self contained. We will refer to an external application that integrates with this system as the "client application". There can be more than one client application, and a client application that provides input (i.e. uploads a file) may be different than an application that responds to the output of the system.
1. A client application writes a PDF to the `InputBucket` S3 bucket. 

    If the service has been configured to use natural language queries (explanation below), a subset of them can be specified using a colon separate list of query keys, supplied as a tag on the uploaded S3 object. For example:

    `Tag: "Queries"`

    `Value: 'q1:q2:q3'`

    If no queries are supplied, then all configured queries will be used.

    _Note: A client application must have permissions to write files to the InputBucket. A CloudFormation output is created when this is deployed, `inputBucketPolicyOutput`, that provides an example IAM policy that you can use to allow access to the bucket._

2. An EventBridge rule triggers the Step Function. 

3. The Step Function definition can be seen here. It consists of seven Lambda function integrations and two SQS integrations. Any unrecoverable errors (from any of the Lambda functions) are caught and sent to the `FailureFunction` function, which then writes a message to the `FailureQueue` with details for the client.

    ![Step Function Definition](/assets/stepfunctions_graph.png)

4. The EventBridge message is parsed by the 


## Codebase

## Deploying in your environment

## Cleanup


## TODO
These are some items that will be added at a later date to make the solution more extensible
1. Create a Systems Manager Parameter that will parameterize the following items:
    - The name of the tag that is used to specify queries to be applied to the document analysis (currently hardcoded to 'Queries')
1. 