using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

namespace DotNetResourcesExtractor;


public partial class MainViewModel : ObservableObject
{
    private readonly MainModel model;

    [ObservableProperty]
    private string outputDirectory = "";

    [ObservableProperty]
    private ObservableCollection<FileItemViewModel> files = [];

    [ObservableProperty]
    private ObservableCollection<FileItemViewModel> selectedFiles = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFilesCommand))]
    public bool anySelectedFiles;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeselectAllCommand))]
    public bool anyFiles;

    public MainViewModel()
    {
        model = new MainModel();

        outputDirectory = model.DefaultOutputDirectoryPath;

        SelectedFiles.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (FileItemViewModel item in e.NewItems)
                {
                    item.IsSelected = true;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (FileItemViewModel item in e.OldItems)
                {
                    item.IsSelected = false;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (FileItemViewModel item in Files)
                {
                    item.IsSelected = false;
                }
            }

            AnySelectedFiles = Files.Any(f => f.IsSelected);
        };

        Files.CollectionChanged += (s, e) => AnyFiles = Files.Count != 0;
    }

    [RelayCommand]
    private void AddFile()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            CheckFileExists = true,
            AddToRecent = true,
            Filter = "DLL and EXE Files (*.dll;*.exe)|*.dll;*.exe|All Files (*.*)|*.*",
            Title = "Select DLL or EXE Files"
        };

        if (dialog.ShowDialog() == true)
        {
            var filesToAdd = new List<string>();

            foreach (string? file in dialog.FileNames)
            {
                if (file is null)
                {
                    continue;
                }

                if (Files.Any(f => f.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                filesToAdd.Add(file);
            }

            var invalidFiles = filesToAdd.Where(f => !model.IsValidFile(f)).ToList();

            if (invalidFiles.Count != 0)
            {
                MessageBox.Show($"The following files are not supported:{Environment.NewLine}{string.Join(Environment.NewLine, invalidFiles)}", "Invalid Files", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var validFiles = filesToAdd.Where(f => !invalidFiles.Contains(f)).ToList();

            foreach (var file in validFiles)
            {
                Files.Add(new FileItemViewModel(Path.GetFileName(file), file));
            }
        }
    }

    [RelayCommand(CanExecute = nameof(AnyFiles))]
    private void SelectAll()
    {
        foreach (var file in Files)
        {
            file.IsSelected = true;

            if (!SelectedFiles.Contains(file))
            {
                SelectedFiles.Add(file);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(AnyFiles))]
    private void DeselectAll()
    {
        SelectedFiles.Clear();

        foreach (var file in Files)
        {
            file.IsSelected = false;
        }
    }

    [RelayCommand(CanExecute = nameof(AnySelectedFiles))]
    private void RemoveSelectedFiles()
    {
        var files = Files.Where(f => f.IsSelected).ToList();
        foreach (var file in files)
        {
            Files.Remove(file);
        }
    }

    [RelayCommand]
    private void SelectOutputDirectory()
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Select Folder"
        };

        if (folderDialog.ShowDialog() == true)
        {
            OutputDirectory = folderDialog.FolderName;
        }
    }

    [RelayCommand]
    private void RemoveFile(FileItemViewModel file)
    {
        Files.Remove(file);
    }

    [RelayCommand]
    private void Extract()
    {
        model.Extract(OutputDirectory, Files.Select(i => i.FilePath).ToList());
    }
}
