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
        private Vector3 position = Vector3.One;
        private float zoom = 2500;
        private float rotationY = 0.0f;
        private float rotationX = 0.0f;
        private Matrix gameWorldRotation;
        private Model earth;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        static List<GravitationalField> gravityObjects = new List<GravitationalField>();
        public BlackholeBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            earth = Content.Load<Model>("jupiter");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            UpdateGamePad();

            // TODO: Add your update logic here
            List<Tuple<Vector3, double>> positionMass = new List<Tuple<Vector3,double>>();
            foreach(GravitationalField gravityObject in gravityObjects)
            {
                positionMass.Add(new Tuple<Vector3, double>(gravityObject.position, gravityObject.mass));
            }
            foreach(GravitationalField gravityObject in gravityObjects)
            {
                if (gravityObject is Spheroid)
                    (gravityObject as Spheroid).Update(positionMass);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            DrawModel(earth);
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
                    effect.World = gameWorldRotation *
                        transforms[mesh.ParentBone.Index] *
                        Matrix.CreateTranslation(position);
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
            if (state.IsKeyDown(Keys.Right))
            {
                position.X += 10;
            }
            if (state.IsKeyDown(Keys.Left))
            {
                position.X -= 10;
            }
            if (state.IsKeyDown(Keys.Down))
            {
                position.Y += 10;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                position.Y -= 10;
            }
            if(state.IsKeyDown(Keys.A))
            {
                zoom += 10;
            }
            if(state.IsKeyDown(Keys.Z))
            {
                zoom -= 10;
            }
            if(state.IsKeyDown(Keys.LeftShift))
            {
                rotationX += 10;
            }
            if(state.IsKeyDown(Keys.LeftControl))
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
