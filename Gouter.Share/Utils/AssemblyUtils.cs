namespace Gouter.Utils;

public static class AssemblyUtils
{
    public static string GetLocalPath(string? relativePath = null)
    {
        var workDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        if (relativePath == null)
            return workDir;

        return Path.Combine(workDir, relativePath);
    }
}
