namespace CreadorDeRequerimientos.Mobile.Services;

public interface ISpeechCaptureService
{
    event EventHandler<SpeechTextReceivedEventArgs>? TextReceived;
    event EventHandler<SpeechListeningChangedEventArgs>? ListeningChanged;

    bool IsNativeCaptureAvailable { get; }
    bool IsContinuousCaptureSupported { get; }
    bool IsListening { get; }

    Task<string> CaptureAsync(string seedText, CancellationToken cancellationToken);
    Task StartContinuousCaptureAsync(CancellationToken cancellationToken);
    Task StopContinuousCaptureAsync();
}

public sealed class SpeechTextReceivedEventArgs(string text, bool isFinal) : EventArgs
{
    public string Text { get; } = text;
    public bool IsFinal { get; } = isFinal;
}

public sealed class SpeechListeningChangedEventArgs(bool isListening) : EventArgs
{
    public bool IsListening { get; } = isListening;
}
