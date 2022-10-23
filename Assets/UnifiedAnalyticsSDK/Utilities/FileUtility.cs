using System.IO;

/// <summary>
/// A utility class responsible for I/O operations.
/// </summary>
public static class FileUtility
{
    /// <summary>
    /// Creates a directory for the specified path if doesn't exist.
    /// </summary>
    /// <param name="pathParameters"></param>
    public static void CreateDirectory(params string[] pathParameters)
    {
        var path  = FileUtility.GetPath(pathParameters);

        if (Directory.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Checks if a file exists in the specified path.
    /// </summary>
    /// <param name="pathParameters"></param>
    /// <returns></returns>
    public static bool FileExists(params string[] pathParameters)
    {
        var path  = FileUtility.GetPath(pathParameters);

        return File.Exists(path);
    }

    /// <summary>
    /// Creates/Combines the specified path parameters into a working system path.
    /// </summary>
    /// <param name="pathParameters"></param>
    /// <returns></returns>
    public static string GetPath(params string[] pathParameters)
    {
        return Path.Combine(pathParameters);
    }
}