# Interact with Azure Cosmos DB with SDK

This is a part of my learning Azure Cosmos DB with SDK. In this practise, I have implemented the basic opeartion that we need to interact with Cosmos DB with SDK.

## Overview
Update appsettings.json file to put your correct endpoint and primary/master key. After that the project is ready to use. Under main function in program.cs, uncomment each function to list the database or containers. From the SDK, I have also shown how to create a documents or update the documents.
In the code you can also see that we can use different ways to create a documents. In addition, you can also notice that we can get documents in a stateful and stateless manner based on your requirement. Also we have streamed version to get the documents from Cosmos DB to reduce the cost. I have used the shared single instance to connect to the comsos db and also showed how to use a different instance.

## Technology used
I have used .net core and added two nuget package:
1. Microsoft.Azure.Cosmos
2. Microsoft.Extensions.Configuration.Json

## Further Improvement
The code are part of learning process. We can take this as a reference and implement based on needs. 
