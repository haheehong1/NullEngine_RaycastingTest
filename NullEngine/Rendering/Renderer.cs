using NullEngine.Utils;
using NullEngine.Rendering.DataStructures;
using System.Threading;
using ILGPU.Runtime;
using NullEngine.Rendering.Implementation;
using System.Windows;
using System;
using ILGPU.Algorithms;
using System.Collections.Generic;

namespace NullEngine.Rendering
{
    public class Renderer
    {
        public int width = 30;
        public int height = 20;
        
        private bool run = true;
        private int targetFramerate;
        private double frameTime;

        private ByteFrameBuffer deviceFrameBuffer;
        private FloatFrameBuffer deviceFrameDistanceBuffer;
        private ByteFrameBuffer deviceFrameDistance2Buffer;

        //where data is stored in cpu
        private byte[] frameBuffer = new byte[0];
        private byte[] frameMaterialID2Buffer = new byte[0];
        private byte[] frameMaterialIDBuffer = new byte[0];
        private float[] frameDistanceBuffer = new float[0];
        private byte[] frameDistance2Buffer = new byte[0];

        private GPU gpu;
        private Camera camera;
        private Scene scene;
        private FrameData frameData;
        private UI.RenderFrame renderFrame;
        private Thread renderThread;
        private FrameTimer frameTimer;

        public Renderer(UI.RenderFrame renderFrame, int targetFramerate, bool forceCPU)
        {
            this.renderFrame = renderFrame;
            this.targetFramerate = targetFramerate;
            gpu = new GPU(forceCPU);
            //this.scene = new Scene(gpu, "../../../Assets/CubeTest/Scene.json");
            //this.scene = new Scene(gpu, "../../../Assets/Sponza/Scene.json");
            //this.scene = new Scene(gpu, "../../../Assets/Suzannes/Scene.json");
            //this.scene = new Scene(gpu, "../../../Assets/Viewbackground/Scene.json");
            this.scene = new Scene(gpu, "../../../Assets/MaterialTest/Scene.json");
            camera = new Camera(new Vec3(0, 0, 10), new Vec3(0, 0, 0), new Vec3(0, -1, 0), 0, 0, 40, new Vec3(0, 0, 0));
            frameTimer = new FrameTimer();

            renderFrame.onResolutionChanged = OnResChanged;

            renderThread = new Thread(RenderThread);
            renderThread.IsBackground = true;
        }

        public void Start()
        {
            renderThread.Start();
        }

        public void Stop()
        {
            run = false;
            renderThread.Join();
        }

        private void OnResChanged(int width, int height)
        {
            this.width = width;
            this.height = height;

            camera = new Camera(camera, this.width, this.height);
        }

        //eveything below this happens in the render thread
        private void RenderThread()
        {
            while (run)
            {
                frameTimer.startUpdate();

                if(ReadyFrameBuffer())
                {
                    RenderToFrameBuffer();
                    Application.Current.Dispatcher.InvokeAsync(Draw);

                    byte[] depth = frameBuffer;
                    byte[] materials = frameMaterialIDBuffer;
                    byte[] materials2 = frameMaterialID2Buffer;
                    float[] distances = frameDistanceBuffer;
                    byte[] distances2 = frameDistance2Buffer;
                }

                frameTime = frameTimer.endUpdateForTargetUpdateTime(1000.0 / targetFramerate, true);
                renderFrame.frameTime = frameTime;
            }

            if (deviceFrameBuffer != null)
            {
                deviceFrameBuffer.Dispose();
                deviceFrameDistanceBuffer.Dispose();
                deviceFrameDistance2Buffer.Dispose();

                frameData.Dispose();
            }
            gpu.Dispose();
        }

        private bool ReadyFrameBuffer()
        {
            if((width != 0 && height != 0))
            {
                if(deviceFrameBuffer == null || deviceFrameBuffer.frameBuffer.width != width || deviceFrameBuffer.frameBuffer.height != height)
                {
                    if (deviceFrameBuffer != null)
                    {
                        deviceFrameBuffer.Dispose();
                        deviceFrameDistanceBuffer.Dispose();
                        deviceFrameDistance2Buffer.Dispose();

                        frameData.Dispose();
                    }

                    frameBuffer = new byte[width * height * 3];
                    frameDistanceBuffer = new float[width * height];
                    frameDistance2Buffer = new byte[width * height * 3];
                    frameMaterialIDBuffer = new byte[width * height];
                    frameMaterialID2Buffer = new byte[width * height * 3];

                    deviceFrameBuffer = new ByteFrameBuffer(gpu, height, width);
                    deviceFrameDistanceBuffer = new FloatFrameBuffer(gpu, height, width);
                    deviceFrameDistance2Buffer = new ByteFrameBuffer(gpu, height, width);

                    frameData = new FrameData(gpu.device, width, height);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void RenderToFrameBuffer()
        {
            if (deviceFrameBuffer != null && !deviceFrameBuffer.isDisposed)
            {
                gpu.Render(camera, scene, deviceFrameBuffer.frameBuffer, deviceFrameDistanceBuffer.frameDistanceBuffer,/* deviceFrameBuffer.frameBuffer.frameMaterialID, deviceFrameBuffer.frameBuffer.frameMaterialID2,*/ frameData.deviceFrameData);  //should I update?? for distance2
                deviceFrameBuffer.memoryBuffer.CopyToCPU(frameBuffer);
                
                deviceFrameBuffer.memoryMaterialIDBuffer.CopyToCPU(frameMaterialIDBuffer);
                deviceFrameBuffer.memoryMaterialID2Buffer.CopyToCPU(frameMaterialID2Buffer);

                deviceFrameDistanceBuffer.memoryDistanceBuffer.CopyToCPU(frameDistanceBuffer);
                deviceFrameDistance2Buffer.memoryDistance2Buffer.CopyToCPU(frameDistance2Buffer);

                //cpu side everything is stored in frameBuffer
            }
        }

        private void Draw()
        {
            //choose which layer to display
            
            //renderFrame.update(ref frameBuffer);// depthMap
            renderFrame.update(ref frameMaterialID2Buffer);
            //renderFrame.updateMaterialID(ref materialIDBuffer);
            //renderFrame.updateDistance(ref frameDistanceBuffer);
            //renderFrame.update(ref frameDistance2Buffer);  //dont need updateMaterialID, updateDistance2
            renderFrame.frameRate = frameTimer.lastFrameTimeMS;
        }
    }
}
