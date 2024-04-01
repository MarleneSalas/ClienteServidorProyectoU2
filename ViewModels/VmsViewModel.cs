using ClienteServidorProyectoU2.Models;
using ClienteServidorProyectoU2.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClienteServidorProyectoU2.ViewModels
{
    public class VmsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Vms> Mensajes { get; set; } = new();

        public string IP
        {
            get
            {
                return string.Join("", Dns.GetHostAddresses(Dns.GetHostName()).
                    Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.ToString()));
            }
        }

        VmsServer server = new();

        public VmsViewModel()
        {
            server.MensajeRecibido += Server_MensajeRecibido;
            server.Iniciar();
        }

        private void Server_MensajeRecibido(object? sender, Vms e)
        {
            Mensajes.Add(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
