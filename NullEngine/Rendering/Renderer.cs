﻿using NullEngine.Utils;
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
        public int width;
        public int height;
        
        private bool run = true;
        private int targetFramerate;
        private double frameTime;

        private ByteFrameBuffer deviceFrameBuffer;
        private FloatFrameBuffer deviceFrameDistanceBuffer;

        //where data is stored in cpu
        private byte[] frameBuffer = new byte[0];
        private byte[] materialIDBuffer = new byte[0];
        private float[] frameDistanceBuffer = new float[0];

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
            this.scene = new Scene(gpu, "../../../Assets/Viewbackground/Scene.json");
            camera = new Camera(new Vec3(0, 0, 10), new Vec3(0, 0, 0), new Vec3(0, -1, 0), 1500, 1000, 40, new Vec3(0, 0, 0));
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

                    byte[] materials = materialIDBuffer;
                    float[] distances = frameDistanceBuffer;
                }

                frameTime = frameTimer.endUpdateForTargetUpdateTime(1000.0 / targetFramerate, true);
                renderFrame.frameTime = frameTime;
            }

            if (deviceFrameBuffer != null)
            {
                deviceFrameBuffer.Dispose();
                deviceFrameDistanceBuffer.Dispose();

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

                        frameData.Dispose();
                    }

                    frameBuffer = new byte[width * height * 3];
                    materialIDBuffer = new byte[width * height];
                    frameDistanceBuffer = new float[width * height];

                    deviceFrameBuffer = new ByteFrameBuffer(gpu, height, width);
                    deviceFrameDistanceBuffer = new FloatFrameBuffer(gpu, height, width);

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
                gpu.Render(camera, scene, deviceFrameBuffer.frameBuffer, deviceFrameDistanceBuffer.frameDistanceBuffer, frameData.deviceFrameData);
                deviceFrameBuffer.memoryBuffer.CopyToCPU(frameBuffer);
                deviceFrameBuffer.memoryMaterialIDBuffer.CopyToCPU(materialIDBuffer);

                deviceFrameDistanceBuffer.memoryDistanceBuffer.CopyToCPU(frameDistanceBuffer);


                //cpu side everything is stored in frameBuffer
            }
        }

        private void Draw()
        {
            renderFrame.update(ref frameBuffer);
            renderFrame.updateMaterialID(ref materialIDBuffer);
            //renderFrame.updateDistance(ref frameDistanceBuffer);
            renderFrame.frameRate = frameTimer.lastFrameTimeMS;
        }
    }
}
