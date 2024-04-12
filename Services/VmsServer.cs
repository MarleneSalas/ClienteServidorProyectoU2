using ClienteServidorProyectoU2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace ClienteServidorProyectoU2.Services
{
    public class VmsServer
    {
        HttpListener server = new();

        // Directorio en el que se encuentra el archivo json
        string ArchivoJson;

        public VmsServer()
        {
            // Guardar json en la ruta especificada, por defecto el json se guarda en la carpeta bin, en este caso se guardara en assets
            ArchivoJson = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "Mensajes.json");

            //netsh add http urlacl "http://*:5400/VMS/" user = Todos
            server.Prefixes.Add("http://*:5400/VMS/");
        }

        public void Iniciar()
        {
            if (!server.IsListening)
            {
                server.Start();

                new Thread(Escuchar) { IsBackground = true }.Start();
            }
        }

        // Metodo que sera llamado cada que el servidor reciba un mensaje
        void GuardarMensaje(Vms mensaje)
        {
            List<Vms> mensajesRecibidos;

            //Verifica si el archivo existe, en caso contrario crea una nueva lista de los mensajes recibidos
            if (File.Exists(ArchivoJson))
            {
                //Deserializa los mensajes que se encuentran en el archivo
                var json = File.ReadAllText(ArchivoJson);
                mensajesRecibidos = JsonConvert.DeserializeObject<List<Vms>>(json);
            }
            else
            {
                mensajesRecibidos= new();
            }

            //Agrega el mensajes a la lista
            mensajesRecibidos?.Add(mensaje);

            //Serializa la lista de los mensajes
            string mensajesJson = JsonConvert.SerializeObject(mensajesRecibidos);

            //Guarda la lista serializada en el archivo
            File.WriteAllText(ArchivoJson, mensajesJson);
        }

        public event EventHandler<Vms>? MensajeRecibido;

        public void Escuchar()
        {
            while (true)
            {
                var context = server.GetContext();
                var page = File.ReadAllText("assets/index.html");
                var bufferPage=Encoding.UTF8.GetBytes(page);

                if(context.Request.Url != null)
                {
                    if (context.Request.Url.LocalPath =="/VMS/")
                    {
                        context.Response.ContentLength64 = bufferPage.Length;
                        context.Response.OutputStream.Write(bufferPage,0,bufferPage.Length);
                        context.Response.StatusCode = 200;
                        context.Response.Close();
                    }
                    else if(context.Request.HttpMethod =="POST" && context.Request.Url.LocalPath == "/VMS/nuevo")
                    {
                        var bufferData = new byte[context.Request.ContentLength64];
                        context.Request.InputStream.Read(bufferData,0,bufferData.Length);
                        string data = Encoding.UTF8.GetString(bufferData);
                        string referer = context.Request.Headers["Referer"]?? "";

                        context.Response.StatusCode=200;
                        context.Response.Redirect(referer);
                        context.Response.Close();

                        var dictionary = HttpUtility.ParseQueryString(data);

                        Vms message = new()
                        {
                            Fecha = DateTime.Now,
                            Texto = (dictionary["Texto"]??"").ToUpper()
                        };

                        //Mandara a llamar al metodo
                        GuardarMensaje(message);

                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            MensajeRecibido?.Invoke(this, message);
                        });

                        
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
            }
        }

        public ObservableCollection<Vms>? CargarArchivo()
        {
            if (File.Exists(ArchivoJson))
            {
                var json = File.ReadAllText(ArchivoJson);

                var mensajesRecibidos = JsonConvert.DeserializeObject<List<Vms>>(json);
                return new ObservableCollection<Vms>(mensajesRecibidos);
            }
            return null;
        }

        public void Terminar()
        {
            server.Stop();
        }
    }
}
