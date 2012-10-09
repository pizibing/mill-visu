using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Windows.Forms;




namespace VisualMill
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainProgramm : Microsoft.Xna.Framework.Game
    {

        private IntPtr drawSurface; 
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont Font;

        VertexBuffer myVertexBuffer;
        VertexBuffer myPath;
       VertexBuffer myVertexBufferMesh;
        IndexBuffer myIndexBufferMesh;
        Effect effect;

        //Save all view Settings
        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        float PixelUnitRatio;
        // The aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        #region BasicSettings

        public struct VertexPositionNormalColored
        {
            public Vector3 Position;
            public Color Color;
            public Vector3 Normal;

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            );
        }

        //public struct MyVertexType : IVertexType
        //{
        //    public readonly Vector3 Position;
        //    public readonly Color Color;

        //    public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
        //        new VertexElement[]
        //    {
        //        new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        //        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        //    }
        //    );

        //    public MyVertexType(Vector3 position, Color color)
        //    {
        //        Position = position;
        //        Color = color;
        //    }

        //    VertexDeclaration IVertexType.VertexDeclaration
        //    {
        //        get { return VertexDeclaration; }
        //    }
        //}


        /// <summary>
        /// Event capturing the construction of a draw surface and makes sure this gets redirected to
        /// a predesignated drawsurface marked by pointer drawSurface
        /// </summary>
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
                e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle =
                drawSurface;
        }

        /// <summary>
        /// Occurs when the original gamewindows' visibility changes and makes sure it stays invisible
        /// </summary>
        ///
        private void Game1_VisibleChanged(object sender, EventArgs e)
        {
                if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)
                    System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;
        }

        //public MainProgramm(IntPtr drawSurface)
        
        public MainProgramm()
        {
        
            MainForm form = new MainForm();
           
            form.Show();
            IntPtr drawSurface = form.getDrawSurface();

            graphics = new GraphicsDeviceManager(this);     
            Content.RootDirectory = "Content";


            this.drawSurface = drawSurface;
            graphics.PreparingDeviceSettings +=
                          new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Control.FromHandle((this.Window.Handle)).VisibleChanged +=
                   new EventHandler(Game1_VisibleChanged);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();

           
            //set size of the screen
            graphics.PreferredBackBufferWidth = 1300;
            graphics.PreferredBackBufferHeight = (int)(1300 / 1.6);
            graphics.ApplyChanges();
            //graphics.ToggleFullScreen();

            //make Mosue settings
            this.IsMouseVisible = true;
            MouseState= Mouse.GetState();
            LastMouseState = MouseState;

            //init the world
            InitializeTransform();
        }

        private void InitializeTransform()
        {
            aspectRatio = GraphicsDevice.Viewport.AspectRatio;

            float tilt = MathHelper.ToRadians(22.5f);
            worldMatrix = Matrix.Identity;// Matrix.CreateRotationX(tilt) * Matrix.CreateRotationY(tilt);           
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 1000), Vector3.Zero, Vector3.Up);

            //shows the scene without perspektive
            const float ViewVieldWidth = 600f;
            projectionMatrix = Matrix.CreateOrthographic(ViewVieldWidth, ViewVieldWidth / aspectRatio, -1000, 2000);
            PixelUnitRatio = (float)(ViewVieldWidth / this.graphics.GraphicsDevice.Viewport.Width);
            //shows the scene with perspektive
            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //MathHelper.ToRadians(45), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1.0f, 100.0f);
        }

        KeyboardState KeyState;
        MouseState MouseState;
        MouseState LastMouseState;
        bool LookBackmove = false;
        /// <summary>
        /// Get the transformation of the World from Keyboard and Mouse
        /// </summary>
        /// <param name="Transfom">Transormation Matrix that Contains the current World</param>
        private void UpdateTransformWorld(ref Matrix Transfom)
        {
            const float fTrans = 4f;
            const float fRot = 0.02f;
            const float fScale = 0.001f;
    
            float xRot = 0;
            float yRot = 0;
            float xTrans = 0;
            float yTrans = 0;
            float Scale = 0;

            MouseState = Mouse.GetState();
            KeyState = Keyboard.GetState();

            //do the Mouse work
            int dx = MouseState.X - LastMouseState.X;
            int dy = MouseState.Y - LastMouseState.Y;

            //load file
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.L) && KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                LoadNCFile();
            }

            //calc the translation
            if (MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                xTrans = dx * PixelUnitRatio;
                yTrans = dy * -PixelUnitRatio;
            }

            //calc the rotation
            if (MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                xRot = fRot * dx * 0.5f;
                yRot = fRot * dy * 0.5f;
            }

            //center the Part
            if (MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed & !LookBackmove)
            {
                xTrans = -(MouseState.X - this.graphics.GraphicsDevice.Viewport.Width / 2) * PixelUnitRatio;
                yTrans = (MouseState.Y - this.graphics.GraphicsDevice.Viewport.Height / 2) * PixelUnitRatio;
                LookBackmove = true;
            }
            if (MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                LookBackmove = false;
            }

            //calc the scale
            Scale = (float)(1 - (MouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue) * fScale);


            //do the key work
            //calc the rotation
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) && !KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                xRot = -fRot;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) && !KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                xRot = fRot;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && !KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                yRot = -fRot;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && !KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                yRot = fRot;
            }

            //calc the translation
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) && KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                xTrans = -fTrans;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                yTrans = fTrans;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                yTrans = -fTrans;
            }

            //do the transformation
            Transfom = Transfom * Matrix.CreateFromYawPitchRoll(xRot, yRot, 0) * Matrix.CreateTranslation(xTrans, yTrans, 0) * Matrix.CreateScale(Scale);

            //update the MouseState
            LastMouseState = MouseState;
        }
        #endregion

        // Set the 3D model to draw.
        Model myModel;

        #region Creategeometry
        // private void CreateTriangle()
        //{
        //    myVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(MyVertexType), 3, BufferUsage.WriteOnly);

        //    MyVertexType[] vertices = new MyVertexType[]
        //    {
        //        new MyVertexType(new Vector3( 0.0f, -0.5f, 0.0f), Color.Red),
        //        new MyVertexType(new Vector3(-0.5f,  0.5f, 0.0f), Color.Green),
        //        new MyVertexType(new Vector3( 0.5f,  0.5f, 0.0f), Color.Blue),
        //    };

        //    myVertexBuffer.SetData<MyVertexType>(vertices);
        //}   

        //private void CreateTriangle()
        //{
        //    myVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), 8, BufferUsage.WriteOnly);

        //    VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[]
        //    {
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),
        //        new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f, 0.0f),new Vector3(-0.5f,  0.5f, 0.0f),new Vector2()),

        //     };


        //    myVertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
        //}

        private void CreateMesh(int Xsize, int Ysize)
        {
            const float Scale = 5;

            VertexPositionColor[] MeshVertexBuffer = new VertexPositionColor[Xsize * Ysize];
            for (int Y = 0; Y < Ysize; Y++)
            {
                for (int X = 0; X < Xsize; X++)
                {
                    MeshVertexBuffer[Y * Xsize + X] = new VertexPositionColor(new Vector3((float)(X * Scale), (float)(Y * Scale), 0), Color.Red);
                }
            }

            myVertexBufferMesh = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, MeshVertexBuffer.Length, BufferUsage.WriteOnly);
            myVertexBufferMesh.SetData(MeshVertexBuffer);

            //create Indizes (short can 250²)
            int[] indices = new int[(Xsize - 1) * (Ysize - 1) * 2 * 3];
            int IndexCounter = 0;
            for (int Y = 0; Y < Ysize - 1; Y++)
            {
                for (int X = 0; X < Xsize - 1; X++)
                {
                    indices[IndexCounter++] = (int)(Xsize * Y + X);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + 1);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize);

                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + 1);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize + 1);
                }
            }
            myIndexBufferMesh = new IndexBuffer(GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            myIndexBufferMesh.SetData(indices);

        }

        private struct PlotSegment
        {
            public int VertexStartIndex;
            public int VertexCounts;
            public int IndexStartIndex;
            public int IndexCounts;
        }

        List<PlotSegment> PlotSegments;
    
        private void CreateMeshFromBitmap(string Filename)
        {
            //ModelMesh mesh;
            //mesh.
            PlotSegments = new List<PlotSegment>();

            int Xsize, Ysize;
            System.Drawing.Bitmap Image = new System.Drawing.Bitmap(Filename);
   
            Xsize = Image.Width;
            Ysize = Image.Height;

            const float Scale = 5;
            float ZPos;

            //create the mesh with the Values from the Picture
            VertexPositionNormalColored[] MeshVertexBuffer = new VertexPositionNormalColored[Xsize * Ysize];
            for (int Y = 0; Y < Ysize; Y++)
            {
                for (int X = 0; X < Xsize; X++)
                {
                    ZPos = (Image.GetPixel(X, Ysize - 1 - Y).R + Image.GetPixel(X, Ysize - 1 - Y).G + Image.GetPixel(X, Ysize - 1 - Y).B) / 5f;
                    //ZPos = 0;
                    MeshVertexBuffer[Y * Xsize + X].Position = new Vector3((float)(X * Scale), (float)(Y * Scale), ZPos);
                    MeshVertexBuffer[Y * Xsize + X].Color = Color.LightSteelBlue;
                    MeshVertexBuffer[Y * Xsize + X].Normal = new Vector3();
                }
            }

            myVertexBufferMesh = new VertexBuffer(GraphicsDevice, VertexPositionNormalColored.VertexDeclaration, MeshVertexBuffer.Length, BufferUsage.None);
            myVertexBufferMesh.SetData(MeshVertexBuffer);

            //create Indizes 
            int[] indices = new Int32[(Xsize - 1) * (Ysize - 1) * 2 * 3];
            int IndexCounter = 0;
            for (int Y = 0; Y < Ysize - 1; Y++)
            {
                for (int X = 0; X < Xsize - 1; X++)
                {
                    indices[IndexCounter++] = (int)(Xsize * Y + X);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + 1);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize);

                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + 1);
                    indices[IndexCounter++] = (int)(Xsize * Y + X + Xsize + 1);
         
                }
            }
            myIndexBufferMesh = new IndexBuffer(GraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
            myIndexBufferMesh.SetData(indices);

            //create the Draw List with the limits of ~1e6 primitives
            //check that each triangle needs 3 Vertices
            const int offset = 999999;
            int CurrentIndex = 0;
            PlotSegment NewSet;
            while (CurrentIndex < indices.Length - offset)
            {
                NewSet = new PlotSegment();
                NewSet.IndexStartIndex = CurrentIndex;
                NewSet.IndexCounts = offset;
                NewSet.VertexStartIndex = indices[CurrentIndex];
                NewSet.VertexCounts = indices[CurrentIndex + NewSet.IndexCounts-1] - indices[CurrentIndex];
                PlotSegments.Add(NewSet);
                CurrentIndex += offset;
            }
            NewSet = new PlotSegment();
            NewSet.IndexStartIndex = CurrentIndex;
            NewSet.IndexCounts = indices.Length-CurrentIndex;
            NewSet.VertexStartIndex = indices[CurrentIndex];
            NewSet.VertexCounts = MeshVertexBuffer.Length - indices[CurrentIndex];
            PlotSegments.Add(NewSet);


            GenerateNormals(myVertexBufferMesh, myIndexBufferMesh);
        }

        private void GenerateNormals(VertexBuffer vb, IndexBuffer ib)
        {
            VertexPositionNormalColored[] vertices = new VertexPositionNormalColored[vb.VertexCount];
            vb.GetData(vertices);
            int[] indices = new int[ib.IndexCount];
            ib.GetData(indices);

            //set the normals to zerro
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            //add each face normal to vertex to make a sum for the middle value
            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstvec = vertices[indices[i * 3 + 1]].Position - vertices[indices[i * 3]].Position;
                Vector3 secondvec = vertices[indices[i * 3]].Position - vertices[indices[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertices[indices[i * 3]].Normal += normal;
                vertices[indices[i * 3 + 1]].Normal += normal;
                vertices[indices[i * 3 + 2]].Normal += normal;
            }

            //nomralize the normals again after all the summary
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }
            //set the data back to vertices
            vb.SetData(vertices);
        }

        double PathLength = 0;
        private void CreateNCPath(string Filename)
        {

            if (!System.IO.File.Exists(Filename))
            {

            }

            GCode GcodeImport = new GCode();
            double CurrentAngle;
   
            Vector3 Vec1, Vec2;
            int CurrentColor;
            double Denum;

            List<Vector3> VectorList = GcodeImport.ImportGCodeVector3(Filename);
            VertexPositionColor[] VertexList = new VertexPositionColor[VectorList.Count];

            VertexPositionColor CurrentVertePositionColored;
            for (int Index = 1; Index < VectorList.Count - 1; Index++)
            {
                PathLength += (VectorList[Index] - VectorList[Index - 1]).Length();

                //calc angle
                Vec1 = VectorList[Index] - VectorList[Index - 1];
                Vec2 = VectorList[Index] - VectorList[Index + 1];

                CurrentVertePositionColored.Position = VectorList[Index];

                Denum = (Vec1.Length() * Vec2.Length());
                if (Denum != 0)
                {
                    CurrentAngle = Math.Acos(Vector3.Dot(Vec1, Vec2) / Denum); //kommt in ca. 180° 
                    CurrentAngle = (180 - CurrentAngle * 180 / Math.PI);
                }
                else
                {
                    CurrentAngle = 0;
                }

                 CurrentColor = (int)(CurrentAngle * 60); //the constant facor makes the window of colorchange 360/knick°= factor
                if (CurrentColor > 255) CurrentColor = 255;
                if (CurrentAngle > 45) CurrentColor = 0;


                CurrentVertePositionColored.Color = Color.FromNonPremultiplied((int)(CurrentColor), (int)(255 - CurrentColor), 0, 255);
                //CurrentVertePositionColored.Color = Color.FromNonPremultiplied(0,0, 0, 255);
                //CurrentVertePositionColored.Color = Color.Green;

                VertexList[Index] = CurrentVertePositionColored;
            }
            
            //int[] PathIndex = new int[VectorList.Count];
            //for (int i = 0; i < VectorList.Count; i++)
            //{
            //    PathIndex[i] = (short)(i);
            //}

            //myPathIndex = new IndexBuffer(GraphicsDevice, typeof(int), VectorList.Count, BufferUsage.WriteOnly);
            //myPathIndex.SetData(PathIndex);


            myPath = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, VectorList.Count, BufferUsage.WriteOnly);
            myPath.SetData(VertexList);           
        }
        #endregion

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("SpriteFont1");

            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = state;

            //Load fxB File
            // myModel = Content.Load<Model>("Frau");

            //CreateMesh(30, 30);
            //CreateMeshFromBitmap(@"C:\Users\bboeck\Desktop\VisualMill1\TestAusgabeSmal.jpg");
        
            //CreateNCPath("SCHLICHTEN 0_002.NC");
            LoadNCFile(@"I:\Eigene Dateien\Eigene Dokumente\Visual Studio 2008\Projects\SVN\VisualMill1\VisualMill\Testdaten\OBERTEIL_SCHRUPPEN.NC");
            effect = Content.Load<Effect>("effect");
        }

        /// <summary>
        /// Open a NC File into a Linestrip
        /// </summary>
        /// <returns></returns>       
        public bool LoadNCFile()
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.CheckFileExists = true;
            OpenFileDialog.Filter = "NC Files (*.NC *.ISO)|*.nc *.iso|Datron (*.MCR)|*.mcr|All Files (*.*)|*.*";
            OpenFileDialog.FilterIndex = 0;
            OpenFileDialog.RestoreDirectory = true;

            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
            
             CreateNCPath(OpenFileDialog.FileName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Open a NC File into a Linestrip
        /// </summary>
        /// <returns></returns>
        public bool LoadNCFile(string Filename)
        {
            if (System.IO.File.Exists(Filename))
            {
                CreateNCPath(Filename);
                return true;
            }
            else
                return LoadNCFile();

            return false;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            UpdateTransformWorld(ref worldMatrix);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DrawText();

            //effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            //effect.Parameters["xShowNormals"].SetValue(true);
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xWorld"].SetValue(worldMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            Vector3 lightDirection = new Vector3(1f,-1f,-1f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);

            effect.CurrentTechnique.Passes[0].Apply();

           // DrawMesh();
            DrawNCPath();
            //DrawTriangle();
            //DrawImportMesh();

            base.Draw(gameTime);
        }

        #region Draw
        private void DrawTriangle()
        {
            //Draw Triangle
            GraphicsDevice.SetVertexBuffer(myVertexBuffer);
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }

        private void DrawMesh()
        {
            //DrawMesh
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SetVertexBuffer(myVertexBufferMesh);
            GraphicsDevice.Indices = myIndexBufferMesh;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (PlotSegment Segment in PlotSegments)
                {
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, Segment.VertexStartIndex, Segment.VertexCounts, Segment.IndexStartIndex, Segment.IndexCounts);
                }
            }
        }

        private void DrawText()
        {
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.DrawString(Font, "Lines Of NC Code " + myPath.VertexCount.ToString(), new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(Font, "Current Line    " + currentIndex.ToString()+ "  " +((float)currentIndex/(float)myPath.VertexCount*100).ToString("f2")+"%", new Vector2(10, 30), Color.White);
            spriteBatch.DrawString(Font, "Path Length "+ PathLength.ToString("f3"), new Vector2(10, 50), Color.White);
            //spriteBatch.DrawString(Font, "X Min Max 1200, 234", new Vector2(10, 50), Color.White);
            //spriteBatch.DrawString(Font, "X Min Max 1200, 234", new Vector2(10, 70), Color.White);
            spriteBatch.End();
        }

        int currentIndex = 1;
        private void DrawNCPath()
        {
            //draw NC Path
            effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SetVertexBuffer(myPath);
            //GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, myPath.VertexCount);
            GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, currentIndex);

            if (currentIndex< myPath.VertexCount)
            currentIndex+=40;
        }

        private void DrawImportMesh()
        {
            //Draw Import Mesh
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect Modeffect in mesh.Effects)
                {
                    Modeffect.EnableDefaultLighting();
                    Modeffect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    //Modeffect.View = Matrix.CreateLookAt(new Vector3(0,0,500),
                    Modeffect.View = viewMatrix;
                    // new Vector3(0,0,0), Vector3.Up);

                    //Modeffect.Projection = Matrix.CreatePerspectiveFieldOfView(
                    //    MathHelper.ToRadians(45.0f), aspectRatio,
                    //    1.0f, 10000.0f);

                    Modeffect.Projection = projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();

            }
        }

        #endregion
    }
}
