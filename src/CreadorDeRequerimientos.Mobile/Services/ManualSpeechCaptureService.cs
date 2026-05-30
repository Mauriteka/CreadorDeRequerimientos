namespace CreadorDeRequerimientos.Mobile.Services;

public sealed class ManualSpeechCaptureService : ISpeechCaptureService
{
    public event EventHandler<SpeechTextReceivedEventArgs>? TextReceived
    {
        add { }
        remove { }
    }
    public event EventHandler<SpeechListeningChangedEventArgs>? ListeningChanged;

    public bool IsNativeCaptureAvailable => false;
    public bool IsContinuousCaptureSupported => false;
    public bool IsListening => false;

    public Task<string> CaptureAsync(string seedText, CancellationToken cancellationToken)
    {
        return Task.FromResult(seedText);
    }

    public Task StartContinuousCaptureAsync(CancellationToken cancellationToken)
    {
        ListeningChanged?.Invoke(this, new SpeechListeningChangedEventArgs(false));
        return Task.CompletedTask;
    }

    public Task StopContinuousCaptureAsync()
    {
        ListeningChanged?.Invoke(this, new SpeechListeningChangedEventArgs(false));
        return Task.CompletedTask;
    }
}
