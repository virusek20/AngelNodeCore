using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model;
using AngelNode.Model.Message;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using AngelNode.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using static AngelNode.Model.Project.ProjectReportMessage;
using MessageBox = System.Windows.MessageBox;

namespace AngelNode.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;
        private readonly IExportService _exportService;
        private readonly IExportService _dotService;
        private readonly IExportService _resourceService;

        private ObservableCollection<TabItem> _tabs = new ObservableCollection<TabItem>();
        private TabItem _selectedTab;
        private string _lastExportPath;
        private string _lastDotExportPath;
        private string _lastResourceExportPath;
        private bool _ignoreClose;

        public ObservableCollection<TabItem> Tabs
        {
            get => _tabs;
            set { Set(() => Tabs, ref _tabs, value); }
        }

        public TabItem SelectedTab
        {
            get => _selectedTab;
            set { Set(() => SelectedTab, ref _selectedTab, value); }
        }
        public Project CurrentProject => _projectService.CurrentProject;

        public RelayCommand AnalyzeCommand { get; }
        public RelayCommand OpenCommand { get; }
        public RelayCommand NewCommand { get; }

        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand<Window> AboutCommand { get; }
        public RelayCommand<Window> BuildCommand { get; }
        public RelayCommand<CancelEventArgs> ExitCommand { get; }
        public RelayCommand RunCommand { get;  }
        public RelayCommand GenerateDotCommand { get; }
        public RelayCommand ResourceAnalysisCommand { get; }

        public RelayCommand<object> CloseTabCommand { get; }
        public RelayCommand<object> CloseAllTabCommand { get; }

        public MainViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
            _exportService = SimpleIoc.Default.GetInstance<IExportService>("xml");
            _dotService = SimpleIoc.Default.GetInstance<IExportService>("dot");
            _resourceService = SimpleIoc.Default.GetInstance<IExportService>("rsc");

            MessengerInstance.Register(this, new Action<TabOpenMessage>(OpenTab));
            MessengerInstance.Register(this, new Action<TabCloseMessage>(CloseTab));
            MessengerInstance.Register(this, new Action<RunMessage>(RunDebug));

            Tabs.Add(new StartView());

            AnalyzeCommand = new RelayCommand(() => _projectService.Analyze());
            OpenCommand = new RelayCommand(OpenProject);
            NewCommand = new RelayCommand(NewProject);

            SaveCommand = new RelayCommand(SaveProject);
            BuildCommand = new RelayCommand<Window>(ExportProject);
            ExitCommand = new RelayCommand<CancelEventArgs>(Exit);
            AboutCommand = new RelayCommand<Window>(OpenAbout);
            CloseCommand = new RelayCommand<Window>(Close);
            GenerateDotCommand = new RelayCommand(GenerateDot);
            ResourceAnalysisCommand = new RelayCommand(ResourceAnalysis);

            CloseTabCommand = new RelayCommand<object>(CloseTabDirect, CanCloseTab);
            CloseAllTabCommand = new RelayCommand<object>(CloseAllTabDirect);

            RunCommand = new RelayCommand(Run);
        }
        private void ResourceAnalysis()
        {
            _projectService.Save(null);

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = _resourceService.FileFilter,
                InitialDirectory = Path.GetDirectoryName(_lastResourceExportPath),
                FileName = Path.GetFileName(_lastResourceExportPath),
                DefaultExt = _resourceService.DefaultExtension,
                CheckFileExists = false,
                AddExtension = true
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                _lastResourceExportPath = dialog.FileName;
                _resourceService.Export(_lastResourceExportPath);
            }
        }

        private void GenerateDot()
        {
            _projectService.Save(null);

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = _dotService.FileFilter,
                InitialDirectory = Path.GetDirectoryName(_lastDotExportPath),
                FileName = Path.GetFileName(_lastDotExportPath),
                DefaultExt = _dotService.DefaultExtension,
                CheckFileExists = false,
                AddExtension = true
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                _lastDotExportPath = dialog.FileName;
                _dotService.Export(_lastDotExportPath);
            }
        }

        private void ExitNoDialog(Window obj)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Exit(CancelEventArgs args)
        {
            if (_ignoreClose) return;
            var result = MessageBox.Show(System.Windows.Application.Current.MainWindow, "Do you want to save this project before exiting?", "Save", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    _projectService.Save(null);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    args.Cancel = true;
                    return;
            }

            _ignoreClose = true;
            System.Windows.Application.Current.Shutdown();
        }

        private void Close(Window window)
        {
            var result = MessageBox.Show(window, "Do you want to save this project before closing it?", "Save", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    _projectService.Save(null);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }

            _ignoreClose = true;
            new LaunchView().Show();
            window.Close();
        }

        private void ExportProject(Window window)
        {
            _projectService.Save(null);
            _projectService.Analyze();
            if (!_projectService.CurrentProject.ProjectReport.WasSuccessful)
            {
                MessageBox.Show(
                    "One or more errors occured during project analysis, please fix all errors before exporting this project.",
                    "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = _exportService.FileFilter,
                InitialDirectory = Path.GetDirectoryName(_lastExportPath),
                FileName = Path.GetFileName(_lastExportPath),
                DefaultExt = _exportService.DefaultExtension,
                CheckFileExists = false,
                AddExtension = true
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                _lastExportPath = dialog.FileName;
                _exportService.Export(_lastExportPath);
            }
        }

        private void Run()
        {
            if (!RunExport()) return;
            _exportService.Run(0);
        }

        private void RunDebug(RunMessage obj)
        {
            if (!RunExport()) return;
            int startNode = 0;

            switch (obj.RunMessageType)
            {
                case RunMessage.RunType.Node:
                    startNode = _exportService.GetNodeNumber((INode) obj.Data);
                    break;
                case RunMessage.RunType.Scene:
                    var scene = (Scene) obj.Data;
                    startNode = _exportService.GetNodeNumber(scene.Nodes.First());
                    break;
            }

            _exportService.Run(startNode);
        }

        private bool RunExport()
        {
            _projectService.Save(null);
            _projectService.Analyze();
            if (!_projectService.CurrentProject.ProjectReport.WasSuccessful)
            {
                MessageBox.Show(
                    "One or more errors occured during project analysis, please fix all errors before exporting this project.",
                    "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            if (string.IsNullOrWhiteSpace(_lastExportPath))
            {
                MessageBox.Show("Please build the project at least once before running the game.", "Run failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else _exportService.Export(_lastExportPath);

            return true;
        }

        private void SaveProject()
        {
            _projectService.Save(null);
            _projectService.Analyze();
        }

        private void OpenAbout(Window window)
        {
            AboutView aboutWindow = new AboutView
            {
                Owner = window
            };

            aboutWindow.ShowDialog();
        }

        private void OpenProject()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "XML Project files|*.xml"
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                string filename = dialog.FileName;
                _projectService.Load(filename);
                _projectService.Analyze();
            }
        }

        private void NewProject()
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
                        MessageBox.Show("Please select a valid, existing folder.", "Cannot create project", MessageBoxButton.OK);
                        return;
                    case IOException _:
                        MessageBox.Show("A project already exists in this location", "Cannot create project", MessageBoxButton.OK);
                        return;
                }

                throw;
            }
        }

        private void OpenTab(TabOpenMessage tab)
        {
            if (SwitchTab(tab)) return;

            switch (tab.TabOpenType)
            {
                case TabOpenMessage.TabType.Character:
                    CharacterView cView = new CharacterView();
                    ((CharacterViewModel)cView.DataContext).Character = tab.Data as Character;
                    Tabs.Add(cView);
                    SelectedTab = cView;
                    break;
                case TabOpenMessage.TabType.Scene:
                    SceneView sView = new SceneView();
                    ((SceneViewModel)sView.DataContext).Scene = tab.Data as Scene;
                    Tabs.Add(sView);
                    SelectedTab = sView;
                    break;
                case TabOpenMessage.TabType.Variable:
                    VariableView vView = new VariableView();
                    ((VariableViewModel)vView.DataContext).Variable = tab.Data as Variable;
                    Tabs.Add(vView);
                    SelectedTab = vView;
                    break;
                case TabOpenMessage.TabType.Node:
                    SceneView snView = new SceneView();
                    var data = tab.Data as NodeLocation;
                    ((SceneViewModel)snView.DataContext).Scene = data.Scene;
                    ((SceneViewModel)snView.DataContext).SelectedNode = null;
                    ((SceneViewModel)snView.DataContext).SelectedNode = data.Node;
                    Tabs.Add(snView);
                    SelectedTab = snView;
                    break;
            }
        }

        private bool SwitchTab(TabOpenMessage tabMessage)
        {
            switch (tabMessage.TabOpenType)
            {
                case TabOpenMessage.TabType.Character:
                    foreach (var tab in Tabs)
                    {
                        if (tab is CharacterView cView)
                        {
                            if (((CharacterViewModel) tab.DataContext).Character == (Character) tabMessage.Data)
                            {
                                SelectedTab = cView;
                                return true;
                            }
                        }
                    }
                    break;
                case TabOpenMessage.TabType.Scene:
                    foreach (var tab in Tabs)
                    {
                        if (tab is SceneView sView)
                        {
                            if (((SceneViewModel)tab.DataContext).Scene == (Scene)tabMessage.Data)
                            {
                                SelectedTab = sView;
                                return true;
                            }
                        }
                    }
                    break;
                case TabOpenMessage.TabType.Variable:
                    foreach (var tab in Tabs)
                    {
                        if (tab is VariableView vView)
                        {
                            if (((VariableViewModel)tab.DataContext).Variable == (Variable)tabMessage.Data)
                            {
                                SelectedTab = vView;
                                return true;
                            }
                        }
                    }
                    break;
                case TabOpenMessage.TabType.Node:
                    var data = tabMessage.Data as NodeLocation;
                    foreach (var tab in Tabs)
                    {
                        if (tab is SceneView snView)
                        {
                            if (((SceneViewModel)tab.DataContext).Scene == data.Scene)
                            {
                                SelectedTab = snView;
                                ((SceneViewModel)snView.DataContext).SelectedNode = null;
                                ((SceneViewModel)snView.DataContext).SelectedNode = data.Node;
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Closes a tab by value remotely
        /// </summary>
        /// <param name="tab"></param>
        private void CloseTab(TabCloseMessage tab)
        {
            List<TabItem> closedTabs;

            switch (tab.TabCloseType)
            {
                case TabCloseMessage.TabType.Character:
                    Character character = tab.Data as Character;
                    closedTabs = Tabs.Where(t =>
                    {
                        if (!(t.DataContext is CharacterViewModel cvm)) return false;
                        return cvm.Character == character;
                    }).ToList();

                    foreach (var closedTab in closedTabs) Tabs.Remove(closedTab);
                    break;
                case TabCloseMessage.TabType.Scene:
                    Scene scene = tab.Data as Scene;
                    closedTabs = Tabs.Where(t =>
                    {
                        if (!(t.DataContext is SceneViewModel vvm)) return false;
                        return vvm.Scene == scene;
                    }).ToList();

                    foreach (var closedTab in closedTabs) Tabs.Remove(closedTab);
                    break;
                case TabCloseMessage.TabType.Variable:
                    Variable variable = tab.Data as Variable;
                    closedTabs = Tabs.Where(t =>
                    {
                        if (!(t.DataContext is VariableViewModel vvm)) return false;
                        return vvm.Variable == variable;
                    }).ToList();

                    foreach (var closedTab in closedTabs) Tabs.Remove(closedTab);
                    break;
                case TabCloseMessage.TabType.All:
                    Tabs.Clear();
                    Tabs.Add(new StartView());
                    break;
            }
        }

        /// <summary>
        /// Closes a tab by viewmodel reference
        /// </summary>
        /// <param name="obj">ViewModel of tab to be closed</param>
        private void CloseTabDirect(object obj)
        {
            switch (obj)
            {
                case CharacterViewModel cvm:
                    CloseTab(new TabCloseMessage
                    {
                        Data = cvm.Character,
                        TabCloseType = TabCloseMessage.TabType.Character
                    });
                    break;
                case SceneViewModel svm:
                    CloseTab(new TabCloseMessage
                    {
                        Data = svm.Scene,
                        TabCloseType = TabCloseMessage.TabType.Scene
                    });
                    break;
                case VariableViewModel vvm:
                    CloseTab(new TabCloseMessage
                    {
                        Data = vvm.Variable,
                        TabCloseType = TabCloseMessage.TabType.Variable
                    });
                    break;
            }
        }

        private bool CanCloseTab(object obj)
        {
            return !(obj is MainViewModel);
        }

        /// <summary>
        /// Closes all tabs except for the one passed by viewmodel reference
        /// </summary>
        /// <param name="obj">ViewModel of tab to be left open</param>
        private void CloseAllTabDirect(object obj)
        {
            Tabs.Clear();
            Tabs.Add(new StartView());

            switch (obj)
            {
                case CharacterViewModel cvm:
                    OpenTab(new TabOpenMessage
                    {
                        Data = cvm.Character,
                        TabOpenType = TabOpenMessage.TabType.Character
                    });
                    break;
                case SceneViewModel svm:
                    OpenTab(new TabOpenMessage
                    {
                        Data = svm.Scene,
                        TabOpenType = TabOpenMessage.TabType.Scene
                    });
                    break;
                case VariableViewModel vvm:
                    OpenTab(new TabOpenMessage
                    {
                        Data = vvm.Variable,
                        TabOpenType = TabOpenMessage.TabType.Variable
                    });
                    break;
            }
        }
    }
}