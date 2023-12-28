using CommunityToolkit.Mvvm.ComponentModel;

namespace DotNetResourcesExtractor;

public partial class FileItemViewModel : ObservableObject
{
    [ObservableProperty]
    bool isSelected;

    public FileItemViewModel(string fileName, string filePath)
    {
        FileName = fileName;
        FilePath = filePath;
    }

    public string FileName { get; }
    public string FilePath { get; }
}
