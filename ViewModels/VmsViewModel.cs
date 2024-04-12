using ClienteServidorProyectoU2.Models;
using ClienteServidorProyectoU2.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClienteServidorProyectoU2.ViewModels
{
    public class VmsViewModel : INotifyPropertyChanged
    {
        public Vms VmsNvo { get; set; } = new();
        public string IP { get; set; } = "0.0.0.0";
        public ObservableCollection<Vms> ListaMensajes { get; set; } = new();
        //public ICommand CerrarConexionCommand { get; set; }


        VmsServer server = new();

        public VmsViewModel()
        {
            var direcciones = Dns.GetHostAddresses
               (Dns.GetHostName());

            if (direcciones != null)
            {
                IP = string.Join(",", direcciones
                    .Where(x => x.AddressFamily ==
                    System.Net.Sockets.AddressFamily
                    .InterNetwork).Select(x => x.ToString()).FirstOrDefault());
            }

            server.MensajeRecibido += Server_MensajeRecibido;
            server.Iniciar();

            //CerrarConexionCommand = new RelayCommand(CerrarConexion);

            ListaMensajes = server.CargarArchivo();
            //Verifica si la lista se a creado, caso contrario se crea 
            if (ListaMensajes != null && ListaMensajes.Any())
            {
                VmsNvo.Texto = ListaMensajes.LastOrDefault().Texto;
            }
            else
            {
                ListaMensajes = new();
            }
        }

        private void CerrarConexion()
        {
            server.Terminar();
        }

        private void Server_MensajeRecibido(object? sender, Vms e)
        {
            VmsNvo = e;
            ListaMensajes.Add(e);

            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Vms"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VmsNvo"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
