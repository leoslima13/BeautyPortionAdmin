using BeautyPortionAdmin.Bootstraping;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace BeautyPortionAdmin.Services
{
    public interface IMediaPickerService
    {
        IObservable<MediaPickerResult> PickPhoto(PickMediaOptions options = null);
        IObservable<MediaPickerResult> TakePhoto(StoreCameraMediaOptions options = null);
    }

    [Singleton]
    public class MediaPickerService : IMediaPickerService, IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IPermissionsService _permissionsService;

        public MediaPickerService(IPermissionsService devicePermissionsService)
        {
            _permissionsService = devicePermissionsService;
        }

        public IObservable<MediaPickerResult> PickPhoto(PickMediaOptions options = null)
        {
            return Observable.Create<MediaPickerResult>(o =>
            {
                var disposables = new CompositeDisposable();

                CrossMedia.Current.Initialize()
                    .ToObservable()
                    .Subscribe(OnInitialized, ex => OnError(ex, o))
                    .AddTo(disposables);

                void OnInitialized(bool isInitialized)
                {
                    if (!isInitialized)
                    {
                        o.OnNext(new MediaPickerResult(null, MediaPickerStatus.Fail, new Exception("Error on initialize")));
                        o.OnCompleted();
                        return;
                    }

                    if (Device.RuntimePlatform == Device.Android)
                    {
                        _permissionsService.RequestPermission<StorageRead>()
                            .Subscribe(OnPermissionRequested, ex => OnError(ex, o))
                            .AddTo(disposables);
                        return;
                    }

                    if (Device.RuntimePlatform == Device.iOS && DeviceInfo.Version.Major < 11 || DeviceInfo.Version.Major >= 14)
                    {
                        _permissionsService.RequestPermission<Photos>()
                            .Subscribe(OnPermissionRequested, ex => OnError(ex, o))
                            .AddTo(disposables);

                        return;
                    }

                    PickPhoto();

                    void OnPermissionRequested(PermissionResult permissionResult)
                    {
                        if (permissionResult.PermissionStatus == PermissionStatus.Denied)
                        {
                            PermissionDenied(o, permissionResult.IsFirstTimeRequested);
                            return;
                        }
                        PickPhoto();
                    }

                    void PickPhoto()
                    {
                        CrossMedia.Current.PickPhotoAsync(options)
                            .ToObservable()
                            .ObserveOnUIDispatcher()
                            .Subscribe(OnPickPhoto, ex => OnError(ex, o))
                            .AddTo(disposables);

                        void OnPickPhoto(MediaFile mediaFile)
                        {
                            //Probably user canceled the operation
                            if (mediaFile == null)
                                o.OnNext(new MediaPickerResult(null, MediaPickerStatus.Canceled, null));
                            else
                                o.OnNext(new MediaPickerResult(mediaFile, MediaPickerStatus.Success, null));

                            o.OnCompleted();
                        }
                    }
                }

                return disposables;
            });
        }

        public IObservable<MediaPickerResult> TakePhoto(StoreCameraMediaOptions options = null)
        {
            return Observable.Create<MediaPickerResult>(o =>
            {
                var disposables = new CompositeDisposable();

                CrossMedia.Current.Initialize()
                    .ToObservable()
                    .Subscribe(OnInitialized, ex => OnError(ex, o))
                    .AddTo(disposables);

                void OnInitialized(bool isInitialized)
                {
                    if (!isInitialized)
                    {
                        o.OnNext(new MediaPickerResult(null, MediaPickerStatus.Fail, new Exception("Error on initialize")));
                        o.OnCompleted();
                        return;
                    }

                    _permissionsService.RequestPermission<Camera>()
                        .ObserveOnUIDispatcher()
                        .Subscribe(OnPermissionRequested, ex => OnError(ex, o))
                        .AddTo(disposables);

                    void OnPermissionRequested(PermissionResult permissionResult)
                    {
                        if (permissionResult.PermissionStatus == PermissionStatus.Denied)
                        {
                            PermissionDenied(o, permissionResult.IsFirstTimeRequested);
                            return;
                        }
                        TakePhoto();
                    }
                }

                void TakePhoto()
                {
                    CrossMedia.Current.TakePhotoAsync(options)
                        .ToObservable()
                        .ObserveOnUIDispatcher()
                        .Subscribe(OnTakePhoto, ex => OnError(ex, o))
                        .AddTo(disposables);

                    void OnTakePhoto(MediaFile mediaFile)
                    {
                        //Probably user canceled the operation
                        if (mediaFile == null)
                            o.OnNext(new MediaPickerResult(null, MediaPickerStatus.Canceled, null));
                        else
                            o.OnNext(new MediaPickerResult(mediaFile, MediaPickerStatus.Success, null));

                        o.OnCompleted();
                    }
                }

                return disposables;
            });
        }

        private void OnError(Exception ex, IObserver<MediaPickerResult> observer)
        {
            if (ex is MediaPermissionException _)
            {
                observer.OnNext(new MediaPickerResult(null, MediaPickerStatus.PermissionDeniedFirstTime, ex));
                observer.OnCompleted();
                return;
            }

            observer.OnNext(new MediaPickerResult(null, MediaPickerStatus.Fail, ex));
            observer.OnCompleted();
        }

        private void PermissionDenied(IObserver<MediaPickerResult> observer, bool isFirstTimeRequested)
        {
            observer.OnNext(new MediaPickerResult(null, isFirstTimeRequested ? MediaPickerStatus.PermissionDeniedFirstTime : MediaPickerStatus.PermissionAlreadyDenied, null));
            observer.OnCompleted();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class MediaPickerResult
    {
        public MediaPickerResult(MediaFile mediaFile, MediaPickerStatus status, Exception exception)
        {
            MediaFile = mediaFile;
            Status = status;
            Exception = exception;
        }

        public MediaFile MediaFile { get; }
        public MediaPickerStatus Status { get; }
        public Exception Exception { get; }
    }

    public enum MediaPickerStatus
    {
        Success,
        Fail,
        Canceled,
        PermissionDeniedFirstTime,
        PermissionAlreadyDenied
    }
}
