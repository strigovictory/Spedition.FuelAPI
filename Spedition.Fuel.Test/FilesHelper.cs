using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace Spedition.Fuel.Test;
public class FilesHelper
{
    private string AssemblyPath => Assembly.GetCallingAssembly().Location;

    protected string ContentPath => Path.Combine(AssemblyPath.Substring(0, AssemblyPath.IndexOf("bin") - 1), "wwwroot");

    protected byte[] ReedFile(string fileName, string directoryName)
    {
        var content = new byte[0];
        var reportsPath = Path.Combine(ContentPath, "src", directoryName, fileName);

        if (File.Exists(reportsPath))
        {
            using FileStream fs = new(reportsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using BinaryReader binaryReader = new(fs);
            content = binaryReader.ReadBytes((Int32)(new FileInfo(reportsPath).Length));
        }

        return content;
    }
}
