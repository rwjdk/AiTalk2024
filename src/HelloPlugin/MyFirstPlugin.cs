using Microsoft.SemanticKernel;

namespace HelloPlugin;

public class MyFirstPlugin
{
    //Todo - Info + Actions (File Interactions)
    [KernelFunction("get_anug_history")]
    public string GetHistory()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Plugin Function 'GetHistory' was called");
        Console.ForegroundColor = ConsoleColor.White;
        return """
               Aarhus .NET User Group (ANUG) is a community in Aarhus, Denmark, 
               for professionals and enthusiasts interested in .NET development. 
               ANUG organizes regular meetups, talks, and workshops focused on .NET technologies, 
               software development practices, and related topics. The group provides a platform for networking, 
               knowledge sharing, and staying updated on the latest trends in the .NET ecosystem."

               The group was established in 2007 by Søren Spelling Lund.

               As of 2024 it have over 1000 members

               Today it have the following Core Members that are in charge of organizing events
               - Kristoffer Strube
               - Mogens Heller Grabe 
               - Henrik Lykke Nielsen
               - Michael Skarum
               - Brian Holmgaard Kristensen
               - Christian Horsdal
               - Rasmus Wulff Jensen
               """;
    }
}