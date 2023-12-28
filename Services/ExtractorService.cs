using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;

namespace DotNetResourcesExtractor;

public class ExtractorService(string outputDirectory)
{
    public void Extract(string filePath)
    {
        Assembly assembly = Assembly.LoadFile(filePath);
        string[] resourceNames = assembly.GetManifestResourceNames();

        foreach (string resourceName in resourceNames)
        {
            if (resourceName.EndsWith(".resources"))
            {
                Console.WriteLine($"Contents of {resourceName}:");
                using Stream? stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource stream is null '{resourceName}'");
                using ResourceReader reader = new(stream);
                foreach (DictionaryEntry entry in reader)
                {
                    string fileName = entry.Key.ToString() ?? throw new InvalidDataException($"Invalid resource name '{resourceName}'");
                    var outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(filePath), resourceName, fileName);

                    string partentDirectory = Path.GetDirectoryName(outputFilePath) ?? throw new InvalidDataException($"Invalid output file path '{outputFilePath}'");
                    Directory.CreateDirectory(partentDirectory);

                    var extention = Path.GetExtension(outputFilePath);

                    using var fileStream = new FileStream(outputFilePath, FileMode.Create);
                    using var writer = new BinaryWriter(fileStream);
                    // Check if entry.Value is a stream
                    if (entry.Value is Stream valueStream)
                    {
                        using var memoryStream = new MemoryStream();
                        valueStream.CopyTo(memoryStream); // Copy the stream to a MemoryStream
                        byte[] data = memoryStream.ToArray(); // Convert MemoryStream to byte array
                        writer.Write(data); // Write the byte array to the file
                    }
                    else
                    {
                        throw new InvalidDataException($"Resource is not a stream '{entry.Key}'");
                    }
                }
            }
        }
    }
}
