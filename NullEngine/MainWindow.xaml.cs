using NullEngine.Rendering;
using NullEngine.Rendering.DataStructures;
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

namespace NullEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Renderer renderer;

        public MainWindow()
        {
            InitializeComponent();

            Closed += MainWindow_Closed; //stop renderer

            InitRenderer(); //generate a new renderer and start the renderer
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            renderer.Stop();
        }

        private void InitRenderer()
        {
            // 0. generate renderer
            renderer = new Renderer(renderFrame, 10, true);

            // 1.change camera and setup thread
            Camera camera1 = new Camera(new Vec3(0, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0), 36, 10, 180, new Vec3(0, 0, 0));
            renderer.CameraUpdateAndRender(camera1);

            // 2.start rendering
            renderer.Start();



            ////다 계산하고, 디스플레이가 되네;;;;

            // 3.change camera and setup thread
            Camera camera2 = new Camera(new Vec3(0, 0, 100), new Vec3(-1, 0, -1), new Vec3(0, 1, 0), 36, 10, 180, new Vec3(0, 0, 0));
            renderer.CameraUpdateAndRender(camera2);

            // 4.start rendering
            renderer.Start();

        }
    }
}
