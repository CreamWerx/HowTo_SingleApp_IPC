using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HowTo_SingleApp_IPC;
internal class InstanceHandler
{
    public event EventHandler<List<string>>? ArgsReceived;
    public event EventHandler<string>? PipeServerErrorOccurred;
    NamedPipeServerStream? pipeServer;

    public InstanceHandler()
    {
        BeginPipeServer();
    }

    private void BeginPipeServer()
    {
        pipeServer = new NamedPipeServerStream("SingleAppPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        pipeServer.BeginWaitForConnection(OnPipeConnection, pipeServer);
        //Dispatcher.Invoke(() => tb.AppendText("Waiting for another instance of the application to connect..." + Environment.NewLine));
    }

    private void OnPipeConnection(IAsyncResult ar)
    {
        NamedPipeServerStream? pipeServer = (NamedPipeServerStream?)ar.AsyncState;
        try
        {
            var newArgs = new List<string>();
            using (var reader = new System.IO.StreamReader(pipeServer))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    newArgs.Add(line);
                }
                ArgsReceived?.Invoke(this, newArgs);
            }
        }
        catch (Exception ex)
        {
            PipeServerErrorOccurred?.Invoke(this, ex.Message);
            Log.WriteToTextFile($"Error while reading from pipe: {ex.Message}");
        }
        finally
        {// TODO: implement error events
            pipeServer?.Close();
            try
            {
                BeginPipeServer();
            }
            catch (Exception ex)
            {
                PipeServerErrorOccurred?.Invoke(this, ex.Message);
                Log.WriteToTextFile($"BeginPipeServer failed: {ex.Message}");
            }
        }
    }

    public static bool SendArgsToExistingInstance(string[] args)
    {
        bool returnStatus = false;
        Log.WriteToTextFile("Sending arguments to the existing instance of the application.");
        NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "SingleAppPipe", PipeDirection.Out);
        try
        {
            pipeClient.Connect(1000); // Wait for 1 second to connect
            using (var writer = new System.IO.StreamWriter(pipeClient))
            {
                foreach (var arg in args)
                {
                    writer.WriteLine(arg);
                }
                writer.Flush();
            }
            returnStatus = true;
        }
        catch (TimeoutException)
        {
            returnStatus = false;
            Log.WriteToTextFile("Failed to connect to the existing instance of the application within the timeout period.");
        }
        catch (Exception ex)
        {
            returnStatus = false;
            Log.WriteToTextFile($"Error while sending arguments: {ex.Message}");
        }
        finally
        {
            pipeClient.Close();
        }
        return returnStatus;
    }
}
