using Android.Content;
using Android.Media;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// Plays Adhan MP3 files as prayer-time alarms using the device's alarm audio stream.
/// Only one playback instance runs at a time; calling <see cref="Play"/> stops any
/// existing playback before starting the new track.
/// </summary>
public sealed class AndroidAdhanAlarmPlayer : IAdhanAlarmPlayer
{
    private readonly Context _context;
    private MediaPlayer? _player;
    private readonly object _lock = new();

    public AndroidAdhanAlarmPlayer(Context context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                try { return _player?.IsPlaying == true; }
                catch { return false; }
            }
        }
    }

    public void Play(AdhanSound sound)
    {
        lock (_lock)
        {
            StopInternal();

            try
            {
                var uri = sound == AdhanSound.Default
                    ? RingtoneManager.GetDefaultUri(RingtoneType.Alarm)
                    : global::Android.Net.Uri.Parse(
                        $"android.resource://{_context.PackageName}/raw/{RawName(sound)}");

                _player = new MediaPlayer();
                _player.SetAudioAttributes(
                    new AudioAttributes.Builder()
                        .SetUsage(AudioUsageKind.Alarm)!
                        .SetContentType(AudioContentType.Music)!
                        .Build()!);

                // Attach Completion handler before Prepare/Start to avoid the race condition
                // where a very short clip finishes before the handler is registered.
                _player.Completion += (_, _) =>
                {
                    lock (_lock) { StopInternal(); }
                };

                _player.SetDataSource(_context, uri!);
                _player.Prepare();
                _player.Start();

                System.Diagnostics.Debug.WriteLine($"AndroidAdhanAlarmPlayer: started playback of {sound}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAdhanAlarmPlayer.Play failed: {ex}");
                StopInternal();
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            StopInternal();
            System.Diagnostics.Debug.WriteLine("AndroidAdhanAlarmPlayer: stopped by user");
        }
    }

    private void StopInternal()
    {
        try
        {
            if (_player?.IsPlaying == true)
                _player.Stop();
            _player?.Release();
        }
        catch
        {
            // Ignore — player may already be in an invalid state.
        }
        finally
        {
            _player = null;
        }
    }

    private static string RawName(AdhanSound sound) => sound switch
    {
        AdhanSound.Adhan1 => "adhan",
        AdhanSound.Adhan2 => "adhan2",
        AdhanSound.Adhan3 => "adhan3",
        _                 => "adhan",
    };
}
