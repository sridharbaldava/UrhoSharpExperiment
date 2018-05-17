//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using SAP2000v18;
using System;
using System.Collections.Generic;
using Urho.Actions;
using Urho.Shapes;

namespace Urho.Samples
{
	public class StaticScene : Sample
	{
		Camera camera;
		Scene scene;
        Node RootNode;
        private Octree octree;
        Material selectedMaterial;
        Material material;

        public StaticScene(ApplicationOptions options = null) : base(SetOptions(options)) { }

        private static ApplicationOptions SetOptions(ApplicationOptions options)
        {
            options.TouchEmulation = true;
            return options;
        }
        protected override void Start ()
		{
			base.Start ();
			CreateScene ();
			SimpleCreateInstructionsWithWasd ();
			SetupViewport ();
            Input.TouchEnd += OnTouched;
		}

		void CreateScene ()
		{
			var cache = ResourceCache;
			scene = new Scene (Context);

			// Create the Octree component to the scene. This is required before adding any drawable components, or else nothing will
			// show up. The default octree volume will be from (-1000, -1000, -1000) to (1000, 1000, 1000) in world coordinates; it
			// is also legal to place objects outside the volume but their visibility can then not be checked in a hierarchically
			// optimizing manner
			octree = scene.CreateComponent<Octree> ();
            scene.CreateComponent<DebugRenderer>();

            // Create a child scene node (at world origin) and a StaticModel component into it. Set the StaticModel to show a simple
            // plane mesh with a "stone" material. Note that naming the scene nodes is optional. Scale the scene node larger
            // (100 x 100 world units)
            var planeNode = scene.CreateChild("Plane");
			planeNode.Scale = new Vector3 (10, 1, 10);
			var planeObject = planeNode.CreateComponent<StaticModel> ();
			planeObject.Model = cache.GetModel ("Models/Plane.mdl");
			planeObject.SetMaterial (cache.GetMaterial ("Materials/StoneTiled.xml"));

			// Create a directional light to the world so that we can see something. The light scene node's orientation controls the
			// light direction; we will use the SetDirection() function which calculates the orientation from a forward direction vector.
			// The light will use default settings (white light, no shadows)
			var lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection (new Vector3(0.6f, -1.0f, 0.8f)); // The direction vector does not need to be normalized
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;

			//var rand = new Random();
			//for (int i = 0; i < 200; i++)
			//{
			//	var mushroom = scene.CreateChild ("Mushroom");
			//	mushroom.Position = new Vector3 (rand.Next (90)-45, 0, rand.Next (90)-45);
			//	mushroom.Rotation = new Quaternion (0, rand.Next (360), 0);
			//	mushroom.SetScale (0.5f+rand.Next (20000)/10000.0f);
			//	var mushroomObject = mushroom.CreateComponent<StaticModel>();
			//	mushroomObject.Model = cache.GetModel ("Models/Mushroom.mdl");
			//	mushroomObject.SetMaterial (cache.GetMaterial ("Materials/Mushroom.xml"));
			//}

            List<VertexBuffer.PositionNormal> lineVertices = new List<VertexBuffer.PositionNormal>();
            cOAPI sapObject = null;
            cSapModel sapModel = null;

            try
            {
                var helper = new Helper();
                sapObject = helper.GetObject("CSI.SAP2000.API.SapObject");
                //sapObject = (cOAPI)System.Runtime.InteropServices.Marshal.GetActiveObject("CSI.SAP2000.API.SapObject");
                sapModel = sapObject.SapModel;
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            int numberFrames = 0;
            string[] frameNames = null;
            int ret = sapModel.FrameObj.GetNameList(ref numberFrames, ref frameNames);

            material = Material.FromColor(Color.Blue);
            material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlit);
            material.LineAntiAlias = true;

            selectedMaterial = Material.FromColor(Color.Yellow);
            selectedMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlit);
            selectedMaterial.LineAntiAlias = true;

            Node linesNode = scene.CreateChild("linesNode");

            foreach (var frameName in frameNames)
            {
                string Point1 = null;
                string Point2 = null;
                ret = sapModel.FrameObj.GetPoints(frameName, ref Point1, ref Point2);
                double X1 = 0;
                double Y1 = 0;
                double Z1 = 0;
                ret = sapModel.PointObj.GetCoordCartesian(Point1, ref X1, ref Y1, ref Z1);
                lineVertices.Add(new VertexBuffer.PositionNormal { Position = new Vector3((float)X1, (float)Z1, (float)Y1) });
                double X2 = 0;
                double Y2 = 0;
                double Z2 = 0;
                ret = sapModel.PointObj.GetCoordCartesian(Point2, ref X2, ref Y2, ref Z2);
                lineVertices.Add(new VertexBuffer.PositionNormal { Position = new Vector3((float)X2, (float)Z2, (float)Y2) });
                var start = new Vector3((float)X1, (float)Z1, (float)Y1);
                var end = new Vector3((float)X2, (float)Z2, (float)Y2);
                var frame = new Frame(start, end, material);
                var frameNode = linesNode.CreateChild("Frame Node");
                frameNode.Position = start + (end - start) * 0.5f;
                frameNode.AddComponent(frame);
            }

            VertexBuffer lineBuffer = new VertexBuffer(CurrentContext, false);
            lineBuffer.SetSize((uint)lineVertices.Count, ElementMask.Position | ElementMask.Normal, false);
            lineBuffer.SetData(lineVertices.ToArray());

            Geometry lineGeometry = new Geometry();
            lineGeometry.SetVertexBuffer(0, lineBuffer);
            lineGeometry.SetDrawRange(PrimitiveType.LineList, 0, 0, 0, (uint)lineVertices.Count, true);

            Model lineModel = new Model();
            lineModel.NumGeometries = 1;
            lineModel.SetGeometry(0, 0, lineGeometry);
            lineModel.BoundingBox = new BoundingBox(new Vector3(-10000, -10000, 1000), new Vector3(10000, 10000, 1000));

            linesNode.SetScale(0.08333f);
            //StaticModel lines = linesNode.CreateComponent<StaticModel>();
            //lines.Model = lineModel;

            RootNode = linesNode;
            
            //lines.SetMaterial(material);

            CameraNode = scene.CreateChild ("camera");
			camera = CameraNode.CreateComponent<Camera>();
			CameraNode.Position = new Vector3 (0, 25, 0);
		}
		
		void SetupViewport ()
		{
			var renderer = Renderer;
			renderer.SetViewport (0, new Viewport (Context, scene, camera, null));
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
            const float mouseSensitivity = .1f;
            const float moveSpeed = 10f;

            if (UI.FocusElement != null)
                return;



            if (Input.GetMouseButtonDown(MouseButton.Right))
            {
                var mouseMove = Input.MouseMove;
                Yaw = mouseSensitivity * mouseMove.X;
                Pitch = mouseSensitivity * mouseMove.Y;
                //Pitch = MathHelper.Clamp(Pitch, -90, 90);
                RootNode.Rotate(new Quaternion(0, Yaw, 0), TransformSpace.World);
            }

            
            if (Input.GetKeyDown(Key.W)) CameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.D)) CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);
        }

        Frame selectedFrame = null;
        private void OnTouched(TouchEndEventArgs e)
        {
            Ray cameraRay = camera.GetScreenRay((float)e.X / Graphics.Width, (float)e.Y / Graphics.Height);
            var result = octree.RaycastSingle(cameraRay);
            if (result != null)
            {
                var frame = result.Value.Node?.Parent?.GetComponent<Frame>();
                if (selectedFrame != frame)
                {
                    selectedFrame?.Deselect();
                    selectedFrame = frame;
                    selectedFrame?.Select();
                }
            }
            else
            {
                selectedFrame?.Deselect();
                selectedFrame = null;
            }
        }
    }

    class Frame : Component
    {
        Node CylinderNode;
        Material Material;
        Vector3 axis;
        Vector3 Start;
        Vector3 End;

        public override void OnAttachedToNode(Node node)
        {
            CylinderNode = node.CreateChild();
            var cylinder = CylinderNode.CreateComponent<Cylinder>();
            CylinderNode.Position = Vector3.Zero;
            CylinderNode.Scale = new Vector3(5, axis.Length, 5);
            CylinderNode.Rotation = Quaternion.FromRotationTo(Vector3.UnitY, axis);
            cylinder.Material = Material;
            base.OnAttachedToNode(node);
        }

        public Frame(Vector3 Start, Vector3 End, Material Material)
        {
            this.Start = Start;
            this.End = End;
            axis = End - Start;
            this.Material = Material;
            ReceiveSceneUpdates = true;
        }

        public void Deselect()
        {
            CylinderNode.RemoveAllActions();//TODO: remove only "selection" action
            CylinderNode.RunActionsAsync(new EaseBackOut(new TintTo(1f, Color.Blue)));
        }

        public void Select()
        {
            // "blinking" animation
            CylinderNode.RunActionsAsync(new RepeatForever(new TintTo(0.3f, 1f, 1f, 1f), new TintTo(0.3f, Color.Yellow)));
        }
    }
}