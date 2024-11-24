using Microsoft.SemanticKernel;

namespace HelloPlugin;

public class MyFirstPlugin
{
    private readonly string _rootFolder;

    public MyFirstPlugin()
    {
        _rootFolder = "C:\\HelloPlugin";
        if (!Directory.Exists(_rootFolder))
        {
            Directory.CreateDirectory(_rootFolder);
        }
    }

    [KernelFunction("get_number_of_files")]
    public int GetNumberOfFiles()
    {
        var rootFolder = GetRootFolder();
        return Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories).Length;
    }

    [KernelFunction("get_root_folder")]
    public string GetRootFolder()
    {
        return _rootFolder;
    }

    [KernelFunction("create_folder")]
    public void CreateFolder(string folderPath)
    {
        Guard(folderPath);
        Directory.CreateDirectory(folderPath);
    }

    [KernelFunction("create_file")]
    public void CreateFile(string filePath, string content)
    {
        Guard(filePath);
        File.WriteAllText(filePath, content);
    }

    [KernelFunction("get_content_of_file")]
    public string GetContentOfFile(string filePath)
    {
        Guard(filePath);
        return File.ReadAllText(filePath);
    }

    [KernelFunction("move_file")]
    public void MoveFile(string source_file_path, string target_file_path)
    {
        Guard(source_file_path);
        Guard(target_file_path);
        File.Move(source_file_path, target_file_path);
    }

    [KernelFunction("move_folder")]
    public void MoveFolder(string source_folder_path, string target_folder_path)
    {
        Guard(source_folder_path);
        Guard(target_folder_path);
        Directory.Move(source_folder_path, target_folder_path);
    }

    [KernelFunction("get_files_for_folder")]
    public string[] GetFiles(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetFiles(folderPath);
    }

    [KernelFunction("get_folders_for_folder")]
    public string[] GetFolders(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetDirectories(folderPath);
    }

    private void Guard(string folderPath)
    {
        if (!folderPath.StartsWith(_rootFolder))
        {
            throw new Exception("No you don't!");
        }
    }
}