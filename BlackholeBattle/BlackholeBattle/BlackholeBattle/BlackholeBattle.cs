using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GameStateManagement;
using GameStateManagementSample;

namespace BlackholeBattle
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class BlackholeBattle : GameScreen
    {
        //TESTING HERE
        float pauseAlpha;
        InputAction pauseAction;
        ContentManager content;
        SpriteFont gameFont;
        //END TESTING

        Random randall = new Random();
        const int PORT = 1521;
        const int PORT2 = 1522;
        bool? IsServer = null;
        string ServerIP = null;
        string ClientIP = null;
        Thread ReceivingThread = null;
        UdpClient client;
        UdpClient client2;
        Queue<Packet> packetProcessQueue = new Queue<Packet>();
        Texture2D arrowTemp;
        //SpriteBatch spriteBatch;
        SpriteFont font;
        GraphicsDeviceManager graphics;

        Dictionary<string, Texture2D> thumbnails = new Dictionary<string, Texture2D>();
        Dictionary<string, Model> models = new Dictionary<string, Model>();

        Matrix projection;
        Matrix view;
        Vector3 cameraPosition = Vector3.Zero;
        Vector3 cameraDirection = Vector3.UnitZ;

        Texture2D hudTexture;
        Rectangle hudRectangle;

        Player curPlayer = new Player(null);

        public static double elapsedTimeSeconds = 0;
        float camToPosition = 300;
        int scrollValue = 0;
        List<IUnit> units = new List<IUnit>();
        static List<GravitationalField> gravityObjects = new List<GravitationalField>();
        static HashSet<IUnit> selectedUnits = new HashSet<IUnit>();
        public static List<GravitationalField> swallowedObjects = new List<GravitationalField>();

        public BlackholeBattle()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);
            //graphics.IsFullScreen = true;
        }
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
                gameFont = content.Load<SpriteFont>("gamefont");
                models.Add("venus", content.Load<Model>("venus"));
                models.Add("mars", content.Load<Model>("mars"));
                models.Add("earth", content.Load<Model>("earth"));
                models.Add("ganymede", content.Load<Model>("ganymede"));
                models.Add("neptune", content.Load<Model>("neptune"));
                models.Add("uranus", content.Load<Model>("uranus"));
                models.Add("moon", content.Load<Model>("moon"));
                models.Add("player1base", content.Load<Model>("pinksunbase"));
                models.Add("player2base", content.Load<Model>("bluesunbase"));
                thumbnails.Add("venus", content.Load<Texture2D>("ivenus"));
                thumbnails.Add("mars", content.Load<Texture2D>("imars"));
                thumbnails.Add("earth", content.Load<Texture2D>("iearth"));
                thumbnails.Add("ganymede", content.Load<Texture2D>("iganymede"));
                thumbnails.Add("neptune", content.Load<Texture2D>("ineptune"));
                thumbnails.Add("uranus", content.Load<Texture2D>("iuranus"));
                thumbnails.Add("moon", content.Load<Texture2D>("imoon"));
                thumbnails.Add("blackhole", content.Load<Texture2D>("blackhole"));
                font = content.Load<SpriteFont>("SpriteFont1");
                arrowTemp = content.Load<Texture2D>("arrow");
                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
                CreateSpheroids(1);
                //CreateBase();
                //CreateBlackHole();
                graphics = ScreenManager.Game.Services.GetService(typeof(IGraphicsDeviceService)) as GraphicsDeviceManager;
                hudRectangle = new Rectangle(0, graphics.PreferredBackBufferHeight * 3 / 4, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight / 4);
                hudTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Color[] c = new Color[1];
                Byte transparency_amount = 175;
                c[0] = Color.FromNonPremultiplied(255, 255, 255, transparency_amount);
                hudTexture.SetData<Color>(c);
                //initial camera position
                selectedUnits.Add(gravityObjects[0]);
            }
        }
        public override void Deactivate()
        {
            base.Deactivate();
        }
        public override void Unload()
        {
            content.Unload();
        }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                if (curPlayer.name == null)
                {
                    curPlayer.name = Interaction.InputBox("Enter your name: ", "Name Entry");
                }
                Vector3 average = new Vector3();
                foreach (IUnit u in selectedUnits)
                {
                    average += u.Position();
                }
                average /= selectedUnits.Count;
                cameraDirection = average;
                UpdateGamePad();
                Vector3 temp = cameraDirection - cameraPosition;
                temp.Normalize();
                cameraPosition = cameraDirection - (camToPosition * temp);
                
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
                    foreach (IUnit unit in units)
                    {
                        if (unit is Base)
                        {
                            if (unit.GetBounds().Intersects(gravityObject.GetBounds()) && gravityObject.Owner() != unit.Owner())
                            {
                                //end game
                            }
                        }
                    }
                }
                foreach (GravitationalField g in swallowedObjects)
                {
                    units.Remove(g);
                    gravityObjects.Remove(g);
                    selectedUnits.Remove(g);
                }
                swallowedObjects.Clear();
                if (selectedUnits.Count == 0)
                {
                    selectedUnits.Add(gravityObjects[0]);
                }
                while (packetProcessQueue.Any())
                {
                    var packet = packetProcessQueue.Dequeue();
                    if (packet.GetPacketID() == 1)
                    {
                        var packet2 = (RequestConnectPacket)packet;
                        new Task(() =>
                        {
                            var response = Interaction.MsgBox("Would you like to allow " + packet2.Nickname + " to connect?", MsgBoxStyle.YesNo);
                            if (response == MsgBoxResult.Yes)
                            {
                                ClientIP = packet2.IPAddress;
                                PacketQueue.Instance.AddPacket(new ConnectDecisionPacket { Accepted = true });
                            }
                            else
                            {
                                PacketQueue.Instance.AddPacket(new ConnectDecisionPacket { Accepted = false });
                            }
                        }).Start();
                    }
                    if (packet.GetPacketID() == 2)
                    {
                        var packet2 = (ConnectDecisionPacket)packet;
                        Console.WriteLine(packet2.Accepted);
                    }
                }
                if (ServerIP != null)
                    if (IsServer == true && client != null && ClientIP != null)
                        PacketQueue.TestFunc(client, new IPEndPoint(new IPAddress(ClientIP.Split('.').Select(byte.Parse).ToArray()), PORT));
                    else if (IsServer == false && client2 != null)
                        PacketQueue.TestFunc(client2, new IPEndPoint(new IPAddress(ServerIP.Split('.').Select(byte.Parse).ToArray()), PORT2));
            }
        }
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            spriteBatch.Begin();

            foreach (IUnit unit in units)
            {
                if (unit is Blackhole)
                {
                    BoundingFrustum frustum = new BoundingFrustum(view * projection);
                    if (frustum.Contains(unit.Position()) == ContainmentType.Contains)
                    {
                        Vector3 blackHoleScreenPos = unit.Position();
                        blackHoleScreenPos = ScreenManager.GraphicsDevice.Viewport.Project(blackHoleScreenPos, projection, view, Matrix.CreateTranslation(0, 0, 0));
                        //blackHoleScreenPos = GraphicsDevice.Viewport.Unproject(blackHoleScreenPos, projection, view, Matrix.CreateTranslation(0, 0, 0));
                        Vector2 posOnScreen;
                        {
                            posOnScreen.X = blackHoleScreenPos.X;
                            posOnScreen.Y = blackHoleScreenPos.Y;
                        }
                        spriteBatch.Draw(arrowTemp, new Rectangle((int)posOnScreen.X, (int)posOnScreen.Y, 50, 50), Color.White);
                        //draw distance from base to blackhole
                        //spriteBatch.DrawString(
                        spriteBatch.DrawString(font, unit.Mass().ToString(), new Vector2(posOnScreen.X + 10, posOnScreen.Y - 30), Color.Red);
                        spriteBatch.DrawString(font, (unit.Position() - cameraPosition).Length().ToString(), new Vector2(posOnScreen.X + 10, posOnScreen.Y - 50), Color.Blue);
                    }
                }
            }
            foreach (IUnit unit in units)
            {
                if (!(unit is Blackhole))
                {
                    DrawModel(models[unit.ModelName()], unit.Size(), unit.Rotation(), unit.Position());
                }
            }
            spriteBatch.Draw(hudTexture, hudRectangle, Color.Red);
            if (curPlayer.name != null)
            {
                spriteBatch.DrawString(font, curPlayer.name, new Vector2(hudRectangle.X + 5, hudRectangle.Y + 5), Color.Black);
            }
            if (IsServer != null && ServerIP == null)
            {
                spriteBatch.DrawString(font, "Waiting on IP...", new Vector2(hudRectangle.X + 5, hudRectangle.Y + 25), Color.Black);
            }
            if (ServerIP != null)
            {
                spriteBatch.DrawString(font, "Server IP is: " + ServerIP, new Vector2(hudRectangle.X + 5, hudRectangle.Y + 25), Color.Black);
            }

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }
        private void DrawModel(Model m, double size, Vector3 rotation, Vector3 position)
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
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale((float)size / rad,(float)size / rad,(float)size/rad) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);
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
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            if (state.IsKeyDown(Keys.Down))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.Right);
                cross.Normalize();
                cameraPosition += cross * ((cameraDirection - cameraPosition).Length() / 16);
            }
            if (state.IsKeyDown(Keys.Up))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.Right);
                cross.Normalize();
                cameraPosition -= cross * ((cameraDirection - cameraPosition).Length() / 16);
            }
            if (state.IsKeyDown(Keys.Left))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                cross.Normalize();
                cameraPosition -= cross * ((cameraDirection - cameraPosition).Length() / 16);
            }
            if (state.IsKeyDown(Keys.Right))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                cross.Normalize();
                cameraPosition += cross * ((cameraDirection - cameraPosition).Length() / 16);
            }
            if(state.IsKeyDown(Keys.LeftControl))
            {
                selectMultipleUnits = true;
            }
            if(state.IsKeyDown(Keys.W))
            {
                if(selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(lookingAt);
                }
            }
            if (state.IsKeyDown(Keys.A))
            {
                if (selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt = Vector3.Cross(lookingAt, Vector3.Down);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(lookingAt);
                }
            }
            if (state.IsKeyDown(Keys.S))
            {
                if (selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(-lookingAt);
                }
            }
            if (state.IsKeyDown(Keys.D))
            {
                if (selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt = Vector3.Cross(lookingAt, Vector3.Up);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(lookingAt);
                }
            }
            if (state.IsKeyDown(Keys.LeftShift))
            {
                if (selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt = Vector3.Cross(lookingAt, Vector3.Left);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(lookingAt);
                }
            }
            if (state.IsKeyDown(Keys.LeftControl))
            {
                if (selectedUnits.First() is IMovable)
                {
                    Vector3 lookingAt = (cameraDirection - cameraPosition);
                    lookingAt = Vector3.Cross(lookingAt, Vector3.Right);
                    lookingAt.Normalize();
                    (selectedUnits.First() as IMovable).Accelerate(lookingAt);
                }
            }
            if(state.IsKeyDown(Keys.Space))
            {
                if(selectedUnits.First() is IMovable)
                {
                    (selectedUnits.First() as IMovable).Brake();
                }
            }
            if (state.IsKeyDown(Keys.Home) && IsServer == null)
            {
                IsServer = true;
                new Task(() =>
                {
                    client = client ?? new UdpClient(PORT, AddressFamily.InterNetwork);
                    client2 = client2 ?? new UdpClient(PORT2, AddressFamily.InterNetwork);

                    IPHostEntry host;
                    string localIP = "?";
                    host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIP = ip.ToString();
                        }
                    }

                    ServerIP = localIP;
                    ReceivingThread = new Thread(() => PacketQueue.Instance.TestLoop(client2, new IPEndPoint(IPAddress.Any, PORT2), packetProcessQueue));
                    ReceivingThread.Start();
                }).Start();
            }
            if (state.IsKeyDown(Keys.End) && IsServer == null)
            {
                IsServer = false;
                new Task(() =>
                {
                    ServerIP = Interaction.InputBox("What is the IP adress of the host?", "Connect");
                    client = client ?? new UdpClient(PORT, AddressFamily.InterNetwork);
                    client2 = client2 ?? new UdpClient(PORT2, AddressFamily.InterNetwork);
                    ReceivingThread = new Thread(() => PacketQueue.Instance.TestLoop(client, new IPEndPoint(new IPAddress(ServerIP.Split('.').Select(byte.Parse).ToArray()), PORT), packetProcessQueue));
                    ReceivingThread.Start();
                    curPlayer.playerID = 2;
                    PacketQueue.Instance.AddPacket(new RequestConnectPacket {Nickname = curPlayer.name});
                }).Start();
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
                Vector3 nearPoint = ScreenManager.GraphicsDevice.Viewport.Unproject(nearsource,
                    projection, view, world);

                Vector3 farPoint = ScreenManager.GraphicsDevice.Viewport.Unproject(farsource,
                    projection, view, world);
                Vector3 direction = farPoint - nearPoint;
                direction.Normalize();

                //create a ray from the points

                Ray pickRay = new Ray(nearPoint, direction);
                IUnit bestGObject = new GravitationalField();
                //find the closest object to the camera that intersects with the ray
                float maxDistance = float.MaxValue;
                foreach (IUnit u in units)
                {
                    float? distanceIntersection = pickRay.Intersects(u.GetBounds());
                    if (distanceIntersection.HasValue)
                    {
                        if (distanceIntersection < maxDistance)
                        {
                            bestGObject = u;
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
            if (mouseScrollValue != scrollValue)
            {
                Vector3 currentCameraDirection = cameraDirection - cameraPosition;
                currentCameraDirection.Normalize();
                cameraPosition += (mouseScrollValue - scrollValue) * 2 * currentCameraDirection;
                camToPosition = (cameraDirection - cameraPosition).Length();
                scrollValue = mouseScrollValue;
            }
        }
        void CreateSpheroids(int numSpheroids)
        {           
            for(int i = 0 ; i < numSpheroids; i++)
            {
                int randy = randall.Next(1,8);
                Spheroid s = new Spheroid(new Vector3(randall.Next(-14000, 14000), randall.Next(-14000, 14000), randall.Next(-14000, 14000)), Vector3.Zero, randall.Next(1, 100), randall.Next(5, 200), randall.Next(2, 40), randy == 1 ? "earth" : randy == 2 ? "mars" : randy == 3 ? "moon" : randy == 4 ? "neptune" : randy == 5 ? "uranus" : randy == 6 ? "venus" : "ganymede", curPlayer.name);
                gravityObjects.Add(s);
                units.Add(s);
            }
        }
        void CreateBase()
        {
            units.Add(new Base(curPlayer.playerID == 1 ? new Vector3(-10000,0,0) : new Vector3(10000,0,0), curPlayer.playerID == 1 ? "player1base" : "player2base", curPlayer.name));
        }
        void CreateBlackHole()
        {
            Blackhole b = new Blackhole(curPlayer.name, 200, curPlayer.playerID == 1 ? new Vector3(-10000, 2500, 0) : new Vector3(10000, 2500, 0));
            gravityObjects.Add(b);
            units.Add(b);
        }
    }
}
