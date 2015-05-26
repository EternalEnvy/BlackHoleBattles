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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Dictionary<string, Texture2D> thumbnails = new Dictionary<string, Texture2D>();
        Dictionary<string, Model> planets = new Dictionary<string, Model>();

        Matrix projection;
        Matrix view;
        Vector3 cameraPosition = Vector3.Zero;
        Vector3 cameraDirection = Vector3.UnitZ;

        Texture2D hudTexture;
        Rectangle hudRectangle;

        Player curPlayer = new Player("Default");

        double elapsedTimeSeconds = 0;
        int scrollValue = 0;
        static List<GravitationalField> gravityObjects = new List<GravitationalField>();
        static List<IUnit> selectedUnits = new List<IUnit>();

        public BlackholeBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            gravityObjects.Add(new Spheroid(new Vector3(0, 0, 600), new Vector3(0, 0, 0), 100, 60, 15, "venus"));
            gravityObjects.Add(new Spheroid(new Vector3(-400, 0, 600), new Vector3(0, 0, 1.581f), 10, 15, 10, "ganymede"));
            //gravityObjects.Add(new Blackhole("Default", 200, new Vector3(0,400,0)));
            hudRectangle = new Rectangle(0, graphics.PreferredBackBufferHeight * 3 / 4, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight / 4);
            hudTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] c = new Color[1];
            Byte transparency_amount = 125;
            c[0] = Color.FromNonPremultiplied(255, 255, 255, transparency_amount);
            hudTexture.SetData<Color>(c);
            IsMouseVisible = true;
            //initial camera position
            selectedUnits.Add(gravityObjects[0]);
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
                {
                    (gravityObject as Spheroid).Update(objects);
                }
            }
            foreach (GravitationalField gravityObject in gravityObjects)
            {
                if (gravityObject is Spheroid)
                    (gravityObject as Spheroid).updatedInLoop = false;
                //RK4
                //gravityObject.Update((float)gameTime.TotalGameTime.TotalSeconds,(float)gameTime.ElapsedGameTime.TotalSeconds);
                //EULER
                gravityObject.Update();
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach(GravitationalField s in gravityObjects)
            {
                if (s is Spheroid)
                {
                    DrawModel(planets[s.modelName], s.size, elapsedTimeSeconds * 360 / (s as Spheroid).orbitalPeriod, s.state.x);
                }
            }
            //have the camera look at the average position of all selected objects
            Vector3 average = new Vector3();
            foreach (IUnit u in selectedUnits)
            {
                average += u.Position();
            }
            average /= selectedUnits.Count;
            cameraDirection = average;
            spriteBatch.Draw(hudTexture, hudRectangle, Color.Red);
            spriteBatch.DrawString(font, curPlayer.name, new Vector2(hudRectangle.X + 5, hudRectangle.Y + 5), Color.Black);
            spriteBatch.DrawString(font, Mouse.GetState().ScrollWheelValue.ToString(), new Vector2(hudRectangle.X + 5, hudRectangle.Y + 20), Color.Black);
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
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            view = Matrix.CreateLookAt(cameraPosition, cameraDirection, Vector3.Up);
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
            MouseState mouse = Mouse.GetState();
            bool selectMultipleUnits = false;
            if (state.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (state.IsKeyDown(Keys.Down))
            {
                cameraPosition.Z -= 10;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                cameraPosition.Z += 10;
            }
            if (state.IsKeyDown(Keys.Left))
            {
                cameraPosition.X -= 10;
            }
            if (state.IsKeyDown(Keys.Right))
            {
                cameraPosition.X += 10;
            }
            if (state.IsKeyDown(Keys.RightControl))
            {
                cameraPosition.Y -= 10;
            }
            if (state.IsKeyDown(Keys.RightShift))
            {
                cameraPosition.Y += 10;
            }
            if(state.IsKeyDown(Keys.LeftControl))
            {
                selectMultipleUnits = true;
            }
            if (mouse.RightButton == ButtonState.Pressed)
            {
                //take position of mouse on the screen
                int mouseX = mouse.X;
                int mouseY = mouse.Y;

                Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
                Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

                Matrix world = Matrix.CreateTranslation(0, 0, 0);

                //find out where they are in the worldspace by transforming the matrix back
                Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource,
                    projection, view, world);

                Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource,
                    projection, view, world);
                Vector3 direction = farPoint - nearPoint;
                direction.Normalize();

                //create a ray from the points

                Ray pickRay = new Ray(nearPoint, direction);
                GravitationalField bestGObject = new GravitationalField();
                //find the closest object to the camera that intersects with the ray
                float maxDistance = float.MaxValue;
                foreach (GravitationalField g in gravityObjects)
                {
                    float? distanceIntersection = pickRay.Intersects(g.bounds);
                    if (distanceIntersection.HasValue)
                    {
                        if (distanceIntersection < maxDistance)
                        {
                            bestGObject = g;
                            maxDistance = distanceIntersection.Value;
                        }
                    }
                }
                if (maxDistance != float.MaxValue)
                {
                    if (selectMultipleUnits == false)
                    {
                        //this allows for control groups and whatever else to be used later
                        selectedUnits.Clear();
                    }
                    selectedUnits.Add(bestGObject);
                }
            }
            //zoom the camera in based on the scroll wheel
            int mouseScrollValue = mouse.ScrollWheelValue;
            Vector3 currentCameraDirection = cameraDirection - cameraPosition;
            currentCameraDirection.Normalize();
            cameraPosition += ((mouseScrollValue - scrollValue) / 2) * currentCameraDirection;
            scrollValue = mouseScrollValue;
        }
    }
}
