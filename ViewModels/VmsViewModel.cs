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
        private Vms vmsselecciondo;

        public Vms VmsNvo
        {
            get { return vmsselecciondo; }
            set { vmsselecciondo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VmsNvo"));
            }
        }

        //public Vms VmsNvo { get; set; } = new();
        public string IP { get; set; } = "0.0.0.0";
        public ObservableCollection<Vms> ListaMensajes { get; set; } = new();
        //public ICommand CerrarConexionCommand { get; set; }


        VmsServer server = new();

        public VmsViewModel()
        {
            VmsNvo = new();
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

            var servercargar = server.CargarArchivo() ?? new ObservableCollection<Vms>();
            var listaOrdensda = servercargar.OrderByDescending(x => x.Fecha);

            foreach (var m in listaOrdensda)
            {
                ListaMensajes.Add(m);
            }

            if(ListaMensajes != null)
            {
                ListaMensajes.OrderByDescending(x=>x.Fecha).ToList();
            }
           
            //Verifica si la lista se a creado, caso contrario se crea 
            if (ListaMensajes != null && ListaMensajes.Any())
            {
                VmsNvo.Texto = ListaMensajes.FirstOrDefault().Texto;
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
            //ListaMensajes.Add(e);

            ListaMensajes.Insert(0,e);
           //var listaOrdenada = ListaMensajes.OrderByDescending(x => x.Fecha).ToList();

           // ListaMensajes.Clear();
           // foreach ( var x in listaOrdenada)
           // {
           //     ListaMensajes.Add(x);
           // }

            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Vms"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VmsNvo"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
