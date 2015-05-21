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
        private SpriteFont font;
        Player curPlayer = new Player("Default");
        Texture2D hudTexture;
        Rectangle hudRectangle;
        private float zoom = 2500;
        private double elapsedTimeSeconds = 0;
        private Dictionary<string, Texture2D> thumbnails = new Dictionary<string, Texture2D>();
        private Dictionary<string, Model> planets = new Dictionary<string, Model>();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        static List<GravitationalField> gravityObjects = new List<GravitationalField>();
        public BlackholeBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            gravityObjects.Add(new Spheroid(new Vector3(50, 0, 600), new Vector3(0, 0, 0), 100, 60, "mars"));
            gravityObjects.Add(new Spheroid(new Vector3(-50, 0, 600), new Vector3(0, 0, 0.01f), 0.01, 15, "earth"));
            //HORRIBLY DESTRUCTIVE, MY PC DIED FOR LIKE AN HOUR BECAUSE OF THIS
            //gravityObjects.Add(new Blackhole("Default", 300, new Vector3(0,200,600)));
            hudRectangle = new Rectangle(0, graphics.PreferredBackBufferHeight * 3 / 4, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight / 4);
            hudTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] c = new Color[1];
            Byte transparency_amount = 125;
            c[0] = Color.FromNonPremultiplied(255, 255, 255, transparency_amount);
            hudTexture.SetData<Color>(c);
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            planets.Add("venus", Content.Load<Model>("venus"));
            planets.Add("mars", Content.Load<Model>("mars"));
            planets.Add("earth", Content.Load<Model>("earth"));
            planets.Add("ganymede", Content.Load<Model>("ganymede"));
            planets.Add("neptune", Content.Load<Model>("neptune"));
            planets.Add("uranus", Content.Load<Model>("uranus"));
            planets.Add("moon", Content.Load<Model>("moon"));
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        protected override void UnloadContent()
        {
            
        }
        protected override void Update(GameTime gameTime)
        {
            UpdateGamePad();
            elapsedTimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            List<GravitationalField> objects = new List<GravitationalField>();
            foreach (GravitationalField gravityObject in gravityObjects)
            {
               objects.Add(gravityObject);
            }
            foreach (GravitationalField gravityObject in gravityObjects)
            {
                if (gravityObject is Spheroid)
                    (gravityObject as Spheroid).Update(objects);
            }
            foreach (GravitationalField gravityObject in gravityObjects)
            {
                if (gravityObject is Spheroid)
                    (gravityObject as Spheroid).updatedInLoop = false;
                //RK4
                //gravityObject.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                //EULER
                gravityObject.Update();
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            foreach(Spheroid s in gravityObjects)
            {
                DrawModel(planets[s.modelName], s.size, elapsedTimeSeconds / s.orbitalPeriod * 2 * Math.PI, s.position);
            }
            spriteBatch.Draw(hudTexture, hudRectangle, Color.Red);
            spriteBatch.DrawString(font, curPlayer.name, new Vector2(hudRectangle.X + 10, hudRectangle.Y + 5), Color.Black);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        private void DrawModel(Model m, double size, double rotation, Vector3 position)
        //draw model of size "size"
        {
            float rad = m.Meshes[0].BoundingSphere.Transform(m.Meshes[0].ParentBone.Transform).Radius;
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
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale((float)size / rad,(float)size / rad,(float)size/rad) * Matrix.CreateRotationY(MathHelper.ToRadians((float)rotation)) * Matrix.CreateTranslation(position);
                }
                mesh.Draw();          
            }
        }
        private void UpdateGamePad()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (state.IsKeyDown(Keys.A))
            {
                zoom += 10;
            }
            if (state.IsKeyDown(Keys.Z))
            {
                zoom -= 10;
            }
        }
    }
}
