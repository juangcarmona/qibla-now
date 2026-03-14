using Android.Content;
using Android.Media;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// Plays a short preview of each Adhan option from the Sound &amp; Notifications settings page.
/// Uses MediaPlayer directly so the full audio file is audible (not truncated by the Ringtone API).
/// Only one track plays at a time; starting a new preview stops the previous one.
/// </summary>
public sealed class AndroidAdhanPlayer : IAdhanPlayer
{
    private readonly Context _context;
    private MediaPlayer? _player;

    public AndroidAdhanPlayer(Context context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Preview(AdhanSound sound)
    {
        // Always stop any currently-playing preview before starting a new one.
        StopPreview();

        try
        {
            var uri = sound == AdhanSound.Default
                ? RingtoneManager.GetDefaultUri(RingtoneType.Notification)
                : global::Android.Net.Uri.Parse(
                    $"android.resource://{_context.PackageName}/raw/{RawName(sound)}");

            _player = new MediaPlayer();
            _player.SetAudioAttributes(
                new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Notification)!
                    .SetContentType(AudioContentType.Sonification)!
                    .Build()!);

            _player.SetDataSource(_context, uri!);
            _player.Prepare();
            _player.Start();

            // Release resources as soon as playback finishes naturally.
            _player.Completion += (_, _) => StopPreview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidAdhanPlayer.Preview failed: {ex}");
            StopPreview();
        }
    }

    public void StopPreview()
    {
        try
        {
            if (_player?.IsPlaying == true)
                _player.Stop();
            _player?.Release();
        }
        catch
        {
            // Ignore errors during release — player may already be in an invalid state.
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
