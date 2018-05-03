
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Modification Part Document


## Target
The new architecture will try best to use the swagger spec. Trying to generate more part in the storage emulator that can be inferred from the swagger spec.

## Possible Ways 
Because of something like **x-ms-paths** that breaks the original OpenAPI definition. The **only way** to generate code is by using [AutoRest](https://github.com/Azure/autorest)

## Demo Steps
1. Clone this [Repo](https://github.com/gamesgao/autorest.nodejs.git
```shell
git clone https://github.com/gamesgao/autorest.nodejs.git
```
2. Get into the folder and install the dependencies
```shell
cd autorest.nodejs
npm install
```
3. Generate server code 
```shell
autorest --input-file=queue-storage-2017-07-29.json --nodejs --output-folder=temp --u
se="/where/is/autorest.nodejs"
```
4. Replace the interface and non-generated code
Copy the two folder `non-auto-genrated/actions` and `non-auto-genrated/providers` into the `<generated code folders>/lib/` and replace the generated `actions` folder
5. Run the Server
```shell
npm install
npm start
```
6. Try the function of this server
I only implemented createQueue, createMessage and getMessage API. You can use the following code to give it a try.

> Package.json
> ```json
> {
>   "name": "test-server",
>   "version": "1.0.0",
>   "description": "",
>   "main": "index.js",
>   "scripts": {
>     "start": "node index.js"
>   },
>   "author": "",
>   "license": "ISC",
>   "dependencies": {
>     "azure-storage": "^2.8.2"
>   }
> }
> ```

> index.js
> ```javascript
> const azure = require('azure-storage')
> let connectionString = 'DefaultEndpointsProtocol=http;AccountName=test;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:3000/;'
> let queueService = azure.createQueueService(connectionString)
> queueService.createQueue('useless', null, (error, result, respose) => {
>   if (error) {
>     console.log(error)
>   } else {
>     console.log(result)
>     queueService.createMessage('useless', 'test message', null, (error, result, respose) => {
>       if (error) {
>         console.log(error)
>       } else {
>         console.log(result)
>         queueService.getMessage('useless', null, (error, result, respose) => {
>           if (error) {
>             console.log(error)
>           } else {
>             console.log(result.messageText)
>           }
>         })
>       }
>     })
>   }
> })
> ```
Create the two file and put them together
7. Run the test code
```shell
npm install
npm start
```
You can see the result that prove that you can access the server by using the official client API

## Quick Summary for AutoRest
In a word: **Code Generation Tools**.
Officially, it can only generate client code by using official extensions. But it does not limit what extensions can do. 

### Architecture
AutoRest.Core -> AutoRest.Common -> AutoRest.Nodejs (Code Generation)
Code gneration part in the last stage. So changing the code of NodeJS extension can make the server code generation.

## Prototype
Based on the Nodejs extension, I change the code generation part to generate server code. 

Primarily, I implement the storage emulator in four parts.
- Route 
  - redirect the request
- Serilization (no default value)
  - Extract the paramters from query, path, body and headers
- Validation (Only validated, no response)
  - Validate the request paramters
- Action (Only interface generated)
  - Do the real work

## Estimated Efforts
Most efforts will in the extension implementation. But this can refer to other extensions (even copy)
The kernel part (template and code generation) will save most repeating works
The action part in my architecture can use actions from [Azurite](https://github.com/azure/azurite) with little changes.

## Problems
- Router rules is not right, path+method cannot specify a operation. So how to generate the router need to reconsider. 
  - Suggestion: There are 2 ways to avoid conflicts.
    - Pay atention to order of different operation. Make the common one at the last of the paths in the swagger spec. 
    - Analysis the path and query paramters to determine the router. 
- response definition in swagger is not used. Because I use actions from [Azurite](https://github.com/azure/azurite) directly. This part may need reconsider
- No path parameter in swagger spec
  - Suggestion: Add it in the swagger spec.
- Suggestion: Seperate this project into server extension and storage emulator extension. 
  - Because server generation is a seperate part that can contribute to autorest team. 

# AutoRest extension configuration

``` yaml
use-extension:
  "@microsoft.azure/autorest.modeler": "2.3.44"

pipeline:
  nodejs/imodeler1:
    input: openapi-document/identity
    output-artifact: code-model-v1
    scope: nodejs
  nodejs/commonmarker:
    input: imodeler1
    output-artifact: code-model-v1
  nodejs/cm/transform:
    input: commonmarker
    output-artifact: code-model-v1
  nodejs/cm/emitter:
    input: transform
    scope: scope-cm/emitter
  nodejs/generate:
    plugin: nodejs
    input: cm/transform
    output-artifact: source-file-nodejs
  nodejs/transform:
    input: generate
    output-artifact: source-file-nodejs
    scope: scope-transform-string
  nodejs/emitter:
    input: transform
    scope: scope-nodejs/emitter

scope-nodejs/emitter:
  input-artifact: source-file-nodejs
  output-uri-expr: $key

output-artifact:
- source-file-nodejs
```
