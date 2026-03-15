using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.Presentation.Tests;

public class ViewModelTests
{
    [Fact]
    public void ViewModels_Inherit_From_ObservableObject()
    {
        // Placeholder test for M01
        // This will be expanded in subsequent milestones
        Assert.True(true);
    }

    // ── Sound selection tests ──────────────────────────────────────────────────

    private static SettingsViewModel BuildSettingsViewModel(
        ISettingsStore? store = null,
        IAdhanPlayer? player = null)
    {
        store  ??= new InMemorySettingsStore();
        player ??= new RecordingAdhanPlayer();
        return new SettingsViewModel(
            store,
            new NullLocationService(),
            new NullNotificationScheduler(),
            player,
            new NullNotificationSettingsOpener());
    }

    [Theory]
    [InlineData(AdhanSound.Default)]
    [InlineData(AdhanSound.Adhan1)]
    [InlineData(AdhanSound.Adhan2)]
    [InlineData(AdhanSound.Adhan3)]
    public void SelectedAdhan_Boolean_Helpers_Are_Exclusive(AdhanSound sound)
    {
        var vm = BuildSettingsViewModel();
        vm.SelectedAdhan = sound;

        Assert.Equal(sound == AdhanSound.Default, vm.IsDefaultSoundSelected);
        Assert.Equal(sound == AdhanSound.Adhan1,  vm.IsAdhan1Selected);
        Assert.Equal(sound == AdhanSound.Adhan2,  vm.IsAdhan2Selected);
        Assert.Equal(sound == AdhanSound.Adhan3,  vm.IsAdhan3Selected);
    }

    [Fact]
    public void SelectAdhan2Command_Updates_SelectedAdhan()
    {
        var vm = BuildSettingsViewModel();

        vm.SelectAdhan2Command.Execute(null);

        Assert.Equal(AdhanSound.Adhan2, vm.SelectedAdhan);
        Assert.True(vm.IsAdhan2Selected);
    }

    [Fact]
    public void SelectedAdhan_Change_Is_Persisted_In_Store()
    {
        var store = new InMemorySettingsStore();
        var vm    = BuildSettingsViewModel(store: store);

        vm.SelectedAdhan = AdhanSound.Adhan3;

        var saved = store.GetNotificationSettings().SelectedAdhan;
        Assert.Equal(AdhanSound.Adhan3, saved);
    }

    [Fact]
    public void PreviewAdhan1Command_Calls_Player_Preview()
    {
        var player = new RecordingAdhanPlayer();
        var vm     = BuildSettingsViewModel(player: player);

        vm.PreviewAdhan1Command.Execute(null);

        Assert.Equal(AdhanSound.Adhan1, player.LastPreviewed);
    }

    [Fact]
    public void PreviewAdhan2Command_Calls_Player_Preview()
    {
        var player = new RecordingAdhanPlayer();
        var vm     = BuildSettingsViewModel(player: player);

        vm.PreviewAdhan2Command.Execute(null);

        Assert.Equal(AdhanSound.Adhan2, player.LastPreviewed);
    }

    [Fact]
    public void PreviewAdhan3Command_Calls_Player_Preview()
    {
        var player = new RecordingAdhanPlayer();
        var vm     = BuildSettingsViewModel(player: player);

        vm.PreviewAdhan3Command.Execute(null);

        Assert.Equal(AdhanSound.Adhan3, player.LastPreviewed);
    }

    [Fact]
    public void Cleanup_Calls_StopPreview_On_Player()
    {
        var player = new RecordingAdhanPlayer();
        var vm     = BuildSettingsViewModel(player: player);

        vm.Cleanup();

        Assert.True(player.StopPreviewCalled);
    }

    [Fact]
    public void Refresh_Restores_SelectedAdhan_From_Store()
    {
        var store = new InMemorySettingsStore();
        store.SaveNotificationSettings(new PrayerNotificationSettings
        {
            SelectedAdhan = AdhanSound.Adhan2
        });

        var vm = BuildSettingsViewModel(store: store);

        // Simulate navigating back to the page which calls Refresh()
        vm.Refresh();

        Assert.Equal(AdhanSound.Adhan2, vm.SelectedAdhan);
        Assert.True(vm.IsAdhan2Selected);
    }

    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        private PrayerNotificationSettings _notif   = new();
        private PrayerCalculationSettings  _calc    = new();
        private LocationMode               _mode    = LocationMode.GPS;
        private LocationSnapshot?          _snapshot;
        private SchedulingState            _sched   = new();

        public LocationMode GetLocationMode()                                      => _mode;
        public void SetLocationMode(LocationMode mode)                             => _mode = mode;
        public LocationSnapshot? GetLastSnapshot()                                 => _snapshot;
        public void SaveSnapshot(LocationSnapshot s)                               => _snapshot = s;
        public PrayerCalculationSettings GetCalculationSettings()                  => _calc;
        public void SaveCalculationSettings(PrayerCalculationSettings s)           => _calc = s;
        public PrayerNotificationSettings GetNotificationSettings()                => _notif;
        public void SaveNotificationSettings(PrayerNotificationSettings s)         => _notif = s;
        public LocationSnapshot? GetLastValidLocation()                            => _snapshot;
        public void SaveLastValidLocation(LocationSnapshot s)                      => _snapshot = s;
        public SchedulingState GetSchedulingState()                                => _sched;
        public void SaveSchedulingState(SchedulingState s)                         => _sched = s;
    }

    private sealed class RecordingAdhanPlayer : IAdhanPlayer
    {
        public AdhanSound? LastPreviewed    { get; private set; }
        public bool        StopPreviewCalled { get; private set; }

        public void Preview(AdhanSound sound) => LastPreviewed = sound;
        public void StopPreview()             => StopPreviewCalled = true;
    }

    private sealed class NullLocationService : ILocationService
    {
        public Task<LocationSnapshot?> GetCurrentLocationAsync()             => Task.FromResult<LocationSnapshot?>(null);
        public Task<LocationSnapshot?> RequestGpsLocationAsync()             => Task.FromResult<LocationSnapshot?>(null);
        public Task<LocationSnapshot?> TryGetGpsLocationAsync(TimeSpan t)   => Task.FromResult<LocationSnapshot?>(null);
    }
}
