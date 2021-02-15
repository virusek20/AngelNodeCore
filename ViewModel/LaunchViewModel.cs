using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AngelNode.Service.Interface;
using AngelNode.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace AngelNode.ViewModel
{
    public class LaunchViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;

        public RelayCommand<Window> OpenCommand { get; }
        public RelayCommand<Window> NewCommand { get; }

        public LaunchViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();

            OpenCommand = new RelayCommand<Window>(OpenProject);
            NewCommand = new RelayCommand<Window>(NewProject);
        }

        private void OpenProject(Window window)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "XML Project files|*.xml" };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                string filename = dialog.FileName;

                try
                {
                    _projectService.Load(filename);
                    _projectService.Analyze();
                }
                catch (FileNotFoundException fe)
                {
                    MessageBox.Show($"Failed to load project.\n{fe.Message}\n{fe.FileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (Debugger.IsAttached) throw;

                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load project.\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (Debugger.IsAttached) throw;

                    return;
                }

                new MainView().Show();
                window.Close();
            }
        }

        private void NewProject(Window window)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder for new project",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            try
            {
                _projectService.CreateNew(dialog.SelectedPath);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case DirectoryNotFoundException _:
                        MessageBox.Show("Please select a valid, existing folder.", "Cannot create project", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    case IOException _:
                        MessageBox.Show("A project already exists in this location", "Cannot create project", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                throw;
            }

            new MainView().Show();
            window.Close();
        }
    }
}
