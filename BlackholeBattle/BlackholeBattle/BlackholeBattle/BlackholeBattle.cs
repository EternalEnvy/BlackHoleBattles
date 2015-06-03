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

namespace BlackholeBattle
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class BlackholeBattle : Microsoft.Xna.Framework.Game
    {
        const int PORT = 1521;
        const int PORT2 = 1522;
        bool? IsServer = null;
        string ServerIP = null;
        string ClientIP = null;
        Thread ReceivingThread = null;
        UdpClient client;
        UdpClient client2;
        Queue<Packet> packetProcessQueue = new Queue<Packet>();
        Model skyDome;
        Texture2D arrowTemp;
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
        static HashSet<IUnit> selectedUnits = new HashSet<IUnit>();
        public static List<GravitationalField> swallowedObjects = new List<GravitationalField>();

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
            gravityObjects.Add(new Spheroid(new Vector3(0, 0, 600), new Vector3(0, 0, 0), 100, 60, 15, "moon"));
            gravityObjects.Add(new Spheroid(new Vector3(-400, 0, 600), new Vector3(0, 0, 1.581f), 10, 15, 10, "ganymede"));
            //Blackhole b = new Blackhole("Default", 200, new Vector3(0,0,-300));
            //gravityObjects.Add(b);
            //curPlayer.myUnits.Add(b);
            hudRectangle = new Rectangle(0, graphics.PreferredBackBufferHeight * 3 / 4, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight / 4);
            hudTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] c = new Color[1];
            Byte transparency_amount = 175;
            c[0] = Color.FromNonPremultiplied(255, 255, 255, transparency_amount);
            hudTexture.SetData<Color>(c);
            IsMouseVisible = true;
            //initial camera position
            selectedUnits.Add(gravityObjects[0]);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //skyDome = Content.Load<Model>("skydome");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            planets.Add("venus", Content.Load<Model>("venus"));
            planets.Add("mars", Content.Load<Model>("mars"));
            planets.Add("earth", Content.Load<Model>("earth"));
            planets.Add("ganymede", Content.Load<Model>("ganymede"));
            planets.Add("neptune", Content.Load<Model>("neptune"));
            planets.Add("uranus", Content.Load<Model>("uranus"));
            planets.Add("moon", Content.Load<Model>("moon"));
            thumbnails.Add("venus", Content.Load<Texture2D>("ivenus"));
            thumbnails.Add("mars", Content.Load<Texture2D>("imars"));
            thumbnails.Add("earth", Content.Load<Texture2D>("iearth"));
            thumbnails.Add("ganymede", Content.Load<Texture2D>("iganymede"));
            thumbnails.Add("neptune", Content.Load<Texture2D>("ineptune"));
            thumbnails.Add("uranus", Content.Load<Texture2D>("iuranus"));
            thumbnails.Add("moon", Content.Load<Texture2D>("imoon"));
            font = Content.Load<SpriteFont>("SpriteFont1");
            arrowTemp = Content.Load<Texture2D>("arrow");
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
            foreach (GravitationalField g in swallowedObjects)
            {
                gravityObjects.Remove(g);
                selectedUnits.Remove(g);
            }
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
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //DrawModel(skyDome, 200, 0, new Vector3(0, 0, 0));
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
            foreach(IUnit unit in curPlayer.myUnits)
            {
                
                if (unit is Blackhole)
                {
                    Vector3 blackHoleScreenPos = unit.Position();
                    blackHoleScreenPos = GraphicsDevice.Viewport.Project(blackHoleScreenPos, projection, view, Matrix.CreateTranslation(0, 0, 0));
                    //blackHoleScreenPos = GraphicsDevice.Viewport.Unproject(blackHoleScreenPos, projection, view, Matrix.CreateTranslation(0, 0, 0));
                    Vector2 posOnScreen;
                    {
                        posOnScreen.X =  blackHoleScreenPos.X;
                        posOnScreen.Y = blackHoleScreenPos.Y;
                    }
                    spriteBatch.Draw(arrowTemp, new Rectangle((int)posOnScreen.X,(int)posOnScreen.Y, 50,50), Color.White);
                    //draw distance from base to blackhole
                    //spriteBatch.DrawString(
                    spriteBatch.DrawString(font, unit.Mass().ToString(), new Vector2(posOnScreen.X - 10, posOnScreen.Y - 10), Color.Red);
                }
            }
            spriteBatch.Draw(hudTexture, hudRectangle, Color.Red);
            spriteBatch.DrawString(font, curPlayer.name, new Vector2(hudRectangle.X + 5, hudRectangle.Y + 5), Color.Black);
            if (IsServer != null && ServerIP == null)
            {
                spriteBatch.DrawString(font, "Waiting on IP...", new Vector2(hudRectangle.X + 5, hudRectangle.Y + 25), Color.Black);
            }
            if (ServerIP != null)
            {
                spriteBatch.DrawString(font, "Server IP is: " + ServerIP, new Vector2(hudRectangle.X + 5, hudRectangle.Y + 25), Color.Black);
            }
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
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale((float)size / rad,(float)size / rad,(float)size/rad) * Matrix.CreateRotationY(MathHelper.ToRadians((float)rotation)) *  Matrix.CreateTranslation(position);
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
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitX);
                cross.Normalize();
                cameraPosition += cross * 17;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitX);
                cross.Normalize();
                cameraPosition -= cross * 17;
            }
            if (state.IsKeyDown(Keys.Left))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                cross.Normalize();
                cameraPosition -= cross * 17;
            }
            if (state.IsKeyDown(Keys.Right))
            {
                Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                cross.Normalize();
                cameraPosition += cross * 17;
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
