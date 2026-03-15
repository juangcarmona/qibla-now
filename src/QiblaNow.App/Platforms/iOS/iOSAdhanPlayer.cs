using AVFoundation;
using Foundation;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.iOS;

/// <summary>
/// Plays a short in-app preview of each Adhan option on iOS and Mac Catalyst.
/// Uses <see cref="AVAudioPlayer"/> so the full file is audible without the OS truncating it.
/// Only one track plays at a time; starting a new preview stops the previous one.
///
/// Note on iOS notification sounds: iOS requires notification sounds to be bundled as
/// .wav or .caf files (≤ 30 s) in the main app bundle.  The current app does not yet
/// include an iOS-specific UNUserNotificationCenter scheduler, so notification playback
/// falls back to the system default alert tone.  When an iOS notification scheduler is
/// added, it should reference these bundled sound file names in the UNNotificationSound
/// initialiser.
/// </summary>
public sealed class iOSAdhanPlayer : IAdhanPlayer
{
    private AVAudioPlayer? _player;
    private EventHandler<AVStatusEventArgs>? _completionHandler;

    public void Preview(AdhanSound sound)
    {
        StopPreview();

        if (sound == AdhanSound.Default)
            return; // system default — nothing for us to play here

        try
        {
            var name = RawName(sound);

            // MAUI copies Resources/Raw files to the root of the app bundle.
            var url = NSBundle.MainBundle.GetUrlForResource(name, "mp3");
            if (url == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"iOSAdhanPlayer: resource '{name}.mp3' not found in bundle.");
                return;
            }

            _player = AVAudioPlayer.FromUrl(url, out var error);
            if (error != null || _player == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"iOSAdhanPlayer: could not create player — {error?.LocalizedDescription}");
                return;
            }

            // Keep a reference so we can unsubscribe before disposal, avoiding a dangling reference.
            _completionHandler = (_, _) => StopPreview();
            _player.FinishedPlaying += _completionHandler;
            _player.PrepareToPlay();
            _player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"iOSAdhanPlayer.Preview failed: {ex}");
            StopPreview();
        }
    }

    public void StopPreview()
    {
        try
        {
            if (_player != null && _completionHandler != null)
            {
                _player.FinishedPlaying -= _completionHandler;
                _completionHandler = null;
            }

            _player?.Stop();
            _player?.Dispose();
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
