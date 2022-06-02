//using NullEngine.Utils;
//using NullEngine.Rendering.DataStructures;
//using System.Threading;
//using ILGPU.Runtime;
//using NullEngine.Rendering.Implementation;
//using System.Windows;
//using System;
//using System.Drawing;
//using System.IO;
//using ILGPU.Algorithms;
//using System.Collections.Generic;

//namespace NullEngine.Rendering
//{
//    public class Renderer
//    {
//        public int width = 36;
//        public int height = 10;
        
//        private bool run = true;

//        private ByteFrameBuffer deviceFrameBuffer;
//        private FloatFrameBuffer deviceFrameDistanceBuffer;
//        private ByteFrameBuffer deviceFrameDistance2Buffer;

//        //where data is stored in cpu
//        private byte[] frameBuffer = new byte[0];
//        private byte[] frameMaterialID2Buffer = new byte[0];
//        private byte[] frameMaterialIDBuffer = new byte[0];
//        private float[] frameDistanceBuffer = new float[0];
//        private byte[] frameDistance2Buffer = new byte[0];

//        private GPU gpu;
//        private Camera camera;
//        private Scene scene;
//        private FrameData frameData;
//        private Thread renderThread;  // main component to run. renderer.start() -> renderThread.start()

//        public Renderer(bool forceCPU)
//        {
            
//            gpu = new GPU(forceCPU);
//            //this.scene = new Scene(gpu, "../../../Assets/CubeTest/Scene.json");
//            //this.scene = new Scene(gpu, "../../../Assets/Sponza/Scene.json");
//            //this.scene = new Scene(gpu, "../../../Assets/Suzannes/Scene.json");
//            //this.scene = new Scene(gpu, "../../../Assets/Viewbackground/Scene.json");
//            this.scene = new Scene(gpu, "../../../Assets/MaterialTest/Scene.json");

            
//        }

//        public void CameraUpdateAndRender(Camera cameraInput)
//        {
//            //Panoramic: verticalFov = 180
//            //camera origin and lookAt coordinates should be changed, (x, y, z) -> (x, -z, -y)
//            camera = cameraInput;

//            this.width = camera.width;
//            this.height = camera.height;

//            //main rendereing setup
//            renderThread = new Thread(RenderThread);
//            renderThread.IsBackground = true;

//        }

//        public void Start()
//        {
//            renderThread.Start();
//        }

//        public void Stop()
//        {
//            run = false;
//            renderThread.Join();
//        }

//        //eveything below this happens in the render thread
//        private void RenderThread()
//        {
//            while (run) 
//            {
                
//                if(ReadyFrameBuffer())  //dispose previous bufferdata and reset them for new resolution
//                {
//                    RenderToFrameBuffer();  //actual rendereing and copy the data to cpu
                    

//                    byte[] depth = frameBuffer;
//                    byte[] materials = frameMaterialIDBuffer;
//                    byte[] materials2 = frameMaterialID2Buffer;
//                    float[] distances = frameDistanceBuffer;
//                    byte[] distances2 = frameDistance2Buffer;


//                    //save rendering into bmp
//                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//                    string dir = (path + @"\NullEngine");

//                    if (!Directory.Exists(dir))
//                    {
//                        Directory.CreateDirectory(dir);
//                    }

//                    long time = DateTime.Now.ToFileTime();
//                    string filename1 = dir + @"\RaycastingResult" + time + ".bmp";

//                    var bitmap = new Bitmap(width, height);

//                    for (int x = 0; x < width; x++)
//                    {
//                        for (int y = 0; y < height; y++)
//                        {
//                            byte r = materials2[(width * y + x) * 3];
//                            byte g = materials2[(width * y + x) * 3 + 1];
//                            byte b = materials2[(width * y + x) * 3 + 2];
//                            var pixColor = Color.FromArgb(r, g, b);

//                            bitmap.SetPixel(width - x - 1, height - y - 1, pixColor);

//                        }
//                    }
//                    bitmap.Save(filename1);
//                }
//            }

//            if (deviceFrameBuffer != null)   //If there are data inside of gpu storage, remove
//            {
//                deviceFrameBuffer.Dispose();
//                deviceFrameDistanceBuffer.Dispose();
//                deviceFrameDistance2Buffer.Dispose();

//                frameData.Dispose();
//            }
//            gpu.Dispose();
//        }

//        private bool ReadyFrameBuffer()
//        {
//            if((width != 0 && height != 0))
//            {
//                if(deviceFrameBuffer == null || deviceFrameBuffer.frameBuffer.width != width || deviceFrameBuffer.frameBuffer.height != height)
//                {
//                    if (deviceFrameBuffer != null)
//                    {
//                        deviceFrameBuffer.Dispose();
//                        deviceFrameDistanceBuffer.Dispose();
//                        deviceFrameDistance2Buffer.Dispose();

//                        frameData.Dispose();
//                    }

//                    frameBuffer = new byte[width * height * 3];
//                    frameDistanceBuffer = new float[width * height];
//                    frameDistance2Buffer = new byte[width * height * 3];
//                    frameMaterialIDBuffer = new byte[width * height];
//                    frameMaterialID2Buffer = new byte[width * height * 3];

//                    deviceFrameBuffer = new ByteFrameBuffer(gpu, height, width);
//                    deviceFrameDistanceBuffer = new FloatFrameBuffer(gpu, height, width);
//                    deviceFrameDistance2Buffer = new ByteFrameBuffer(gpu, height, width);

//                    frameData = new FrameData(gpu.device, width, height);
//                }

//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        private void RenderToFrameBuffer()
//        {
//            if (deviceFrameBuffer != null && !deviceFrameBuffer.isDisposed)
//            {
//                gpu.Render(camera, scene, deviceFrameBuffer.frameBuffer, deviceFrameDistanceBuffer.frameDistanceBuffer,/* deviceFrameBuffer.frameBuffer.frameMaterialID, deviceFrameBuffer.frameBuffer.frameMaterialID2,*/ frameData.deviceFrameData);  //should I update?? for distance2
//                deviceFrameBuffer.memoryBuffer.CopyToCPU(frameBuffer);
                
//                deviceFrameBuffer.memoryMaterialIDBuffer.CopyToCPU(frameMaterialIDBuffer);
//                deviceFrameBuffer.memoryMaterialID2Buffer.CopyToCPU(frameMaterialID2Buffer);

//                deviceFrameDistanceBuffer.memoryDistanceBuffer.CopyToCPU(frameDistanceBuffer);
//                deviceFrameDistance2Buffer.memoryDistance2Buffer.CopyToCPU(frameDistance2Buffer);

//                //cpu side everything is stored in frameBuffer
//            }
//        }
//    }
//}
