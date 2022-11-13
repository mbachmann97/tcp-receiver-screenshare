using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TcpReceiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private TcpListener server = new TcpListener(IPAddress.Any, 9724);

        public MainWindow()
        {
            server.Start();
            InitializeComponent();

            Thread t = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Trace.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also use server.AcceptSocket() here.
                        TcpClient client = server.AcceptTcpClient();
                        Trace.WriteLine("Connected!");


                        if (client.Connected)
                        {
                            IPEndPoint ipe = (IPEndPoint) client.Client.RemoteEndPoint;
                            connectionInfo.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                connectionInfo.Content = ipe.ToString().Split(':')[0];
                            }));
                            using (NetworkStream stream = client.GetStream())
                            {
                                byte[] buffer = new byte[1024];
                                using (MemoryStream mss = new MemoryStream())
                                {

                                    int numBytesRead;
                                    while ((numBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        mss.Write(buffer, 0, numBytesRead);


                                    }
                                    Bitmap bitmap = new Bitmap(mss);
                                    IntPtr hBitmap = bitmap.GetHbitmap();

                                    BitmapSource source;
                                    try
                                    {
                                        source = Imaging.CreateBitmapSourceFromHBitmap(
                                        hBitmap,
                                        IntPtr.Zero,
                                        Int32Rect.Empty,
                                        BitmapSizeOptions.FromEmptyOptions());
                                    }
                                    finally
                                    {
                                        DeleteObject(hBitmap);
                                    }
                                    source.Freeze();
                                    receivedImage.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        receivedImage.Source = source;
                                    }));
                                }
                            }
                        }

                        // Shutdown and end connection
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.ToString());
                    }
                }

            });
            t.Start();
        }
    }
}
