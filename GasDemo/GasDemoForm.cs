using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GasDemo
{
    public partial class GasDemoForm : GraphicsForm
    {
        private Config config = new Config("Config.txt");

        private Gas.Graphics.Font font = null;

        private bool useBloom = false;

        private Texture sceneImage = null;
        private BloomPostProcessor bloomProcessor = null;

        private SceneGraph sceneGraph = null;
        private LightNode light = null;
        private GeometryNode backgroundQuad = null;
        private GeometryNode waterMesh = null;
        private GeometryNode object1 = null;
        private GeometryNode object2 = null;
        private GeometryNode object3 = null;

        private float angle = 0.0f;
        private const float AngularVelocity = (2.0f * (float)Math.PI) / 5.0f;

        public Config Config
        {
            get { return config; }
        }

        protected override void InitializeGame()
        {
            this.Icon = new Icon(Application.StartupPath + @"\GasIcon.ico");
            Cursor.Hide();

            useBloom = config.GetSetting<bool>("UseBloom");

            renderer.ProjectionMode = ProjectionMode.Orthogonal;
            renderer.ViewMatrix = Matrix.LookAtLH(new Vector3(0, 0, 5.0f), new Vector3(),
                new Vector3(0, 1, 0));

            font = new Gas.Graphics.Font(renderer, "Arial", 16);
            font.ShadowColor = Color.Red;

            sceneImage = new Texture(renderer, renderer.FullscreenSize.Width, renderer.FullscreenSize.Height,
                true);
            bloomProcessor = new BloomPostProcessor(renderer);
            bloomProcessor.BloomScale = config.GetSetting<float>("BloomScale");
            bloomProcessor.Blur = config.GetSetting<float>("Blur");
            bloomProcessor.BrightPassThreshold = config.GetSetting<float>("BrightPassThreshold");

            sceneGraph = new SceneGraph(renderer);

            string lightColorName = config.GetSetting<string>("LightColor");
            Color lightColor = Color.FromName(lightColorName);
            light = new LightNode(renderer, sceneGraph, Matrix.Identity, 250.0f, 1.0f, lightColor);

            backgroundQuad = new GeometryNode(renderer, sceneGraph, Matrix.Translation(0.0f, 0.0f, -1.0f),
                Mesh.Rectangle(renderer, Color.Black, renderer.FullscreenSize.Width,
                renderer.FullscreenSize.Height, 2.0f), "roughWall");

            waterMesh = new GeometryNode(renderer, sceneGraph, Matrix.Translation(0.0f, 0.0f, 0.0f),
                Mesh.Circle(renderer, Color.Black, 500, 64, 1.0f), "water");

            object1 = new GeometryNode(renderer, sceneGraph, Matrix.Translation(0.0f, 0.0f, 1.0f),
                Mesh.Circle(renderer, Color.Blue, 85, 12), "stones");
            object2 = new GeometryNode(renderer, sceneGraph, Matrix.Identity,
                Mesh.Circle(renderer, Color.Blue, 65, 8), "stones");
            object3 = new GeometryNode(renderer, sceneGraph, Matrix.Identity,
                Mesh.Circle(renderer, Color.Blue, 25, 6), "stones");

            sceneGraph.Root.AddChild(backgroundQuad);
            sceneGraph.Root.AddChild(waterMesh);
            sceneGraph.Root.AddChild(object1);
            object1.AddChild(object2);
            object1.AddChild(light);
            object2.AddChild(object3);

            this.KeyDown += new KeyEventHandler(OnKeyDown);
        }

        protected override void UpdateEnvironment()
        {
            angle += AngularVelocity * timer.MoveFactorPerSecond;

            light.LocalTransform = Matrix.Translation(-200.0f, 0.0f, 2.0f) *
                Matrix.RotationZ(-angle);
            object2.LocalTransform = Matrix.Translation(350.0f, 0.0f, 1.0f) *
                Matrix.RotationZ(angle);
            object3.LocalTransform = Matrix.Translation(100.0f, 50.0f, 1.0f) *
                Matrix.RotationZ(angle);
        }

        protected override void Render3DEnvironment()
        {
            renderer.Clear();

            if (useBloom)
            {
                renderer.SaveRenderTarget();
                sceneImage.SetAsRenderTarget();
                renderer.Clear();
            }

            sceneGraph.Update();
            renderer.Render();

            if (useBloom)
            {
                renderer.RestoreRenderTarget();
                bloomProcessor.SceneImage = sceneImage;
                bloomProcessor.Render();
            }

            font.RenderText(new Vector2(30, 30), "FPS: " + timer.FramesPerSecond.ToString(), Color.Pink, true);

            renderer.Present();
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                running = false;
        }
    }
}