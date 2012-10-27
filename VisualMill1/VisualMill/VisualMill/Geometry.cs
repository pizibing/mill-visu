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

namespace VisualMill
{
    static class Geometry
    {
        public struct PlotSegment
        {
            public int VertexStartIndex;
            public int VertexCounts;
            public int IndexStartIndex;
            public int IndexCounts;
        }  

        public interface IDrawableObject
        {
           // bool Visible { get; set; }
            void Draw();
        }

        public static void GenerateNormals(VertexBuffer vb, IndexBuffer ib)
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

        public class cMesh : IDrawableObject
        {

            VertexBuffer myVertexBufferMesh;
            IndexBuffer myIndexBufferMesh;
            Effect effect;
            GraphicsDevice GraphicsDevice;
             
            List<PlotSegment> PlotSegments;

            public void Draw()
            {
                RasterizerState state = new RasterizerState();
                state.CullMode = CullMode.None;
                state.FillMode = FillMode.WireFrame;
                GraphicsDevice.RasterizerState = state;
                
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

            public cMesh(GraphicsDevice Device, Effect GivenEffect, int Xsize, int Ysize)
            {
                GraphicsDevice = Device;
                effect = GivenEffect;


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
        }

        public class cMeshFromBitmap : IDrawableObject
        {

            VertexBuffer myVertexBufferMesh;
            IndexBuffer myIndexBufferMesh;
            Effect effect;
            GraphicsDevice GraphicsDevice;

            List<PlotSegment> PlotSegments;

            public void Draw()
            {
                RasterizerState state = new RasterizerState();
                state.CullMode = CullMode.None;
                state.FillMode = FillMode.WireFrame;
                GraphicsDevice.RasterizerState = state;

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

            public cMeshFromBitmap(GraphicsDevice Device, Effect GivenEffect,string Filename)
            {
                GraphicsDevice = Device;
                effect = GivenEffect;

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
                    NewSet.VertexCounts = indices[CurrentIndex + NewSet.IndexCounts - 1] - indices[CurrentIndex];
                    PlotSegments.Add(NewSet);
                    CurrentIndex += offset;
                }
                NewSet = new PlotSegment();
                NewSet.IndexStartIndex = CurrentIndex;
                NewSet.IndexCounts = indices.Length - CurrentIndex;
                NewSet.VertexStartIndex = indices[CurrentIndex];
                NewSet.VertexCounts = MeshVertexBuffer.Length - indices[CurrentIndex];
                PlotSegments.Add(NewSet);


                GenerateNormals(myVertexBufferMesh, myIndexBufferMesh);
            }

        }

        public class cNCPath : IDrawableObject
        {
            VertexBuffer myPath;

            Effect effect;
            GraphicsDevice GraphicsDevice;
            double PathLength = 0;

            public void Draw()
            {
                RasterizerState state = new RasterizerState();
                state.CullMode = CullMode.None;
                state.FillMode = FillMode.WireFrame;
                GraphicsDevice.RasterizerState = state;
                
                //draw NC Path
                effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
                effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.SetVertexBuffer(myPath);
                GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, myPath.VertexCount);

                //if (currentIndex < myPath.VertexCount)
                //    currentIndex += 40;
            }

            public cNCPath(GraphicsDevice Device, Effect GivenEffect, string Filename)
            {
                GraphicsDevice = Device;
                effect = GivenEffect;

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
                    if (CurrentAngle > 3) CurrentColor = 255; else CurrentColor = 0;

                    if (CurrentColor > 255) CurrentColor = 255;
                    //if (CurrentAngle > 70) CurrentColor = 0;


                    CurrentVertePositionColored.Color = Color.FromNonPremultiplied((int)(CurrentColor), (int)(255 - CurrentColor), 0, 255);
                    //CurrentVertePositionColored.Color = Color.FromNonPremultiplied(0,0, 0, 255);
                    //CurrentVertePositionColored.Color = Color.Green;

                    VertexList[Index] = CurrentVertePositionColored;
                }

                myPath = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, VectorList.Count, BufferUsage.WriteOnly);
                myPath.SetData(VertexList);
            }
  
        }

        public class ImportMesh : IDrawableObject
        {
            // Set the 3D model to draw.
            Model myModel;
            Effect effect;
            GraphicsDevice GraphicsDevice;

            public void Draw()
            {
                
            }

            public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
            {
                RasterizerState state = new RasterizerState();
                state.CullMode = CullMode.None;
                state.FillMode = FillMode.Solid;
                GraphicsDevice.RasterizerState = state;

                //create position rigth up
                Vector3 Scale;
                Quaternion Rotation;
                Vector3 Transform;
                worldMatrix.Decompose(out Scale, out Rotation, out Transform);
                Matrix NewWorld = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(250, 150, 0);

                //Draw Import Mesh
                // Copy any parent transforms.
                Matrix[] transforms = new Matrix[myModel.Bones.Count];
                myModel.CopyAbsoluteBoneTransformsTo(transforms);

                //effect.CurrentTechnique = effect.Techniques["Colored"];
                //effect.CurrentTechnique.Passes[0].Apply();  

                // Draw the model. A model can have multiple meshes, so loop.
                foreach (ModelMesh mesh in myModel.Meshes)
                {
                    // This is where the mesh orientation is set, as well 
                    // as our camera and projection.
                    foreach (BasicEffect Modeffect in mesh.Effects)
                    {                       
                        Modeffect.EnableDefaultLighting();
                        Modeffect.World = transforms[mesh.ParentBone.Index] * NewWorld;
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

            public ImportMesh(GraphicsDevice Device, Effect GivenEffect, Model CurrentModel)
            {
                GraphicsDevice = Device;
                effect = GivenEffect;
                myModel = CurrentModel;
            }


        }


    }
}
