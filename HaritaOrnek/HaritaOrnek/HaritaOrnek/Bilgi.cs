using GeoCoordinatePortable;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaritaOrnek
{
    class Bilgi
    {
        public int MusteriID { get; set; }
        public int KargoID { get; set; }
        public string MüsteriAdi { get; set; }
        public string MusteriLat { get; set; }
        public string MusteriLng { get; set; }
        public string MusteriAdres { get; set; }
        public GeoCoordinate nokta { get; set; }
        

        public Bilgi(string _adres, GeoCoordinate _nokta)
        {
            MusteriAdres = _adres;
            nokta = _nokta;
        }

        public Bilgi()
        {

        }
    }
}
