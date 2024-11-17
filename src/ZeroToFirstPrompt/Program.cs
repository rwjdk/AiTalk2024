Console.WriteLine("Zero to First Prompt (and beyond)");

#region Step 1

//Install Nuget: Microsoft.SemanticKernel

#endregion

#region Step 2

//Create an Azure Open AI Service: https://portal.azure.com/
//- Create the Service
//- Find Endpoint and API Key
//- Deploy a Model via Azure Open AI Studio
const string azureOpenAiEndpoint = "";
const string azureOpenAiApiKey = ""; //Todo - Should not be directly in real code!!! 
const string modelDeploymentName = "";

#endregion

#region Step 3

//Build a kernel (Kernel.CreateBuilder) and register AddAzureOpenAIChatCompletion

#endregion

#region Step 4 (First AI Result back)

//Make first raw call directly on the kernel
//- Ask for a Question
//- InvokePromptAsync
//- Write the Answer

#endregion

#region Step 5

//Let's introduce an agent so things become easier
//- Install one more Nuget: Microsoft.SemanticKernel.Agents.Core  (NB: It is a pre-release package so need to be turned on)
//- Create a ChatCompletionAgent instance, give it a name and bind it to the kernel
//- Choose the Agents Instructions (personal) 😉

#endregion

#region Step 6

//Let's introduce the ChatHistory Object

#endregion

#region Step 7

//Let's build a Chat-while(true) loop where is question the agent and let make the answer streaming so it feel more alive
//- Ask for input and add that to history as a user-message
//- Invoke the agent (A bit of a "funky" syntax if you are not used to iAsyncEnumerable)
//- Iterate the response-content
//- Tip: Console.OutputEncoding = Encoding.UTF8; to get Emoji to work

#endregion

#region Step 8

//What would it take to use another Model? (Demo in complete Example)?

#endregion

#region Post Code Work

//Rotate Key/Delete Azure Open AI Service
//Let's take a look in the Azure Open AI Studio (https://oai.azure.com/)

#endregion