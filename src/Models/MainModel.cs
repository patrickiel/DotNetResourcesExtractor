
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DotNetResourcesExtractor;

public class MainModel
{
    public MainModel()
    {
    }

    public string DefaultOutputDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "DotNetResourcesExtractor");

    public void Extract(string outputDirectory, IReadOnlyList<string> filePaths)
    {
        var extractor = new ExtractorService(outputDirectory);

        var errors = new List<string>();

        foreach (var filePath in filePaths)
        {
            try
            {
                extractor.Extract(filePath);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }

        if (errors.Count != 0)
        {
            MessageBox.Show("The following errors occurred:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, errors), "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show("Done!", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        new Process
        {
            StartInfo = new ProcessStartInfo(outputDirectory)
            {
                UseShellExecute = true
            }
        }.Start();
    }

    internal bool IsValidFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            Assembly.LoadFrom(filePath);
            return true;
        }
        catch (BadImageFormatException)
        {
            // The file is not a valid .NET assembly
            return false;
        }
        catch (Exception)
        {
            // Other exceptions indicate problems that are not related to the file format
            return false;
        }
    }
}