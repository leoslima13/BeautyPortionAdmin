using BeautyPortionAdmin.Bootstraping;
using BeautyPortionAdmin.Models;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace BeautyPortionAdmin.Services
{
    public interface IPermissionsService : IDisposable
    {
        IObservable<PermissionResult> RequestPermission<T>() where T : BasePermission, new();
        void GoToSettings(string message);
    }

    [Singleton]
    public class PermissionsService : IPermissionsService
    {
        private readonly ISecureStorageService _secureStorageService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public PermissionsService(ISecureStorageService secureStorageService)
        {
            _secureStorageService = secureStorageService;
        }

        public IObservable<PermissionResult> RequestPermission<T>() where T : BasePermission, new()
        {
            return Observable.Create<PermissionResult>(o =>
            {
                var disposables = new CompositeDisposable();

                _secureStorageService.GetValue<DevicePermissions>(nameof(DevicePermissions))
                    .ToObservable()
                    .Subscribe(OnDevicePermissions, o.OnError)
                    .AddTo(disposables);

                void OnDevicePermissions(DevicePermissions devicePermissions)
                {
                    if (devicePermissions == null || !devicePermissions.PermissionsRequested.ContainsKey(typeof(T)))
                    {
                        RequestAsync<T>()
                            .ToObservable()
                            .ObserveOnUIDispatcher()
                            .Subscribe(x => OnPermissionRequested(x, true), o.OnError)
                            .AddTo(disposables);
                        return;

                    }

                    RequestAsync<T>()
                        .ToObservable()
                        .ObserveOnUIDispatcher()
                        .Subscribe(x => OnPermissionRequested(x, false), o.OnError)
                        .AddTo(disposables);

                    void OnPermissionRequested(PermissionStatus permissionStatus, bool isFirstTimeRequested)
                    {
                        o.OnNext(new PermissionResult(permissionStatus, isFirstTimeRequested));
                        o.OnCompleted();
                        SavePermission(devicePermissions, typeof(T));
                    }
                }

                return disposables;
            });
        }

        public void GoToSettings(string message)
        {
            //_pageDialogService.DisplayAlert(CommonResources.PermissionNeeded, message, CommonResources.GoSettings, CommonResources.ContinueWithoutPermission, OnGoSettings, _navigator.PopPopup, canClose: false);

            //void OnGoSettings()
            //{
            //    _navigator.PopPopup();
            //    AppInfo.ShowSettingsUI();
            //}
        }

        private void SavePermission(DevicePermissions devicePermissions, Type permissionType)
        {
            if (devicePermissions == null)
                devicePermissions = new DevicePermissions();

            if (devicePermissions.PermissionsRequested.ContainsKey(permissionType))
                devicePermissions.PermissionsRequested[permissionType] = true;
            else
                devicePermissions.PermissionsRequested.Add(permissionType, true);

            _secureStorageService.SetValue(devicePermissions, nameof(DevicePermissions));
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class PermissionResult
    {
        public PermissionResult(PermissionStatus permissionStatus, bool isFirstTimeRequested)
        {
            PermissionStatus = permissionStatus;
            IsFirstTimeRequested = isFirstTimeRequested;
        }
        public PermissionStatus PermissionStatus { get; }
        public bool IsFirstTimeRequested { get; }
    }

}
