using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransfer.Clases
{
    public class Servidor
    {
        private readonly int puerto;
        private readonly string carpetaRecepcion;
        private Action<string> log;
        private Action<int, double> actualizarProgreso;
        private Action<string, long, string> onArchivoRecibido;

        public Servidor(int puerto, string carpetaRecepcion, Action<string> log, Action<int, double> actualizarProgreso, Action<string,long,string> onArchivoRecibido)
        {
            this.puerto = puerto;
            this.carpetaRecepcion = carpetaRecepcion;
            this.log = log;
            this.actualizarProgreso = actualizarProgreso;
            this.onArchivoRecibido = onArchivoRecibido;
        }

        public void Iniciar()
        {
            Thread hiloServidor = new Thread(() =>
            {
                TcpListener servidor = new TcpListener(IPAddress.Any, puerto);
                servidor.Start();
                log("Servidor iniciado...");
                while (true)
                {
                    TcpClient cliente = servidor.AcceptTcpClient();
                    Thread hiloCliente = new Thread(() => ManejarCliente(cliente));
                    hiloCliente.Start();
                }
            });

            hiloServidor.IsBackground = true; // Permite que el hilo se detenga al cerrar la aplicación.
            hiloServidor.Start();
        }

        private void ManejarCliente(TcpClient cliente)
        {
            try
            {
                using NetworkStream stream = cliente.GetStream();
                using BinaryReader reader = new BinaryReader(stream);

                string nombreArchivo = reader.ReadString();
                long tamañoArchivo = reader.ReadInt64();
                string rutaArchivo = Path.Combine(carpetaRecepcion, nombreArchivo);

                using FileStream fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[1024];
                int bytesLeidos;
                long totalBytesLeidos = 0;

                while ((bytesLeidos = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesLeidos);
                    totalBytesLeidos += bytesLeidos;

                    // Actualizamos el progreso
                    double progreso = (double)totalBytesLeidos / tamañoArchivo;
                    actualizarProgreso(0, progreso); // Aquí solo actualizamos un único progreso para simplificar
                }

                log($"Archivo recibido: {nombreArchivo}");
                onArchivoRecibido(nombreArchivo, tamañoArchivo, ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString());
            }
            catch (Exception ex)
            {
                log($"Error recibiendo archivo: {ex.Message}");
            }
        }
    }


}
