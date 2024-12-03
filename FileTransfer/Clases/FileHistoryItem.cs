using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Clases
{
    public class FileHistoryItem
    {
        public string Nombre { get; set; }
        public string Tamaño { get; set; }
        public string Fecha { get; set; }
        public string Origen { get; set; }
        public string Estado { get; set; }
    }

}
