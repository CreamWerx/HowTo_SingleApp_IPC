using System.Diagnostics;
using System.IO.Pipes;
using System.Windows;

namespace HowTo_SingleApp_IPC;

public partial class MainWindow : Window
{
    InstanceHandler? instanceHandler;
    public bool ClearLog { get; set; } = true;
    public MainWindow()
    {
        // This check is done before InitializeComponent to prevent the UI from loading if another instance is running.
        if (CheckForExistingProcess())
        {
            string[] testArgs = Generate2RandomStrings();
            //Log.WriteToTextFile("Another instance of the application is already running.");
            var args = Environment.GetCommandLineArgs();
            // TODO: Send the command line arguments to the existing instance.
            //Log.WriteToTextFile($"Command line arguments: {string.Join(", ", testArgs)}");
            bool res = false;
            try
            {
                res = InstanceHandler.SendArgsToExistingInstance(testArgs);
                //Log.WriteToTextFile($"SendArgsToExistingInstance returned: {res}");
            }
            catch (Exception ex)
            {
                Log.WriteToTextFile($"Error while sending arguments to the existing instance: {ex.Message}");
            }
            
            ShutdownApp();
        }
        InitializeComponent();
        instanceHandler = new InstanceHandler();
        instanceHandler.ArgsReceived += instanceHandler_ArgsReceived;
        instanceHandler.PipeServerErrorOccurred += InstanceHandler_PipeServerErrorOccurred;

    }

    private void ShutdownApp(bool v = true)
    {
        ClearLog = v;
        Application.Current.Shutdown();
    }

    private bool CheckForExistingProcess()
    {
        var nameOfThisApp = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        if (Process.GetProcessesByName(nameOfThisApp).Length > 1)
        {
            Log.WriteToTextFile("Additional Application started.");
            return true;
        }
        else
        {
            Log.WriteToTextFile("Single Application started.");
            return false;
        }
    }

    private string[] Generate2RandomStrings()
    {
        Random random = new Random();
        string[] args = new string[2];
        args[0] = "RandomString1_" + random.Next(1000, 9999);
        args[1] = "RandomString2_" + random.Next(1000, 9999);
        return args;
    }

    private void InstanceHandler_PipeServerErrorOccurred(object? sender, string e)
    {
        MessageBox.Show($"Pipe server error: {Environment.NewLine}{e}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void instanceHandler_ArgsReceived(object? sender, List<string> e)
    {
        Dispatcher.Invoke(() => Activate());
        foreach (var item in e)
        {
            Dispatcher.Invoke(() => tb.AppendText($"{item}{Environment.NewLine}"));
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //tb.AppendText("This is the first instance of the application." + Environment.NewLine);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (! ClearLog) return;
        Log.ClearLog();
    }
}