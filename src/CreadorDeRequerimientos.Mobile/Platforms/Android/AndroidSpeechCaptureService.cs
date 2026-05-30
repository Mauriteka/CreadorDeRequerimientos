using Android.Content;
using Android.OS;
using Android.Speech;
using CreadorDeRequerimientos.Mobile.Services;
using Application = Android.App.Application;

namespace CreadorDeRequerimientos.Mobile.Platforms.Android;

public sealed class AndroidSpeechCaptureService : ISpeechCaptureService
{
    private static readonly TimeSpan CaptureTimeout = TimeSpan.FromSeconds(25);
    private static readonly TimeSpan RestartDelay = TimeSpan.FromMilliseconds(450);
    private readonly SemaphoreSlim captureGate = new(1, 1);
    private readonly object syncRoot = new();

    private SpeechRecognizer? continuousRecognizer;
    private ContinuousRecognitionListener? continuousListener;
    private CancellationTokenSource? restartCancellation;
    private bool shouldListenContinuously;
    private bool isStarting;

    public event EventHandler<SpeechTextReceivedEventArgs>? TextReceived;
    public event EventHandler<SpeechListeningChangedEventArgs>? ListeningChanged;

    public bool IsNativeCaptureAvailable
        => SpeechRecognizer.IsRecognitionAvailable(Application.Context);

    public bool IsContinuousCaptureSupported => IsNativeCaptureAvailable;

    public bool IsListening { get; private set; }

    public async Task<string> CaptureAsync(string seedText, CancellationToken cancellationToken)
    {
        if (!IsNativeCaptureAvailable)
        {
            return seedText;
        }

        var permission = await Permissions.RequestAsync<Permissions.Microphone>();
        if (permission != PermissionStatus.Granted)
        {
            return seedText;
        }

        await captureGate.WaitAsync(cancellationToken);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(CaptureTimeout);

            var recognizedText = await MainThread.InvokeOnMainThreadAsync(() =>
                ListenOnceAsync(timeout.Token));

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                return seedText;
            }

            return string.IsNullOrWhiteSpace(seedText)
                ? recognizedText.Trim()
                : $"{seedText.TrimEnd()} {recognizedText.Trim()}";
        }
        finally
        {
            captureGate.Release();
        }
    }

    public async Task StartContinuousCaptureAsync(CancellationToken cancellationToken)
    {
        if (!IsNativeCaptureAvailable)
        {
            return;
        }

        var permission = await Permissions.RequestAsync<Permissions.Microphone>();
        if (permission != PermissionStatus.Granted)
        {
            return;
        }

        lock (syncRoot)
        {
            shouldListenContinuously = true;
        }

        await MainThread.InvokeOnMainThreadAsync(StartContinuousRecognizer);
    }

    public Task StopContinuousCaptureAsync()
    {
        lock (syncRoot)
        {
            shouldListenContinuously = false;
            isStarting = false;
            restartCancellation?.Cancel();
            restartCancellation?.Dispose();
            restartCancellation = null;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            continuousRecognizer?.Cancel();
            DestroyContinuousRecognizer();
            SetListening(false);
        });

        return Task.CompletedTask;
    }

    private void StartContinuousRecognizer()
    {
        lock (syncRoot)
        {
            if (!shouldListenContinuously || isStarting)
            {
                return;
            }

            isStarting = true;
        }

        try
        {
            continuousRecognizer ??= SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
            if (continuousRecognizer is null)
            {
                SetListening(false);
                return;
            }

            continuousListener ??= new ContinuousRecognitionListener(this);
            continuousRecognizer.SetRecognitionListener(continuousListener);
            continuousRecognizer.StartListening(CreateRecognizerIntent());
            SetListening(true);
        }
        finally
        {
            lock (syncRoot)
            {
                isStarting = false;
            }
        }
    }

    private void ScheduleContinuousRestart()
    {
        CancellationToken token;
        lock (syncRoot)
        {
            if (!shouldListenContinuously)
            {
                SetListening(false);
                return;
            }

            restartCancellation?.Cancel();
            restartCancellation?.Dispose();
            restartCancellation = new CancellationTokenSource();
            token = restartCancellation.Token;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(RestartDelay, token);
                if (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(StartContinuousRecognizer);
                }
            }
            catch (System.OperationCanceledException)
            {
            }
        }, token);
    }

    private void PublishRecognizedText(string? text, bool isFinal)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        TextReceived?.Invoke(this, new SpeechTextReceivedEventArgs(text.Trim(), isFinal));
    }

    private void SetListening(bool value)
    {
        if (IsListening == value)
        {
            return;
        }

        IsListening = value;
        ListeningChanged?.Invoke(this, new SpeechListeningChangedEventArgs(value));
    }

    private void DestroyContinuousRecognizer()
    {
        continuousRecognizer?.SetRecognitionListener(null);
        continuousRecognizer?.Destroy();
        continuousRecognizer?.Dispose();
        continuousRecognizer = null;

        continuousListener?.Dispose();
        continuousListener = null;
    }

    private static Intent CreateRecognizerIntent()
    {
        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraLanguage, "es-MX");
        intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, "es-MX");
        intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
        intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
        intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Application.Context.PackageName);
        return intent;
    }

    private static Task<string> ListenOnceAsync(CancellationToken cancellationToken)
    {
        var taskCompletion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var recognizer = SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
        if (recognizer is null)
        {
            return Task.FromResult(string.Empty);
        }

        var listener = new SingleUtteranceRecognitionListener(taskCompletion);
        recognizer.SetRecognitionListener(listener);

        CancellationTokenRegistration registration = default;
        registration = cancellationToken.Register(() =>
        {
            recognizer.Cancel();
            taskCompletion.TrySetResult(listener.BestText);
        });

        taskCompletion.Task.ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                registration.Dispose();
                recognizer.Destroy();
                recognizer.Dispose();
                listener.Dispose();
            });
        }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

        recognizer.StartListening(CreateRecognizerIntent());
        return taskCompletion.Task;
    }

    private sealed class ContinuousRecognitionListener(AndroidSpeechCaptureService owner)
        : Java.Lang.Object, IRecognitionListener
    {
        private string lastPartialText = string.Empty;

        public void OnReadyForSpeech(Bundle? @params)
        {
        }

        public void OnBeginningOfSpeech()
        {
        }

        public void OnRmsChanged(float rmsdB)
        {
        }

        public void OnBufferReceived(byte[]? buffer)
        {
        }

        public void OnEndOfSpeech()
        {
        }

        public void OnError(SpeechRecognizerError error)
        {
            owner.SetListening(false);
            owner.ScheduleContinuousRestart();
        }

        public void OnResults(Bundle? results)
        {
            var text = ReadBestMatch(results) ?? lastPartialText;
            lastPartialText = string.Empty;
            owner.PublishRecognizedText(text, true);
            owner.SetListening(false);
            owner.ScheduleContinuousRestart();
        }

        public void OnPartialResults(Bundle? partialResults)
        {
            lastPartialText = ReadBestMatch(partialResults) ?? lastPartialText;
        }

        public void OnEvent(int eventType, Bundle? @params)
        {
        }
    }

    private sealed class SingleUtteranceRecognitionListener(TaskCompletionSource<string> taskCompletion)
        : Java.Lang.Object, IRecognitionListener
    {
        public string BestText { get; private set; } = string.Empty;

        public void OnReadyForSpeech(Bundle? @params)
        {
        }

        public void OnBeginningOfSpeech()
        {
        }

        public void OnRmsChanged(float rmsdB)
        {
        }

        public void OnBufferReceived(byte[]? buffer)
        {
        }

        public void OnEndOfSpeech()
        {
        }

        public void OnError(SpeechRecognizerError error)
        {
            taskCompletion.TrySetResult(BestText);
        }

        public void OnResults(Bundle? results)
        {
            BestText = ReadBestMatch(results) ?? BestText;
            taskCompletion.TrySetResult(BestText);
        }

        public void OnPartialResults(Bundle? partialResults)
        {
            BestText = ReadBestMatch(partialResults) ?? BestText;
        }

        public void OnEvent(int eventType, Bundle? @params)
        {
        }
    }

    private static string? ReadBestMatch(Bundle? bundle)
    {
        var matches = bundle?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
        return matches?.FirstOrDefault(match => !string.IsNullOrWhiteSpace(match));
    }
}
