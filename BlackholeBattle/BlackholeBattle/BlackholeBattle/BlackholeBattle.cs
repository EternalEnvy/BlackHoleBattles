using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlackholeBattle
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class BlackholeBattle : Microsoft.Xna.Framework.Game
    {
        Player curPlayer = new Player("Default");
        Texture2D hudTexture;
        Rectangle hudRectangle;
        private float zoom = 2500;
        private float rotationY = 0.0f;
        private float rotationX = 0.0f;
        private Matrix gameWorldRotation;
        private static Vector3 position = new Vector3(0, 0, 600);
        private Dictionary<string, Texture2D> thumbnails = new Dictionary<string, Texture2D>();
        private Dictionary<string, Model> planets = new Dictionary<string, Model>();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        static List<GravitationalField> gravityObjects = new List<GravitationalField>();
        public BlackholeBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            hudRectangle = new Rectangle(0, graphics.PreferredBackBufferHeight * 3 / 4, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight / 4);
            hudTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] c = new Color[1];
            Byte transparency_amount = 100;
            c[0] = Color.FromNonPremultiplied(255, 255, 255, transparency_amount);
            hudTexture.SetData<Color>(c);
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //planets.Add("venus", Content.Load<Model>("venus"));
            planets.Add("mars", Content.Load<Model>("mars"));
            //planets.Add("earth", Content.Load<Model>("earth"));
            //planets.Add("ganymede", Content.Load<Model>("ganymede"));
            //planets.Add("neptune", Content.Load<Model>("neptune"));
            //planets.Add("uranus", Content.Load<Model>("uranus"));
            //planets.Add("moon", Content.Load<Model>("moon"));
        }

        protected override void UnloadContent()
        {
            
        }
        protected override void Update(GameTime gameTime)
        {
            UpdateGamePad();

            List<Tuple<Vector3, double>> positionMass = new List<Tuple<Vector3, double>>();
            foreach (GravitationalField gravityObject in gravityObjects)
            {
                positionMass.Add(new Tuple<Vector3, double>(gravityObject.position, gravityObject.mass));
            }
            foreach (GravitationalField gravityObject in gravityObjects)
            {
                if (gravityObject is Spheroid)
                    (gravityObject as Spheroid).Update(positionMass);
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Draw(hudTexture, hudRectangle, Color.Black);
            double index = 0;
            foreach (KeyValuePair<string, Model> m in planets)
            {
                DrawModel(m.Value);
                index += 250;
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
        private void DrawModel(Model m)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            m.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
               aspectRatio, 1.0f, 10000.0f);
           Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 50.0f, zoom),
               Vector3.Zero, Vector3.Up);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(4,4,4) * Matrix.CreateRotationX(MathHelper.ToRadians(rotationX)) * Matrix.CreateRotationY(MathHelper.ToRadians(rotationY)) * Matrix.CreateTranslation(position) ;
                }
                mesh.Draw();
                float rad = m.Meshes[0].BoundingSphere.Transform(m.Meshes[0].ParentBone.Transform).Radius;
            }
        }
        private void UpdateGamePad()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (state.IsKeyDown(Keys.Right))
            {
                position.X -= 10;
            }
            if (state.IsKeyDown(Keys.Left))
            {
                position.X += 10;
            }
            if (state.IsKeyDown(Keys.Down))
            {
                position.Y += 10;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                position.Y -= 10;
            }
            if (state.IsKeyDown(Keys.A))
            {
                zoom += 10;
            }
            if (state.IsKeyDown(Keys.Z))
            {
                zoom -= 10;
            }
            if (state.IsKeyDown(Keys.LeftShift))
            {
                rotationX += 10;
            }
            if (state.IsKeyDown(Keys.LeftControl))
            {
                rotationX -= 10;
            }
            if (state.IsKeyDown(Keys.RightShift))
            {
                rotationY += 10;
            }
            if (state.IsKeyDown(Keys.RightControl))
            {
                rotationY -= 10;
            }
            gameWorldRotation =
                Matrix.CreateRotationX(MathHelper.ToRadians(rotationX)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotationY));
        }
    }
}
