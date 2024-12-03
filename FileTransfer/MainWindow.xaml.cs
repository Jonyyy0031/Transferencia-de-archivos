using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileTransfer.Clases;
using System.Threading;

namespace FileTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int puerto = 8000;
        private readonly string carpetaEnvio = @"C:\FileTransfer\Send";
        private readonly string carpetaRecepcion = @"C:\FileTransfer\Receive";

        // Diccionario que asocia IPs con nombres de PC
        private readonly Dictionary<string, string> nombrePorIP = new Dictionary<string, string>
    {
        { "192.168.1.93", "PC-Jony" }
    };

        private Servidor servidor;
        private Cliente cliente;

        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(carpetaEnvio);
            Directory.CreateDirectory(carpetaRecepcion);

            servidor = new Servidor(puerto, carpetaRecepcion, Log, ActualizarProgreso, ArchivoRecibido);
            cliente = new Cliente(puerto, Log, ActualizarProgreso);
        }

        private void StartTransfer_Click(object sender, RoutedEventArgs e)
        {
            servidor.Iniciar();

            Thread hiloTransferencia = new Thread(() =>
            {
                int index = 0;
                foreach (var entry in nombrePorIP)  // Iteramos sobre el diccionario
                {
                    string ip = entry.Key;
                    string nombrePC = entry.Value;  // Nombre asociado a la IP

                    foreach (var rutaArchivo in Directory.GetFiles(carpetaEnvio))
                    {
                        Archivo archivo = new Archivo(rutaArchivo);
                        AgregarProgressBar(index, archivo);
                        cliente.EnviarArchivo(ip, archivo, index, nombrePC);  // Pasamos la IP y el nombre
                        index++;
                    }
                }
            });

            hiloTransferencia.Start();
        }

        private void RegistrarArchivoRecibido(string nombre, long tamaño, string origen)
        {
            Dispatcher.Invoke(() =>
            {
                var item = new FileHistoryItem
                {
                    Nombre = nombre,
                    Tamaño = FormatFileSize(tamaño),
                    Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Origen = origen,
                    Estado = "Completado"
                };

                HistoryList.Items.Add(item);
            });
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (number > 1024 && counter < suffixes.Length - 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        // Llamar a este método cuando se complete una transferencia
        private void ArchivoRecibido(string nombre, long tamaño, string ipOrigen)
        {
            RegistrarArchivoRecibido(nombre, tamaño, ipOrigen);
        }

        private void AgregarProgressBar(int index, Archivo archivo)
        {
            Dispatcher.Invoke(() =>
            {
                var container = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var header = new TextBlock
                {
                    Text = $"Transfiriendo: {archivo.Nombre}",
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var progressBar = new ProgressBar
                {
                    Name = $"progress_{index}",
                    Height = 20,
                    Width = double.NaN, // Stretch to fill
                    Minimum = 0,
                    Maximum = 1,
                    Value = 0
                };

                var percentageText = new TextBlock
                {
                    Text = "0%",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                container.Children.Add(header);
                container.Children.Add(progressBar);
                container.Children.Add(percentageText);

                ProgressPanel.Children.Add(container);
            });
        }

        private void ActualizarProgreso(int index, double progreso)
        {
            Dispatcher.Invoke(() =>
            {
                var container = ProgressPanel.Children[index] as StackPanel;
                var progressBar = container.Children[1] as ProgressBar;
                var percentageText = container.Children[2] as TextBlock;

                progressBar.Value = progreso;
                percentageText.Text = $"{progreso:P0}";
            });
        }

        private void Log(string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                var logItem = new TextBlock
                {
                    Text = $"[{DateTime.Now:HH:mm:ss}] {mensaje}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                LogBox.Items.Add(logItem);
                LogBox.ScrollIntoView(logItem);
            });
        }
    }



}
