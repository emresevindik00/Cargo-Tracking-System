using GeoCoordinatePortable;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using names;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HaritaOrnek
{
    public partial class Form1 : Form
    {
        
        public List<PointLatLng> _points = new List<PointLatLng>();
        public double[,] _mesafeler;
        public List<String> _adresler = new List<String>();
        private List<Bilgi> _bilgiler;
        public List<PointLatLng> _tempPoints = new List<PointLatLng>();

        public Form1()
        {
            InitializeComponent();
            _bilgiler = new List<Bilgi>();
            
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            GMapProviders.GoogleMap.ApiKey = @"AIzaSyCYqO_PdB2EitIbyOhYZAW27e2SSjyxyqA";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            map.CacheLocation = @"cache";
            map.ShowCenter = false;
            map.DragButton = MouseButtons.Left;
            map.MapProvider = GMapProviders.GoogleMap;

            map.MinZoom = 5;
            map.MaxZoom = 100;
            map.Zoom = 12;
        }
        
    }
}
