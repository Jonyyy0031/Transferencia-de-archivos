using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransfer.Clases
{
    public class Cliente
    {
        private readonly int puerto;
        private Action<string> log;
        private Action<int, double> actualizarProgreso; // Para actualizar el progreso

        public Cliente(int puerto, Action<string> log, Action<int, double> actualizarProgreso)
        {
            this.puerto = puerto;
            this.log = log;
            this.actualizarProgreso = actualizarProgreso;
        }

        public void EnviarArchivo(string direccionIP, Archivo archivo, int index, string nombrePC)
        {
            Thread hiloEnvio = new Thread(() =>
            {
                try
                {
                    using TcpClient cliente = new TcpClient(direccionIP, puerto);
                    using NetworkStream stream = cliente.GetStream();
                    using BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write(archivo.Nombre);
                    writer.Write(archivo.Tamaño);

                    using FileStream fileStream = new FileStream(archivo.Ruta, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[1024];
                    int bytesLeidos;
                    long totalBytesLeidos = 0;

                    while ((bytesLeidos = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        stream.Write(buffer, 0, bytesLeidos);
                        totalBytesLeidos += bytesLeidos;

                        // Actualizamos el progreso
                        double progreso = (double)totalBytesLeidos / archivo.Tamaño;
                        actualizarProgreso(index, progreso);
                    }

                    // Log con el nombre de la PC en lugar de la IP
                    log($"Archivo enviado: {archivo.Nombre} a {nombrePC}");  // Aquí se muestra el nombre en lugar de la IP
                }
                catch (Exception ex)
                {
                    log($"Error enviando archivo a {direccionIP}: {ex.Message}");
                }
            });

            hiloEnvio.Start();
        }



    }


}
