using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Memory;

namespace RetrievalAugmentedGeneration.RelewiseDocs;

#pragma warning disable SKEXP0001
public class RelewiseDocsImporter(string collection, ISemanticTextMemory memory)
{
    private int _progressCounter;

    public async Task Import()
    {
        _progressCounter = 0;
        const string root = "C:\\SourceData";
        var mdFiles = Directory.GetFiles(root, "*.md", SearchOption.AllDirectories);
        foreach (var mdFile in mdFiles)
        {
            var mdContent = await File.ReadAllTextAsync(mdFile);
            //Remove Any Commented out parts
            mdContent = Regex.Replace(mdContent, "<!--[\\s\\S]*?-->", string.Empty);

            var mdContentLine = mdContent.Split([System.Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

            string description = string.Empty;
            string id = mdFile.Replace(root, "https://docs.relewise.com/docs").Replace("\\", "/").Replace(".md", ".html");
            string sectionId = string.Empty;
            string sectionContent = string.Empty;
            foreach (var line in mdContentLine)
            {
                if (line is "---" or "")
                {
                    continue;
                }

                if (line.StartsWith("description:"))
                {
                    description = line.Replace("description:", string.Empty).Replace("'", string.Empty).Trim();
                    continue;
                }

                if (line.StartsWith("# ")) //H1
                {
                    if (await WriteMemoryIfNeeded(sectionId, sectionContent, description))
                    {
                        sectionId = string.Empty;
                        sectionContent = string.Empty;
                    }

                    string heading = line.Replace("# ", string.Empty);
                    sectionContent += heading + Environment.NewLine;


                    sectionId = $"{id}#{PrepareHeader(heading)}";
                }
                else if (line.StartsWith("## ")) //H2
                {
                    if (await WriteMemoryIfNeeded(sectionId, sectionContent, description))
                    {
                        sectionId = string.Empty;
                        sectionContent = string.Empty;
                    }

                    string heading = line.Replace("## ", string.Empty);
                    sectionId = $"{id}#{PrepareHeader(heading)}";
                }
                else
                {
                    //Regular Content
                    string lineContent = line;
                    MatchCollection imageMatches = Regex.Matches(lineContent, @"\.\.\/(?'file'assets\/images\/(?'name'[a-z-_0-9]*\.png))");
                    foreach (Match imageMatch in imageMatches)
                    {
                        string path = $"{root}\\{imageMatch.Groups["file"].Value.Replace("/", "\\")}";
                        if (File.Exists(path))
                        {
                            //Todo - find how they make the URL Transformation
                        }
                    }

                    sectionContent += lineContent + Environment.NewLine;
                }
            }

            string PrepareHeader(string heading)
            {
                List<string> charsToReplace = [" ", "/", "\\", "{", "}", "@", "&"];
                foreach (var toReplace in charsToReplace)
                {
                    heading = heading.Replace(toReplace, "-");
                }

                return heading.Replace("?", string.Empty);
            }

            if (await WriteMemoryIfNeeded(sectionId, sectionContent, description))
            {
                sectionId = string.Empty;
                sectionContent = string.Empty;
            }
        }
    }


    private async Task<bool> WriteMemoryIfNeeded(string contentId, string content, string contentDescription, int retryCount = 0)
    {
        if (!string.IsNullOrWhiteSpace(contentId))
        {
            _progressCounter = _progressCounter - retryCount + 1;
            System.Console.WriteLine($"{_progressCounter}: Saving: " + contentId);
            try
            {
                await memory.SaveInformationAsync(collection, id: contentId, text: content, description: contentDescription, additionalMetadata: contentId);
            }
            catch (Exception e)
            {
                await Task.Delay(30000);
                if (retryCount < 3)
                {
                    await WriteMemoryIfNeeded(contentId, content, contentDescription, retryCount);
                }
            }

            return true;
        }

        return false;
    }
}


#pragma warning restore SKEXP0001