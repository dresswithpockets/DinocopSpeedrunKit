using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;

namespace Autosplit;

public class LiveSplitClient : IDisposable
{
    public TcpClient Client { get; set; }

    public bool Connected => Client.Connected;

    private readonly string _hostname;
    private readonly int _port;
    private StreamWriter? _writer;
    private bool _hasResetSinceLastStart;

    public LiveSplitClient(string hostname, int port)
    {
        _port = port;
        _hostname = hostname;
        Client = new TcpClient();
    }

    public void Connect()
    {
        Client.Connect(_hostname, _port);
        _writer = new StreamWriter(Client.GetStream());
        Console.WriteLine($"LiveSplitClient Connected");
    }

    public IEnumerator WaitForConnectedRoutine(Action? callback = null)
    {
        var task = Client.ConnectAsync(_hostname, _port);
        while (!task.IsCompleted)
            yield return null;

        callback?.Invoke();
    }

    private void QueueCommand(string command)
    {
        if (!Connected)
            return;

        Plugin.Logger?.LogInfo($"Queueing LiveSplit Command: '{command}'");
        _writer!.WriteLine(command);
        _writer.Flush();
    }

    public void StartOrSplit()
    {
        QueueCommand("startorsplit");
        _hasResetSinceLastStart = false;
    }

    public void StartTimer()
    {
        QueueCommand("starttimer");
        _hasResetSinceLastStart = false;
    }

    public void InitGameTime() => QueueCommand("initgametime");
    public void PauseGameTime() => QueueCommand("pausegametime");
    public void UnPauseGameTime() => QueueCommand("unpausegametime");
    public void Reset(bool force = false)
    {
        if (!force && _hasResetSinceLastStart)
            return;
        
        QueueCommand("reset");
        _hasResetSinceLastStart = true;
    }

    public void StopTimer() => QueueCommand("stoptimer");

    public void Dispose()
    {
        if (Connected)
        {
            _writer?.Dispose();
        }

        Client.Dispose();
    }
}