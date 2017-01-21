using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HDFaceSample {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        IFaceSensor sensor = new KinectFaceSensor();
        List<Ellipse> vertices = new List<Ellipse>();

        public MainWindow() {
            InitializeComponent();
            sensor.FacePointUpdated += Sensor_FacePointUpdated;
        }

        private void Sensor_FacePointUpdated(object sender, FacePointEventArgs e) {
            if (vertices.Count == 0) {
                for (int i = 0; i < e.DepthSpacePoints.Count; ++i) {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Width = 2.0;
                    ellipse.Height = 2.0;
                    ellipse.Fill = new SolidColorBrush(Colors.Blue);
                    vertices.Add(ellipse);
                }
                foreach (Ellipse ellipse in vertices) {
                    canvas.Children.Add(ellipse);
                }
            }

            for (int i = 0; i < vertices.Count; ++i) {
                Point2 p = e.DepthSpacePoints[i];
                if(float.IsInfinity(p.X) || float.IsInfinity(p.Y)) {
                    return;
                }

                Ellipse ellipse = vertices[i];
                Canvas.SetLeft(ellipse, p.X * 1.2);
                Canvas.SetTop(ellipse, p.Y * 1.2);
            }        
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            sensor.Initialize();
            sensor.Open();          
        }
    }
}
