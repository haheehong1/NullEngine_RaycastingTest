using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Generic;
using System.Text;
using NullEngine.Rendering.DataStructures;
using NullEngine.Rendering.DataStructures.BVH;

namespace NullEngine.Rendering.Implementation
{
    public class GPU
    {
        public Context context;
        public Accelerator device;
        public Action<Index1D, Camera, dFrameData> generatePrimaryRays;
        public Action<Index1D, dFrameData, dTLAS, dRenderData> hitRays;
        public Action<Index1D, dByteFrameBuffer, dFloatFrameBuffer, dFrameData> generateFrame;
        public GPU(bool forceCPU)
        {
            context = Context.Create(builder => builder.Cuda().CPU().EnableAlgorithms().Assertions());
            device = context.GetPreferredDevice(preferCPU: forceCPU)
                                      .CreateAccelerator(context);

            initRenderKernels();
        }

        private void initRenderKernels()
        {
            generateFrame = device.LoadAutoGroupedStreamKernel<Index1D, dByteFrameBuffer, dFloatFrameBuffer, dFrameData>(GPUKernels.GenerateFrame);
            hitRays = device.LoadAutoGroupedStreamKernel<Index1D, dFrameData, dTLAS, dRenderData>(GPUKernels.HitRays);
            generatePrimaryRays = device.LoadAutoGroupedStreamKernel<Index1D, Camera, dFrameData>(GPUKernels.GeneratePrimaryRays);
        }

        public void Dispose()
        {
            device.Dispose();
            context.Dispose();
        }

        public void Render(Camera camera, Scene scene, dByteFrameBuffer output, dFloatFrameBuffer output2, dFrameData frameData)
        {
            generatePrimaryRays(output.width * output.height, camera, frameData);
            hitRays(output.width * output.height, frameData, scene.tlas.GetDTLAS(), scene.tlas.renderDataManager.getDeviceRenderData());
            generateFrame(output.height * output.width, output, output2, frameData);
            device.Synchronize();
        }
    }

    public static class GPUKernels
    {
        public static void GeneratePrimaryRays(Index1D pixel, Camera camera, dFrameData frameData)
        {
            float x = (pixel % camera.width);
            float y = (pixel / camera.width);

            frameData.rayBuffer[pixel] = camera.GetRay(x, y);
        }

        //real raycasting first place of making hitrecord
        public static void HitRays(Index1D pixel, dFrameData frameData, dTLAS tlas, dRenderData renderData)
        {
            HitRecord hit = new HitRecord();
            hit.t = float.MaxValue;

            int materialIndex = 0;

            if(true)
            {
                for(int i = 0; i < tlas.meshes.Length; i++)
                {
                    dMesh mesh = tlas.meshes[i];
                    for(int j = 0; j < mesh.triangleLength; j++)
                    {
                        mesh.GetTriangle(j, renderData).GetTriangleHit(frameData.rayBuffer[pixel], j, ref hit);
                        if (hit.t < float.MaxValue)
                        {
                            hit.materialID = renderData.rawMaterialID2Buffers[materialIndex];
                        }
                        else
                        {
                            hit.materialID = 0;
                        }
                        materialIndex++;


                        //here is raytracing part, for one ray(frameData,rayBuffer[pixel], test all meshes and all triangles in a mesh
                        //save the record to hit and save it to **framedata.outputbuffer**
                    }
                }
            }
            else
            {
                tlas.hit(renderData, frameData.rayBuffer[pixel], 0.01f, ref hit);
            }

            if (hit.t < float.MaxValue)
            {
                frameData.outputBuffer[(pixel * 3)]     = hit.t;
                frameData.outputBuffer[(pixel * 3) + 1] = hit.t;
                frameData.outputBuffer[(pixel * 3) + 2] = hit.t;
                frameData.outputMaterialID2Buffer[(pixel * 3)] = hit.materialID; //hum.. let's see if this works
                frameData.outputMaterialID2Buffer[(pixel * 3) + 1] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3) + 2] = hit.materialID;
                frameData.outputMaterialIDBuffer[pixel] = hit.materialID;
                frameData.depthBuffer[pixel] = hit.t;

                frameData.outputDistance2Buffer[(pixel * 3)] = hit.t;
                frameData.outputDistance2Buffer[(pixel * 3) + 1] = hit.t;
                frameData.outputDistance2Buffer[(pixel * 3) + 2] = hit.t;
            }
        }

        public static void GenerateFrame(Index1D pixel, dByteFrameBuffer output, dFloatFrameBuffer output2, dFrameData frameData)
        {
            Vec3 color = UtilityKernels.readFrameBuffer(frameData.outputBuffer, pixel * 3);
            //color = Vec3.reinhard(color); can affect to distance measurement output so disabled
            output.writeFrameBuffer(pixel * 3, color.x, color.y, color.z);

            Vec3 materialID2 = UtilityKernels.readFrameMaterialID2Buffer(frameData.outputMaterialID2Buffer, pixel * 3); //problem
            //materialID2 = Vec3.reinhard(materialID2);
            switch(materialID2.x)
            {
                case 1:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 255, 0);
                    break;
                case 100:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 0, 255);
                    break;
                case 0:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 255, 255);
                    break;
                default:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 0, 0);
                    break;

            }
            //output.writeFrameMaterialID2Buffer(pixel * 3, (int)materialID2.x, (int)materialID2.y, (int)materialID2.z);

            //add frameData.outputMaterialIDBuffer
            int materialID = UtilityKernels.readFrameMaterialIDBuffer(frameData.outputMaterialIDBuffer, pixel);
            output.writeFrameMaterialIDBuffer(pixel, materialID);

            //add frameData.depthBuffer
            float distance = UtilityKernels.readFrameDistanceBuffer(frameData.depthBuffer, pixel);
            output2.writeFrameDistanceBuffer(pixel, distance);

            //Distance2
            Vec3 colorD = UtilityKernels.readFrameBuffer(frameData.outputDistance2Buffer, pixel * 3);
            colorD = Vec3.reinhard(colorD);
            output.writeFrameDistance2Buffer(pixel * 3, colorD.x, colorD.y, colorD.z);
        }

    }
}
