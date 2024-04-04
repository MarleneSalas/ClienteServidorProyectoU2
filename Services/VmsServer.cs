using ClienteServidorProyectoU2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace ClienteServidorProyectoU2.Services
{
    public class VmsServer
    {
        HttpListener server = new();

        public VmsServer()
        {
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

        public event EventHandler<Vms>? MensajeRecibido;

        public void Escuchar()
        {
            while (true)
            {
                var context = server.GetContext();
                var page = File.ReadAllText("assets/index.html");
                //var indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                //    "assets", "index.html"); 
                //var page = File.ReadAllText(indexPath);
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
                        var data = Encoding.UTF8.GetString(bufferData);
                        context.Response.StatusCode=200;
                        context.Response.Close();

                        var dictionary = HttpUtility.ParseQueryString(data);

                        Vms message = new()
                        {
                            Texto = (dictionary["Texto"]??"").ToUpper()
                        };

                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            MensajeRecibido?.Invoke(this, message);
                        });

                        //context.Response.StatusCode = 200;
                        //context.Response.Close();
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
            }
        }

        public void Terminar()
        {
            server.Stop();
        }
    }
}
