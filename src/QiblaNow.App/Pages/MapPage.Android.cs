#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Maps.Handlers;

namespace QiblaNow.App.Pages;

public partial class MapPage
{
    private const float InitialMeccaZoom = 15f;
    private const float FinalUserZoom = 17f;
    private const double CameraBearingThresholdDegrees = 1.0;

    private GoogleMap? _nativeMap;
    private bool _nativeMapReady;
    private bool _isProgrammaticCameraChange;
    private double _lastUserLatitude;
    private double _lastUserLongitude;
    private double _lastAppliedBearing;

    partial void InitializeNativeMap()
    {
        QiblaMap.HandlerChanged += OnMapHandlerChanged;
    }

    private void OnMapHandlerChanged(object? sender, EventArgs e)
    {
        if (QiblaMap.Handler is not MapHandler handler)
            return;

        if (handler.PlatformView is not MapView nativeMapView)
            return;

        nativeMapView.GetMapAsync(new NativeMapReadyCallback(this));
    }

    partial void ApplyNativeMapBearing(double bearing)
    {
        _lastAppliedBearing = bearing;

        if (!_nativeMapReady || _nativeMap is null || !_initialFlightCompleted || _isProgrammaticCameraChange)
            return;

        var current = _nativeMap.CameraPosition;
        if (AngularDifferenceDegrees(current.Bearing, bearing) < CameraBearingThresholdDegrees)
            return;

        var next = new CameraPosition.Builder(current)
            .Target(new LatLng(_lastUserLatitude, _lastUserLongitude))
            .Bearing((float)bearing)
            .Build();

        MoveCameraSafely(() =>
        {
            _nativeMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(next));
        });
    }

    partial void UpdateNativeUserLocation(double latitude, double longitude)
    {
        _lastUserLatitude = latitude;
        _lastUserLongitude = longitude;

        if (!_nativeMapReady || _nativeMap is null || !_initialFlightCompleted || _isProgrammaticCameraChange)
            return;

        var current = _nativeMap.CameraPosition;
        var next = new CameraPosition.Builder(current)
            .Target(new LatLng(latitude, longitude))
            .Bearing((float)_lastAppliedBearing)
            .Build();

        MoveCameraSafely(() =>
        {
            _nativeMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(next));
        });
    }

    private partial async Task TryRunInitialFlightAsync()
    {
        if (_initialFlightCompleted || !_viewModel.HasLocation || !_nativeMapReady || _nativeMap is null)
            return;

        var mecca = new LatLng(_viewModel.TargetLatitude, _viewModel.TargetLongitude);
        var user = new LatLng(_viewModel.UserLatitude, _viewModel.UserLongitude);

        MoveCameraSafely(() =>
        {
            _nativeMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(mecca, InitialMeccaZoom));
        });

        await Task.Delay(250);

        var bounds = new LatLngBounds.Builder()
            .Include(mecca)
            .Include(user)
            .Build();

        await AnimateCameraAsync(CameraUpdateFactory.NewLatLngBounds(bounds, 160), 1800);

        var finalCamera = new CameraPosition.Builder()
            .Target(user)
            .Zoom(FinalUserZoom)
            .Bearing((float)_lastAppliedBearing)
            .Tilt(0f)
            .Build();

        await AnimateCameraAsync(CameraUpdateFactory.NewCameraPosition(finalCamera), 1800);

        _initialFlightCompleted = true;
    }

    private void OnNativeMapReady(GoogleMap googleMap)
    {
        _nativeMap = googleMap;
        _nativeMapReady = true;

        _nativeMap.UiSettings.ScrollGesturesEnabled = false;
        _nativeMap.UiSettings.ZoomGesturesEnabled = true;
        _nativeMap.UiSettings.RotateGesturesEnabled = false;
        _nativeMap.UiSettings.TiltGesturesEnabled = false;
        _nativeMap.UiSettings.CompassEnabled = false;
        _nativeMap.UiSettings.MapToolbarEnabled = false;

        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await TryRunInitialFlightAsync();
        });
    }

    private async Task AnimateCameraAsync(CameraUpdate update, int durationMs)
    {
        if (_nativeMap is null)
            return;

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _isProgrammaticCameraChange = true;

        try
        {
            _nativeMap.AnimateCamera(update, durationMs, new CameraAnimationCallback(tcs));
            await tcs.Task;
        }
        finally
        {
            _isProgrammaticCameraChange = false;
        }
    }

    private void MoveCameraSafely(Action move)
    {
        _isProgrammaticCameraChange = true;

        try
        {
            move();
        }
        finally
        {
            _isProgrammaticCameraChange = false;
        }
    }

    private static double AngularDifferenceDegrees(double a, double b)
    {
        var diff = Math.Abs((a - b) % 360d);
        return diff > 180d ? 360d - diff : diff;
    }

    private sealed class NativeMapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        private readonly MapPage _page;

        public NativeMapReadyCallback(MapPage page)
        {
            _page = page;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _page.OnNativeMapReady(googleMap);
        }
    }

    private sealed class CameraAnimationCallback : Java.Lang.Object, GoogleMap.ICancelableCallback
    {
        private readonly TaskCompletionSource _tcs;

        public CameraAnimationCallback(TaskCompletionSource tcs)
        {
            _tcs = tcs;
        }

        public void OnFinish()
        {
            _tcs.TrySetResult();
        }

        public void OnCancel()
        {
            _tcs.TrySetResult();
        }
    }
}
#endif