using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlackholeBattle
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class BlackholeBattle : Game
    {
        Random randall = new Random();
        const int PORT = 1521;
        const int PORT2 = 1522;
        bool? IsServer;
        string ServerIP;
        string ClientIP;
        Thread ReceivingThread;
        UdpClient client;
        UdpClient client2;
        Queue<Packet> packetProcessQueue = new Queue<Packet>();

        List<InputPacket> ClientInputBuffer = new List<InputPacket>();

        private long? FrameNumber = null;

        Texture2D arrowTemp;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

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
        int scrollValue = 0;
        List<IUnit> units = new List<IUnit>();
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
            CreateSpheroids(1);
            //CreateBase();
            CreateBlackHole(true);
            CreateBlackHole(false);
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
            models.Add("venus", Content.Load<Model>("venus"));
            models.Add("mars", Content.Load<Model>("mars"));
            models.Add("earth", Content.Load<Model>("earth"));
            models.Add("ganymede", Content.Load<Model>("ganymede"));
            models.Add("neptune", Content.Load<Model>("neptune"));
            models.Add("uranus", Content.Load<Model>("uranus"));
            models.Add("moon", Content.Load<Model>("moon"));
            thumbnails.Add("venus", Content.Load<Texture2D>("ivenus"));
            thumbnails.Add("mars", Content.Load<Texture2D>("imars"));
            thumbnails.Add("earth", Content.Load<Texture2D>("iearth"));
            thumbnails.Add("ganymede", Content.Load<Texture2D>("iganymede"));
            thumbnails.Add("neptune", Content.Load<Texture2D>("ineptune"));
            thumbnails.Add("uranus", Content.Load<Texture2D>("iuranus"));
            thumbnails.Add("moon", Content.Load<Texture2D>("imoon"));
            thumbnails.Add("blackhole", Content.Load<Texture2D>("blackhole"));
            font = Content.Load<SpriteFont>("SpriteFont1");
            arrowTemp = Content.Load<Texture2D>("arrow");
        }

        protected override void UnloadContent()
        {
            
        }
        protected override void Update(GameTime gameTime)
        {
            if(curPlayer.name == null)
            {
                curPlayer.name = Interaction.InputBox("Enter your name: ", "Name Entry");
            }

            var state = Keyboard.GetState();
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
                    curPlayer.playerID = false; ;
                    PacketQueue.Instance.AddPacket(new RequestConnectPacket { Nickname = curPlayer.name });
                }).Start();
            }

            if (state.IsKeyDown(Keys.Escape))
            {
                Exit();
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
                    FrameNumber = 0;
                }
                if (packet.GetPacketID() == 3)
                {
                    var packet2 = (InputPacket)packet;
                    if (!FrameNumber.HasValue)
                        //x frame delay before starting inputs. This allows the other players inputs to be here before we start processing.
                        FrameNumber = 0;
                    ClientInputBuffer.Add(packet2);
                }
            }

            if (FrameNumber.HasValue)
            {
                //Send keyboard input to server. This is done using reliable UDP as keyboard inputs should generally not be dropped unless they don't arrive within the buffer on the other side.
                if (IsServer == false)
                {
                    //Selected units sounds like a bad idea considering it's a hashset and .First() can be any element...
                    var packet = new InputPacket
                    {
                        FrameNumber = FrameNumber.Value,
                        CameraPosition = cameraPosition,
                        CameraRotation = cameraDirection,
                        SelectedBlackHoleID = !selectedUnits.Any() ? -1 : selectedUnits.First().ID(),
                        Front = Keyboard.GetState().IsKeyDown(Keys.W),
                        Back = Keyboard.GetState().IsKeyDown(Keys.S),
                        Left = Keyboard.GetState().IsKeyDown(Keys.A),
                        Right = Keyboard.GetState().IsKeyDown(Keys.D),
                        Up = Keyboard.GetState().IsKeyDown(Keys.LeftShift),
                        Down = Keyboard.GetState().IsKeyDown(Keys.LeftControl)
                    };
                    PacketQueue.Instance.AddPacket(packet);
                }
                else if (IsServer == true)
                {
                    if (FrameNumber >= 0)
                    {
                        for (int i = 0; i < ClientInputBuffer.Count; i++)
                        {
                            if (ClientInputBuffer[i].FrameNumber <= FrameNumber)
                            {
                                var item = ClientInputBuffer[i];
                                //TODO: Process other player's input.
                                UpdateGamePad();
                                UpdateClientGamePad(item);
                                ClientInputBuffer.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                elapsedTimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;
                List<GravitationalField> objects = new List<GravitationalField>();
                foreach (GravitationalField gravityObject in gravityObjects)
                {
                    objects.Add(gravityObject);
                }
                if (IsServer != false)
                {
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
                        units.Remove(g);
                        gravityObjects.Remove(g);
                        selectedUnits.Remove(g);
                    }
                    swallowedObjects.Clear();
                    if (selectedUnits.Count == 0)
                    {
                        selectedUnits.Add(gravityObjects[0]);
                    }
                }
                FrameNumber++;
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
            
            //have the camera look at the average position of all selected objects
            Vector3 average = new Vector3();
            foreach (IUnit u in selectedUnits)
            {
                average += u.Position();
            }
            average /= selectedUnits.Count;
            cameraDirection = average;
            foreach(IUnit unit in units)
            {
                if (unit is Blackhole)
                {
                    BoundingFrustum frustum = new BoundingFrustum(view * projection);
                    if(frustum.Contains(unit.Position()) == ContainmentType.Contains)
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
                        spriteBatch.DrawString(font, unit.Mass().ToString(), new Vector2(posOnScreen.X + 10, posOnScreen.Y - 30), Color.Red);
                        spriteBatch.DrawString(font, (unit.Position() - cameraPosition).Length().ToString(), new Vector2(posOnScreen.X + 10, posOnScreen.Y - 50), Color.Blue);
                    }
                }
            }
            foreach(IUnit unit in units)
            {
                if (!(unit is Blackhole))
                {
                    DrawModel(models[unit.ModelName()], unit.Size(), unit.Rotation(), unit.Position());
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
                if (state.IsKeyDown(Keys.Down))
                {
                    Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.Right);
                    cross.Normalize();
                    cameraPosition += cross * ((cameraDirection - cameraPosition).Length() / 8);
                }
                if (state.IsKeyDown(Keys.Up))
                {
                    Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.Right);
                    cross.Normalize();
                    cameraPosition -= cross * ((cameraDirection - cameraPosition).Length() / 8);
                }
                if (state.IsKeyDown(Keys.Left))
                {
                    Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                    cross.Normalize();
                    cameraPosition -= cross * ((cameraDirection - cameraPosition).Length() / 8);
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    Vector3 cross = Vector3.Cross((cameraDirection - cameraPosition), Vector3.UnitY);
                    cross.Normalize();
                    cameraPosition += cross * ((cameraDirection - cameraPosition).Length() / 8);
                }
            if (state.IsKeyDown(Keys.LeftControl))
            {
                selectMultipleUnits = true;
            }
            if (IsServer != false)
            {
                if (state.IsKeyDown(Keys.W))
                {
                    if (selectedUnits.First() is IMovable)
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
                if (state.IsKeyDown(Keys.Space))
                {
                    if (selectedUnits.First() is IMovable)
                    {
                        (selectedUnits.First() as IMovable).Brake();
                    }
                }
            }

            if (mouse.RightButton == ButtonState.Pressed)
            {
                //take position of mouse on the screen
                int mouseX = mouse.X;
                int mouseY = mouse.Y;

                Vector3 nearsource = new Vector3((float) mouseX, (float) mouseY, 0f);
                Vector3 farsource = new Vector3((float) mouseX, (float) mouseY, 1f);

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
                    if (!selectMultipleUnits)
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
            cameraPosition += ((mouseScrollValue - scrollValue)/2)*currentCameraDirection;
            scrollValue = mouseScrollValue;
        }
        private void UpdateClientGamePad(InputPacket input)
        {
            Vector3 lookingAt = (input.CameraRotation - input.CameraPosition);
            var unit = (units.Where(a => a.ID() == input.SelectedBlackHoleID)) as IMovable;
            if (input.Front)
            {
                var lookingAt2 = lookingAt;
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
            if (input.Left)
            {
                var lookingAt2 = Vector3.Cross(lookingAt, Vector3.Down);
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
            if (input.Back)
            {
                var lookingAt2 = -lookingAt;
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
            if (input.Right)
            {
                var lookingAt2 = Vector3.Cross(lookingAt, Vector3.Up);
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
            if (input.Up)
            {
                var lookingAt2 = Vector3.Cross(lookingAt, Vector3.Left);
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
            if (input.Down)
            {
                var lookingAt2 = Vector3.Cross(lookingAt, Vector3.Right);
                lookingAt2.Normalize();
                unit.Accelerate(lookingAt2);
            }
        }
        void CreateSpheroids(int numSpheroids)
        {           
            for(int i = 0 ; i < numSpheroids; i++)
            {
                int randy = randall.Next(1,8);
                Spheroid s = new Spheroid(new Vector3(randall.Next(-1000, 1000), randall.Next(-1000, 1000), randall.Next(-1000, 1000)), Vector3.Zero, randall.Next(1, 100), randall.Next(5, 200), randall.Next(2, 40), randy == 1 ? "earth" : randy == 2 ? "mars" : randy == 3 ? "moon" : randy == 4 ? "neptune" : randy == 5 ? "uranus" : randy == 6 ? "venus" : "ganymede");
                gravityObjects.Add(s);
                units.Add(s);
            }
        }
        void CreateBase()
        {
            //Send command, bases are not part of network stuff yet.
            units.Add(new Base(curPlayer.playerID ? new Vector3(-1000,0,0) : new Vector3(1000,0,0), curPlayer.playerID ? "player1base" : "player2base"));
        }
        void CreateBlackHole(bool id)
        {
            //Send command from client instead.
            Blackhole b = new Blackhole(id, 200, id ? new Vector3(-1000, 250, 0) : new Vector3(1000, 250, 0));
            gravityObjects.Add(b);
            units.Add(b);
        }
    }
}
