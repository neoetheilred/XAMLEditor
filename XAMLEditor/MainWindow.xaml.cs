using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XAMLEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            NewCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            OpenCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CloseCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Alt));
        }

        private static FileService fileService = new FileService();
        public static RoutedCommand SaveCommand = new RoutedCommand();
        public static RoutedCommand NewCommand = new RoutedCommand();
        public static RoutedCommand OpenCommand = new RoutedCommand();
        public static RoutedCommand CloseCommand = new RoutedCommand();

        private void NewFile(object sender, RoutedEventArgs e)
        {
            Tabs.Items.Add(new TabItem
            {
                Header = new TextBlock { Text = "Untitled" },
                Content = new CodeEditor()
            });
            Tabs.SelectedIndex = Tabs.Items.Count - 1;
        }

        private void CreateNewInstance(object sender, RoutedEventArgs e)
        {
            var newInst = AppDomain.CreateDomain(Startup.GetNewDomainName());
            newInst.DoCallBack(Startup.Action);
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (fileService.TryGetOpenFileName(out string fileName))
            {
                try
                {
                    foreach (var tabsItem in Tabs.Items)
                    {
                        if (((tabsItem as TabItem)?.Content as CodeEditor)?.FilePath == fileName)
                        {
                            Tabs.SelectedIndex = Tabs.Items.IndexOf(tabsItem);
                            return;
                        }
                    }
                    Tabs.Items.Add(new TabItem
                    {
                        Header = new TextBlock { Text = $"{fileName.Split(Path.DirectorySeparatorChar).Last()}" },
                        Content = new CodeEditor(fileName)
                    });
                    Tabs.SelectedIndex = Tabs.Items.Count - 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open file: {ex.Message}", "Error");
                }
            }
            // else MessageBox.Show("Could not open file", "Error");
        }


        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (Tabs.Items.Count < 1)
                return;

            try
            {
                var currentEditor = Monad<TabControl>.Of(Tabs)
                    .Bind(x => x.Items[Tabs.SelectedIndex] as TabItem)
                    .Bind(x => x.Content as CodeEditor)
                    .Value;
                if (currentEditor.FilePath == null &&
                    fileService.TryGetSaveFileName(out string fileName))
                {
                    currentEditor.FilePath = fileName;
                }

                currentEditor.Save();
                // MessageBox.Show(currentEditor.FilePath, "Info");
                Monad<TabControl>.Of(Tabs)
                    .Bind(x => x.Items[Tabs.SelectedIndex] as TabItem)
                    .Pipe(x => x.Header = new TextBlock { Text = $"{currentEditor.FilePath.Split(Path.DirectorySeparatorChar).Last()}" });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file: {ex.Message}", "Error");
            }
        }

        private void CloseFile(object sender, RoutedEventArgs e)
        {
            if (Tabs.Items.Count < 1)
                return;

            var currentEditor = new Monad<TabControl>(Tabs)
                .Bind(x => x.Items[Tabs.SelectedIndex] as TabItem)
                .Bind(x => x.Content as CodeEditor)
                .Value;

            if (!currentEditor.Saved &&
                MessageBox.Show(
                    "File not saved, save it?",
                    "Save", MessageBoxButton.YesNo, 
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SaveFile(sender, e);
            }

            int selectedIndex = Tabs.SelectedIndex;
            Tabs.Items.RemoveAt(selectedIndex);
            if (Tabs.Items.Count >= 1)
            {
                Tabs.SelectedIndex = Math.Max(Tabs.Items.Count - 1, selectedIndex);
            }
        }
    }
}
