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

        Effect effect;
        Geometry.cNCPath NCPath;
        Geometry.ImportMesh Mesh;

        //Save all view Settings
        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        float PixelUnitRatio;
        // The aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        #region BasicSettings

  

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
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 10000), Vector3.Zero, Vector3.Up);

            //shows the scene without perspektive
            const float ViewVieldWidth = 600f;
            projectionMatrix = Matrix.CreateOrthographic(ViewVieldWidth, ViewVieldWidth / aspectRatio, 0, 20000);
            PixelUnitRatio = (float)(ViewVieldWidth / this.graphics.GraphicsDevice.Viewport.Width);
            //shows the scene with perspektive
            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //MathHelper.ToRadians(45), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1.0f, 100.0f);
        }

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
                //LoadNCFile();
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
            Debug.WriteLine(MouseState.ScrollWheelValue.ToString());

            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Add))
            {
                Scale += 0.05f;
            }
            if (KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Subtract))
            {
                Scale -= 0.05f;
            }
       

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

  
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("SpriteFont1");

            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = state;
            effect = Content.Load<Effect>("effect");
 

            //Load fxB File
            // myModel = Content.Load<Model>("Frau");

            //CreateMesh(30, 30);
            //CreateMeshFromBitmap(@"C:\Users\bboeck\Desktop\VisualMill1\TestAusgabeSmal.jpg");
             
            NCPath = new Geometry.cNCPath(GraphicsDevice, effect, @"I:\Eigene Dateien\Eigene Dokumente\Visual Studio 2008\Projects\SVN\VisualMill1\VisualMill\Testdaten\OBERTEIL_SCHRUPPEN.NC");
            Mesh= new Geometry.ImportMesh(GraphicsDevice,effect,Content.Load<Model>("CUbe"));
       
        }

        /// <summary>
        /// Open a NC File into a Linestrip
        /// </summary>
        /// <returns></returns>       
        //public bool LoadNCFile()
        //{
        //    OpenFileDialog OpenFileDialog = new OpenFileDialog();
        //    OpenFileDialog.CheckFileExists = true;
        //    OpenFileDialog.Filter = "NC Files (*.NC *.ISO)|*.nc;*.NC;*.iso;*.ISO|Datron (*.MCR)|*.mcr;*.MCR|All Files (*.*)|*.*";
        //    OpenFileDialog.FilterIndex = 0;
        //    OpenFileDialog.RestoreDirectory = true;

        //    if (OpenFileDialog.ShowDialog() == DialogResult.OK)
        //    {            
        //     CreateNCPath(OpenFileDialog.FileName);
        //        return true;
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Open a NC File into a Linestrip
        ///// </summary>
        ///// <returns></returns>
        //public bool LoadNCFile(string Filename)
        //{
        //    if (System.IO.File.Exists(Filename))
        //    {
        //        CreateNCPath(Filename);
        //        return true;
        //    }
        //    else
        //        return LoadNCFile();

        //    return false;
        //}
    
 
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

            //DrawText();

            //effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            //effect.Parameters["xShowNormals"].SetValue(true);
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xWorld"].SetValue(worldMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            Vector3 lightDirection = new Vector3(1f,-1f,-1f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);

            //  effect.CurrentTechnique.Passes[0].Apply();

            NCPath.Draw();
            Mesh.Draw(worldMatrix,viewMatrix,projectionMatrix);
     
            base.Draw(gameTime);
        }

        #region Draw

        //private void DrawTriangle()
        //{
        //    //Draw Triangle
        //    GraphicsDevice.SetVertexBuffer(myVertexBuffer);
        //    effect.Parameters["xWorld"].SetValue(worldMatrix);
        //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Apply();
        //        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        //    }
        //}


        //private void DrawText()
        //{
        //    spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
        //    spriteBatch.DrawString(Font, "Lines Of NC Code " + myPath.VertexCount.ToString(), new Vector2(10, 10), Color.White);
        //    spriteBatch.DrawString(Font, "Current Line    " + currentIndex.ToString()+ "  " +((float)currentIndex/(float)myPath.VertexCount*100).ToString("f2")+"%", new Vector2(10, 30), Color.White);
        //    spriteBatch.DrawString(Font, "Path Length "+ PathLength.ToString("f3"), new Vector2(10, 50), Color.White);
        //    //spriteBatch.DrawString(Font, "X Min Max 1200, 234", new Vector2(10, 50), Color.White);
        //    //spriteBatch.DrawString(Font, "X Min Max 1200, 234", new Vector2(10, 70), Color.White);
        //    spriteBatch.End();
        //}

  
    
        #endregion
    }
}
