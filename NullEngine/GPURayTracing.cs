//using System;
//using System.Collections.Generic;
//using NullEngine.Rendering;
//using NullEngine.Rendering.DataStructures;
//using NullEngine.UI;
//using System.Windows;
//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace NullEngine
//{
//    public class GPURayTracing : GH_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GPURayTracing class.
//        /// </summary>
//        public GPURayTracing()
//          : base("GPURayTracing", "Nickname",
//              "Description",
//              "Category", "Subcategory")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            Boolean run = false;

//            if (!DA.GetData(0, ref run)) return;
//            if (run == false) return;


//            Renderer renderer = new Renderer(false);

//            // 1.change camera and setup thread
//            Camera camera1 = new Camera(new Vec3(0, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0), 36, 10, 180, new Vec3(0, 0, 0));
//            renderer.CameraUpdateAndRender(camera1);

//            // 2.start rendering
//            renderer.Start();



//            ////다 계산하고, 디스플레이가 되네;;;;

//            // 3.change camera and setup thread
//            Camera camera2 = new Camera(new Vec3(0, 0, 100), new Vec3(-1, 0, -1), new Vec3(0, 1, 0), 36, 10, 180, new Vec3(0, 0, 0));
//            renderer.CameraUpdateAndRender(camera2);

//            // 4.start rendering
//            renderer.Start();

//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                //You can add image files to your project resources and access them like this:
//                // return Resources.IconForThisComponent;
//                return null;
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("B87D563F-44E0-4F57-8880-B54F8E767662"); }
//        }
//    }
//}