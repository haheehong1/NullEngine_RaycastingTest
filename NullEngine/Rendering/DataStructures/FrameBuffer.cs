using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ILGPU;
using ILGPU.Runtime;
using NullEngine.Rendering.Implementation;

namespace NullEngine.Rendering.DataStructures
{
    public class FloatFrameBuffer
    {
        //public dFloatFrameBuffer frameBuffer;
        public dFloatFrameBuffer frameDistanceBuffer;
        //public MemoryBuffer1D<float, Stride1D.Dense> memoryBuffer;
        public MemoryBuffer1D<float, Stride1D.Dense> memoryDistanceBuffer;

        public FloatFrameBuffer(GPU gpu, int height, int width)
        {
            //memoryBuffer = gpu.device.Allocate1D<float>(height * width * 3);
            //frameBuffer = new dFloatFrameBuffer(height, width, memoryBuffer);

            memoryDistanceBuffer = gpu.device.Allocate1D<float>(height * width);
            frameDistanceBuffer = new dFloatFrameBuffer(height, width, memoryDistanceBuffer);

        }

        public void Dispose()
        {
            //memoryBuffer.Dispose();
            memoryDistanceBuffer.Dispose();
        }
    }

    public struct dFloatFrameBuffer
    {
        public int height;
        public int width;
        public ArrayView1D<float, Stride1D.Dense> frame;


        public dFloatFrameBuffer(int height, int width, MemoryBuffer1D<float, Stride1D.Dense> frame)
        {
            this.height = height;
            this.width = width;
            this.frame = frame.View;
        }

        public Vec3 readFrameBuffer(int x, int y)
        {
            int newIndex = ((y * width) + x) * 3;
            return readFrameBuffer(newIndex);
        }

        public Vec3 readFrameBuffer(int index)
        {
            return new Vec3(frame[index], frame[index + 1], frame[index + 2]);
        }

        public void writeFrameBuffer(int x, int y, float r, float g, float b)
        {
            int newIndex = ((y * width) + x) * 3;
            writeFrameBuffer(newIndex, r, b, g);
        }

        public void writeFrameBuffer(int index, float r, float g, float b)
        {
            frame[index] = r;
            frame[index + 1] = g;
            frame[index + 2] = b;
        }

        public void writeFrameDistanceBuffer(int x, int y, float r)
        {
            int newIndex = ((y * width) + x);
            writeFrameDistanceBuffer(newIndex, r);
        }

        public void writeFrameDistanceBuffer(int index, float r)
        {
            frame[index] = r;
        }
    }

    public class ByteFrameBuffer
    {
        public dByteFrameBuffer frameBuffer;
        //public dByteFrameBuffer frameMaterialID2Buffer;
        //public dByteFrameBuffer frameMaterialIDBuffer;
        public MemoryBuffer1D<byte, Stride1D.Dense> memoryBuffer;
        public MemoryBuffer1D<byte, Stride1D.Dense> memoryMaterialID2Buffer;
        public MemoryBuffer1D<byte, Stride1D.Dense> memoryMaterialIDBuffer;
        public MemoryBuffer1D<byte, Stride1D.Dense> memoryDistance2Buffer;
        public bool isDisposed = false;
        public bool inUse = false;

        public ByteFrameBuffer(GPU gpu, int height, int width)
        {
            memoryBuffer = gpu.device.Allocate1D<byte>(height * width * 3);
            memoryMaterialID2Buffer = gpu.device.Allocate1D<byte>(height * width * 3);
            memoryMaterialIDBuffer = gpu.device.Allocate1D<byte>(height * width);
            memoryDistance2Buffer = gpu.device.Allocate1D<byte>(height * width * 3);
            frameBuffer = new dByteFrameBuffer(height, width, memoryBuffer, memoryMaterialID2Buffer, memoryMaterialIDBuffer, memoryDistance2Buffer);
            
        }


        public void Dispose()
        {
            while(inUse)
            {
                Thread.Sleep(1);
            }
            isDisposed = true;
            memoryBuffer.Dispose();
            memoryMaterialID2Buffer.Dispose();
            memoryMaterialIDBuffer.Dispose();
            memoryDistance2Buffer.Dispose();
        }
    }


    public struct dByteFrameBuffer
    {
        public int height;
        public int width;
        public ArrayView1D<byte, Stride1D.Dense> frame;
        public ArrayView1D<byte, Stride1D.Dense> frameMaterialID;
        public ArrayView1D<byte, Stride1D.Dense> frameMaterialID2;
        public ArrayView1D<byte, Stride1D.Dense> frameDistance2;

        public dByteFrameBuffer(int height, int width, MemoryBuffer1D<byte, Stride1D.Dense> frame, MemoryBuffer1D<byte, Stride1D.Dense> frameMaterialID2, MemoryBuffer1D<byte, Stride1D.Dense> frameMaterialID, MemoryBuffer1D<byte, Stride1D.Dense> frameDistance2)
        {
            this.height = height;
            this.width = width;
            this.frame = frame.View;
            this.frameMaterialID = frameMaterialID.View;
            this.frameMaterialID2 = frameMaterialID2.View;
            this.frameDistance2 = frameDistance2.View;
        }

        /*
        public Vec3i readFrameBuffer(int x, int y)
        {
            int newIndex = ((y * width) + x) * 3;
            return readFrameBuffer(newIndex);
        }

        public Vec3i readFrameBuffer(int index)
        {
            return new Vec3i(frame[index], frame[index + 1], frame[index + 2]);
        }
        */

        public void writeFrameBuffer(int x, int y, float r, float g, float b)
        {
            int newIndex = ((y * width) + x) * 3;
            writeFrameBuffer(newIndex, r, b, g);
        }


        public void writeFrameBuffer(int x, int y, byte r, byte g, byte b)
        {
            int newIndex = ((y * width) + x) * 3;
            writeFrameBuffer(newIndex, r, b, g);
        }

        public void writeFrameBuffer(int index, byte r, byte g, byte b)
        {
            frame[index] = r;
            frame[index + 1] = g;
            frame[index + 2] = b;
        }

        public void writeFrameBuffer(int index, float r, float g, float b)
        {
            //original
            //frame[index] = (byte)(r * 255f);
            //frame[index + 1] = (byte)(g * 255f);
            //frame[index + 2] = (byte)(b * 255f);

            //apply color depthmap
            float distance = r;
            float remap = ColorGenerator.Remap(distance, 0.0f, 20.0f, 0.0f, 1.0f);
            byte[] pixColor = ColorGenerator.Turbo.ReturnTurboColor((float)remap);


            frame[index] = pixColor[0];
            frame[index + 1] = pixColor[1];
            frame[index + 2] = pixColor[2];


        }

        //Distance2

        public void writeFrameDistnace2Buffer(int index, byte r, byte g, byte b)
        {
            frameDistance2[index] = r;
            frameDistance2[index + 1] = g;
            frameDistance2[index + 2] = b;
        }

        public void writeFrameDistance2Buffer(int index, float r, float g, float b) //Coloring!!
        {
            frameDistance2[index] = (byte)(r * 255f);
            frameDistance2[index + 1] = (byte)(g * 255f);
            frameDistance2[index + 2] = (byte)(b * 255f);
        }

        //MaterialID2
        public void writeFrameMaterialID2Buffer(int x, int y, int r, int g, int b)
        {
            int newIndex = ((y * width) + x) * 3;
            writeFrameMaterialID2Buffer(newIndex, r, b, g);
        }

        public void writeFrameMaterialID2Buffer(int x, int y, byte r, byte g, byte b)
        {
            int newIndex = ((y * width) + x) * 3;
            writeFrameMaterialID2Buffer(newIndex, r, b, g);
        }

        public void writeFrameMaterialID2Buffer(int index, byte r, byte g, byte b)
        {
            frameMaterialID2[index] = r;
            frameMaterialID2[index + 1] = g;
            frameMaterialID2[index + 2] = b;
        }

        public void writeFrameMaterialID2Buffer(int index, int r, int g, int b)
        {
            frameMaterialID2[index] = (byte)(r);
            frameMaterialID2[index + 1] = (byte)(g);
            frameMaterialID2[index + 2] = (byte)(b);
        }


        //Material ID
        public void writeFrameMaterialIDBuffer(int index, byte r)
        {
            frameMaterialID[index] = r;
        }

        public void writeFrameMaterialIDBuffer(int index, float r)
        {
            frameMaterialID[index] = (byte)(r);
        }


    }
}
