using System.IO;
using Microsoft.Extensions.Configuration;

public class BoundaryIndexProviderFromConfig : IBoundaryIndexProvider
{
    private readonly IConfiguration configuration;
    private const string SectionName = "Database Connection";
    private const string TextFilePath = "last_boundary_index.txt";

    public BoundaryIndexProviderFromConfig()
    {
        configuration = ConfigFile();
    }

    public int Read()
    {
        int boundaryIndex;
        int.TryParse(File.ReadAllText(TextFilePath), out boundaryIndex);
        return boundaryIndex;
    }

    public void Write(int last)
    {
        File.WriteAllText(TextFilePath, last.ToString());

        var modifiedConfig = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("config.ini")
            .Build();

        modifiedConfig.GetSection(SectionName)["boundaryIndexStart"] = last.ToString();

        using var writer = new StreamWriter("config.ini");
        modifiedConfig.Save(writer);
    }

    private IConfiguration ConfigFile()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("config.ini")
            .Build();

        return configuration;
    }
}
