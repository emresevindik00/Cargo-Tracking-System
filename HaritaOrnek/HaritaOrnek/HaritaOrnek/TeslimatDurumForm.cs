using GeoCoordinatePortable;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using names;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp.Config;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Http.Json;

namespace HaritaOrnek
{
    public partial class TeslimatDurumForm : Form
    {
        string silinecekKargoID;
        string silinecekMusteriLat, silinecekMusteriLng;
        int kaldirilanNokta;
        Form1 form1 = new Form1();
        private ShortestPath _shortestPath;
        private List<Bilgi> _bilgiler;
        GDirections myDirection;
        GMapOverlay ca = new GMapOverlay("overlay");
        List<double> tmp;


        public TeslimatDurumForm()
        {
            InitializeComponent();
            _bilgiler = new List<Bilgi>();
        }

        private void TeslimatDurumForm_Load(object sender, EventArgs e)
        {
           
            form1.Visible = false;
            GMapProviders.GoogleMap.ApiKey = @"AIzaSyCYqO_PdB2EitIbyOhYZAW27e2SSjyxyqA";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            menuHarita.CacheLocation = @"cache";
            menuHarita.ShowCenter = false;
            menuHarita.DragButton = MouseButtons.Left;
            menuHarita.MapProvider = GMapProviders.GoogleMap;

            menuHarita.MinZoom = 5;
            menuHarita.MaxZoom = 100;
            menuHarita.Zoom = 12;

        }

        void musteriGetir()
        {
            

            HttpClient client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback =
             delegate (object s, X509Certificate certificate,
                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
             { return true; };

            client.BaseAddress = new Uri("https://localhost:44389/");
            HttpResponseMessage response = client.GetAsync("api/kargo").Result;
            var emp = response.Content.ReadFromJsonAsync<IEnumerable<Bilgi>>().Result;
            dataGridView1.DataSource = emp;
        }

        private void LoadMap(PointLatLng point)
        {    
            form1.map.Position = point;
            Console.WriteLine(form1.map.Position);
        }

        GMapOverlay markers = new GMapOverlay("markers");

        private void AddMarker(PointLatLng pointToAdd, GMarkerGoogleType markerType = GMarkerGoogleType.red_dot)
        {
            var marker = new GMarkerGoogle(pointToAdd, markerType);
            form1.map.Overlays.Add(markers);
            markers.Markers.Add(marker);
        }


        private List<String> GetAddress(PointLatLng point)
        {
            List<Placemark> placemarks = null;
            var statusCode = GMapProviders.GoogleMap.GetPlacemarks(point, out placemarks);
            if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS && placemarks != null)
            {
                List<String> addresses = new List<string>();
                foreach (var placemark in placemarks)
                {
                    addresses.Add(placemark.Address);
                }
                return addresses;
            }
            return null;

        }


        private async void yeniTeslimBtn_ClickAsync(object sender, EventArgs e)
        {
            ca.Routes.Clear();
            

            if (!addressTxt.Text.Trim().Equals(""))
                {

                    GeoCoderStatusCode statusCode;
                    var pointLatLng = GoogleMapProvider.Instance.GetPoint(addressTxt.Text.Trim(), out statusCode);
                    if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                    {
                        textBox1.Text = pointLatLng?.Lat.ToString();
                        textBox2.Text = pointLatLng?.Lng.ToString();

                        var point = new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));
                        LoadMap(point);
                        AddMarker(point);

                             
                        var placeMark = GoogleMapProvider.Instance.GetPlacemark(point, out statusCode);

                        addressTxt.Text = placeMark?.Address;
                    
                        form1.Show();
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong " + statusCode);
                        Console.WriteLine("Nokta: " + form1._points);
                    }
                }
                else
                {

                    MessageBox.Show("Invalid Data to Load");
                }
           


            form1._points.Add(new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text)));


            var nokta = new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));
            //lokasyonTxt.Text = nokta.ToString();

            //konumun adresi
            var addresses = GetAddress(new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text)));
            


            //Müşterileri ekle
            _bilgiler.Add
                (new Bilgi(addresses.First(), new GeoCoordinate(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text))));


            var array = new GeoCoordinate[form1._points.Count];
            form1._mesafeler = new double[form1._points.Count, form1._points.Count];

            //listeyi array yap
            Bilgi[] bilgis = _bilgiler.ConvertAll<Bilgi>(i => (Bilgi)i).ToArray();


            //arraye koordinatları ekle
            for (int i = 0; i < form1._points.Count; i++)
            {
                array[i] = new GeoCoordinate(form1._points[i].Lat, form1._points[i].Lng);
            }



            //tüm noktalar arasındaki mesafeyi hesapla
            for (int i = 0; i < form1._points.Count; i++)
            {
                for (int j = i + 1; j < form1._points.Count; j++)
                {
                    double u = bilgis[i].nokta.GetDistanceTo(bilgis[j].nokta) / 1000;

                    form1._mesafeler[i, j] = u;
                    form1._mesafeler[j, i] = u;

                }

            }


            for (int i = 0; i < form1._mesafeler.GetLength(0); i++)
            {
                for (int j = 0; j < form1._mesafeler.GetLength(1); j++)
                {
                    Console.WriteLine(bilgis[i].MusteriAdres + " ile " + bilgis[j].MusteriAdres + " arasındaki mesafee: " + form1._mesafeler[i, j]);
                }
            }


           
            _shortestPath = new ShortestPath();
            ShortestPath.N = form1._points.Count();
            _shortestPath.visited = new bool[form1._points.Count];
            _shortestPath.final_path = new double[form1._points.Count + 1];


            Bilgi[] bilgiss = _bilgiler.ConvertAll<Bilgi>(i => (Bilgi)i).ToArray();
            _shortestPath.TSP(form1._mesafeler);
            Console.WriteLine("Mesafe : " + _shortestPath.final_res + " km\n");
            Console.WriteLine("Yol : ");


            for (int i = 0; i <= form1._mesafeler.GetLength(0); i++)
            {
                //en kısa yol indeksleri
                Console.WriteLine(" " + _shortestPath.final_path[i]);
                //en kısa yol adresleri
                Console.WriteLine("Adres: " + bilgiss[(int)_shortestPath.final_path[i]].MusteriAdres);

        
            }

            
            Console.WriteLine("mesafeler.length bu: " + ShortestPath.N);
            Console.WriteLine("mesafeler: " + form1._mesafeler.GetLength(1));
          

            YolCiz(form1._points.First(), form1._points.Last(), form1._points);

           

            HttpClient client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback =
             delegate (object s, X509Certificate certificate,
                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
             { return true; };

            var bilgi = new Bilgi()
            {
                KargoID = Convert.ToInt32(textBox3.Text),
                MüsteriAdi = musteriAdiTxt.Text,
                MusteriLat = textBox1.Text,
                MusteriLng = textBox2.Text,
                MusteriAdres = addressTxt.Text
            };
            client.BaseAddress = new Uri("https://localhost:44389/");
            var response = await client.PostAsJsonAsync("api/kargo", bilgi);

            musteriGetir();


            RefreshMap();
        }


        private void YolCiz(PointLatLng start, PointLatLng end, List<PointLatLng> waypoints)
        {
            //GDirections myDirections;
            var rt = GMapProviders.GoogleMap.GetDirections(out myDirection, start, waypoints, end, false, false, false, false, false);

            GMapRoute route = new GMapRoute(myDirection.Route, "route");
            //GMapOverlay c = new GMapOverlay("overlay");
            
            ca.Routes.Add(new GMapRoute(route.Points, "tst"));

            form1.map.Overlays.Add(ca);
            
        }


        //sil
        private async void teslimBtn_Click(object sender, EventArgs e)
        {
            
            markers.Clear();
            ca.Routes.Clear();
            

            HttpClient client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback =
             delegate (object s, X509Certificate certificate,
                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
             { return true; };

            var silinecekBilgi = new Bilgi()
            {
                KargoID = Convert.ToInt32(silinecekKargoID) //datagriedviewdan aldıgın veriyi koy
            };
            client.BaseAddress = new Uri("https://localhost:44389/");
            var response = await client.PostAsJsonAsync("api/kargo/delete", silinecekBilgi);

            //sonradan eklenen
            

            if (form1._points.Count > 2)
            {

                int indeks = 0;
                var point = new PointLatLng(Convert.ToDouble(silinecekMusteriLat), Convert.ToDouble(silinecekMusteriLng));
                for (int i = 0; i < form1._points.Count; i++)
                {
                    if(form1._points[i] == point)
                    {
                        indeks = i;
                       
                    }
                }
                form1._points.RemoveAt(indeks);



                Console.WriteLine("indeks: " + indeks);
                tmp = new List<double>(_shortestPath.final_path);

                for (int i = 0; i < tmp.Count; i++)
                {
                    if(indeks == (int)_shortestPath.final_path[i])
                    {
                        kaldirilanNokta = (int)_shortestPath.final_path[i];
                        tmp.Remove((int)_shortestPath.final_path[i]);
                        
                    }
                    
                }

                _shortestPath.final_path = tmp.ToArray();

                for (int i = 0; i < _shortestPath.final_path.Length; i++)
                {
                    
                    if((int)_shortestPath.final_path[i] > kaldirilanNokta)
                    {
                        _shortestPath.final_path[i] = _shortestPath.final_path[i] - 1;
                    }
                }


                for (int i = 0; i < form1._points.Count(); i++)
                {
                    
                    var marker = new GMarkerGoogle(form1._points[i], GMarkerGoogleType.red_dot);
                    form1.map.Overlays.Add(markers);
                    markers.Markers.Add(marker);
                }


                for (int i = 0; i < form1._points.Count() - 1; i++)
                {

                    var routee = GoogleMapProvider.Instance.GetRoutePoints
                        (form1._points[(int)_shortestPath.final_path[i]],
                        form1._points[(int)_shortestPath.final_path[i + 1]],
                        false, false, 15, "hey");
                    
                        ca.Routes.Add(new GMapRoute(routee.Points, "My Route"));
                    
                }
                

            }

      
            form1.map.Overlays.Add(ca);
            RefreshMap();


            MessageBox.Show("Teslimat Yapıldı!");

        }

        public void RefreshMap()
        {
            form1.map.Zoom--;
            form1.map.Zoom++;
        }


        private async void yeniTeslimBtn_Click_1(object sender, EventArgs e)
        {
            ca.Routes.Clear();


            if (!addressTxt.Text.Trim().Equals(""))
            {

                GeoCoderStatusCode statusCode;
                var pointLatLng = GoogleMapProvider.Instance.GetPoint(addressTxt.Text.Trim(), out statusCode);
                if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    textBox1.Text = pointLatLng?.Lat.ToString();
                    textBox2.Text = pointLatLng?.Lng.ToString();

                    var point = new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));
                    LoadMap(point);
                    AddMarker(point);


                    var placeMark = GoogleMapProvider.Instance.GetPlacemark(point, out statusCode);

                    addressTxt.Text = placeMark?.Address;

                    form1.Show();
                }
                else
                {
                    MessageBox.Show("Something went wrong " + statusCode);
                    Console.WriteLine("Nokta: " + form1._points);
                }
            }
            else
            {

                MessageBox.Show("Invalid Data to Load");
            }



            form1._points.Add(new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text)));


           

            //konumun adresi
            var addresses = GetAddress(new PointLatLng(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text)));



            //Müşterileri ekle
            _bilgiler.Add
                (new Bilgi(addresses.First(), new GeoCoordinate(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text))));


            var array = new GeoCoordinate[form1._points.Count];
            form1._mesafeler = new double[form1._points.Count, form1._points.Count];

            //listeyi array yap
            Bilgi[] bilgis = _bilgiler.ConvertAll<Bilgi>(i => (Bilgi)i).ToArray();


            //arraye koordinatları ekle
            for (int i = 0; i < form1._points.Count; i++)
            {
                array[i] = new GeoCoordinate(form1._points[i].Lat, form1._points[i].Lng);
            }



            //tüm noktalar arasındaki mesafeyi hesapla
            for (int i = 0; i < form1._points.Count; i++)
            {
                for (int j = i + 1; j < form1._points.Count; j++)
                {
                    double u = bilgis[i].nokta.GetDistanceTo(bilgis[j].nokta) / 1000;

                    form1._mesafeler[i, j] = u;
                    form1._mesafeler[j, i] = u;

                }

            }


            for (int i = 0; i < form1._mesafeler.GetLength(0); i++)
            {
                for (int j = 0; j < form1._mesafeler.GetLength(1); j++)
                {
                    Console.WriteLine(bilgis[i].MusteriAdres + " ile " + bilgis[j].MusteriAdres + " arasındaki mesafee: " + form1._mesafeler[i, j]);
                }
            }



            _shortestPath = new ShortestPath();
            ShortestPath.N = form1._points.Count();
            _shortestPath.visited = new bool[form1._points.Count];
            _shortestPath.final_path = new double[form1._points.Count + 1];


            Bilgi[] bilgiss = _bilgiler.ConvertAll<Bilgi>(i => (Bilgi)i).ToArray();
            _shortestPath.TSP(form1._mesafeler);
            Console.WriteLine("Mesafe : " + _shortestPath.final_res + " km\n");
            Console.WriteLine("Yol : ");


            for (int i = 0; i <= form1._mesafeler.GetLength(0); i++)
            {
                //en kısa yol indeksleri
                Console.WriteLine(" " + _shortestPath.final_path[i]);
                //en kısa yol adresleri
                Console.WriteLine("Adres: " + bilgiss[(int)_shortestPath.final_path[i]].MusteriAdres);


            }

            

            

            HttpClient client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback =
             delegate (object s, X509Certificate certificate,
                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
             { return true; };

            var bilgi = new Bilgi()
            {
                KargoID = Convert.ToInt32(textBox3.Text),
                MüsteriAdi = musteriAdiTxt.Text,
                MusteriLat = textBox1.Text,
                MusteriLng = textBox2.Text,
                MusteriAdres = addressTxt.Text
            };
            client.BaseAddress = new Uri("https://localhost:44389/");
            var response = await client.PostAsJsonAsync("api/kargo", bilgi);
            
            //tabloda göster
            HttpResponseMessage response2 = client.GetAsync("api/kargo").Result;
            var emp = response2.Content.ReadFromJsonAsync<IEnumerable<Bilgi>>().Result;
            dataGridView1.DataSource = emp;

            MessageBox.Show("Yeni Teslimat Eklendi!");


            if (form1._points.Count == 2)
            {
                var routee = GoogleMapProvider.Instance.GetRoutePoints
                        (form1._points[0],
                        form1._points[1],
                        false, false, 15, "hey");

                ca.Routes.Add(new GMapRoute(routee.Points, "My Route"));
            }

            if (form1._points.Count > 2)
            {
                for (int i = 0; i < form1._points.Count() - 1; i++)
                {
                    var routee = GoogleMapProvider.Instance.GetRoutePoints
                        (form1._points[(int)_shortestPath.final_path[i]], 
                        form1._points[(int)_shortestPath.final_path[i + 1]], 
                        false, false, 15, "hey");

                    ca.Routes.Add(new GMapRoute(routee.Points, "My Route"));

                }

            }

            form1.map.Overlays.Add(ca);
            RefreshMap();
        }

        private void menuHarita_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                var point = menuHarita.FromLocalToLatLng(e.X, e.Y);
                double lat = point.Lat;
                double lng = point.Lng;

                textBox1.Text = lat.ToString();
                textBox2.Text = lng.ToString();

                menuHarita.Position = point;

                var markers = new GMapOverlay("markers");
                var marker = new GMarkerGoogle(point, GMarkerGoogleType.red_dot);
                markers.Markers.Add(marker);
                menuHarita.Overlays.Add(markers);

                menuHarita.Zoom++;
                menuHarita.Zoom--;

                var addresses = GetAddress(point);
                addressTxt.Text = addresses[0];
            }
        }

       

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            silinecekKargoID = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            silinecekMusteriLat = dataGridView1.CurrentRow.Cells[3].Value.ToString();
            silinecekMusteriLng = dataGridView1.CurrentRow.Cells[4].Value.ToString();
            
        }
    }
}
