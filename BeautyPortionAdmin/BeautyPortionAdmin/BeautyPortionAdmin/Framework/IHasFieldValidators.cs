using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;

namespace BeautyPortionAdmin.Framework
{
    public interface IHasFieldValidators
    {
        IFieldValidatorsObservable FieldValidatorsObservable { get; }
    }

    public interface IFieldValidatorsObservable : IDisposable
    {
        IObservable<bool> ObserveFieldHasErrors { get; }
        void SetFieldError(Guid fieldId, bool hasError);
    }

    public class FieldValidatorsObservable : IFieldValidatorsObservable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BehaviorSubject<bool> _fieldErrorsSubject;
        private readonly Dictionary<Guid, bool> _dicFieldsValidations = new Dictionary<Guid, bool>();

        public FieldValidatorsObservable()
        {
            _fieldErrorsSubject = new BehaviorSubject<bool>(false).AddTo(_disposables);
        }

        public IObservable<bool> ObserveFieldHasErrors => _fieldErrorsSubject;

        public void SetFieldError(Guid fieldId, bool hasError)
        {
            if (_dicFieldsValidations.ContainsKey(fieldId))
                _dicFieldsValidations[fieldId] = hasError;
            else
                _dicFieldsValidations.Add(fieldId, hasError);

            _fieldErrorsSubject.OnNext(_dicFieldsValidations.Values.Any(x => x));
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
