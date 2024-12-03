using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Clases
{
    public class Archivo
    {
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public long Tamaño { get; set; }

        public Archivo(string ruta)
        {
            Ruta = ruta;
            Nombre = Path.GetFileName(ruta);
            Tamaño = new FileInfo(ruta).Length;
        }
    }

}
