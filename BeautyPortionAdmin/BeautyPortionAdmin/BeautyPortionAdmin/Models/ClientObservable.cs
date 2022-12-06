using BeautyPortionAdmin.Bootstraping;
using BeautyPortionAdmin.Framework;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;

namespace BeautyPortionAdmin.Models
{
    public interface IClientObservable : ICrudObservable<Client> { }

    [Singleton]
    public class ClientObservable : IClientObservable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BehaviorSubject<List<Client>> _clients;

        public ClientObservable()
        {
            _clients = new BehaviorSubject<List<Client>>(new List<Client>(MockClients()))
                .AddTo(_disposables);
        }

        public IObservable<IEnumerable<Client>> ObserveItems => _clients;

        private List<Client> MockClients()
        {
            return new List<Client>
            {
                new Client { FirstName = "Leonardo", LastName = "Lima", Phone = "31-98461-2727", Address="Rua Eugenio Sales, 270, Ap 202" },
                new Client { FirstName = "Leonardo", LastName = "Soares", Phone = "31-98461-2727", Address="Rua Eugenio Sales, 270, Ap 202" },
                new Client { FirstName = "Yasmin", LastName = "Santos", Phone = "31-98461-2727", Address="Rua Eugenio Sales, 270, Ap 202" },
                new Client { FirstName = "Amanda", LastName = "Alves", Phone = "31-98461-2727", Address="Rua Eugenio Sales, 270, Ap 202" },
                new Client { FirstName = "Big", LastName = "Jhow", Phone = "31-98461-2727", Address="Rua Eugenio Sales, 270, Ap 202" }
            };
        }

        public void AddItem(Client client)
        {
            var clients = _clients.Value;
            clients.Add(client);
            _clients.OnNext(clients);
        }

        public void RemoveItem(Client client)
        {
            var clients = _clients.Value;
            clients.Remove(client);
            _clients.OnNext(clients);
        }

        public void UpdateItem(Client client)
        {
            var clientIndex = _clients.Value.FindIndex(x => x.Id == client.Id);
            _clients.Value[clientIndex] = client;
            _clients.OnNext(_clients.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
