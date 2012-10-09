// TODO: Umstellen auf Indexlisten

using System;
using System.Diagnostics;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Windows;
using Color = SharpDX.Color;

public struct quadResult {
    public bool all_same;
    public float height;
    public bool marked;
}

public struct sizeXZ {
    public int xs;
    public int zs;
}

namespace ShowHeightMap
{
    internal class Program
    {
        const string heightmap_file = "pngs\\peg_1024.png";

        static float zoom = 1.0f;
        static bool mouseDrag = false;
        static bool dragButtonLeft = false;
        static int dragX, dragY;
        static float transX = 0.0f, transY = 0.0f;
        static float rotX = 0.0f, rotY = 0.0f;
        static Random r = new Random();
        static float minx = -1.0f, maxx = 1.0f;
        static float minz = -1.0f, maxz = 1.0f;
        static public int x_dim, z_dim;
        static bool drillKeyDown = false;
        static float drillX1, drillZ1, drillX2, drillZ2;
        static float xstep, zstep;
        static float toolDm;
        static float[] heightmap;
        static bool[] markedmap;
        static sizeXZ[] sizemap;
        static VertexBuffer vertices;
        static int triangle_count;
        static int vertex_count;
        static Device device;

        private static quadResult recursiveCall(int x, int z, int xs, int zs) {
            quadResult q_res = new quadResult();

            if ((xs <= 1) && (zs <= 1)) {
                q_res.all_same = true;
                q_res.height = heightmap[z * x_dim + x];
                q_res.marked = markedmap[z * x_dim + x];
                sizemap[z * x_dim + x].xs = 1;
                sizemap[z * x_dim + x].zs = 1;
                return q_res;
            }

            int xs2 = xs >> 1;
            int zs2 = zs >> 1;
            quadResult q1, q2, q3, q4;

            q1 = recursiveCall(x, z, xs2, zs2);
            q2 = recursiveCall(x + xs2, z, xs - xs2, zs2);
            q3 = recursiveCall(x, z + zs2, xs2, zs - zs2);
            q4 = recursiveCall(x + xs2, z + zs2, xs - xs2, zs - zs2);

            q_res.all_same = false;
            if ((q1.all_same) && (q2.all_same) && (q1.height == q2.height) && (q1.marked == q2.marked)) {
                if (q3.all_same && (q1.height == q3.height) && (q1.marked == q3.marked)) {
                    if (q4.all_same && (q1.height == q4.height) && (q1.marked == q4.marked)) {
                        q_res.all_same = true;
                        q_res.height = q1.height;
                        q_res.marked = q1.marked;
                    }
                }
            }

            if (q_res.all_same) {
                if ((xs > 2) && (zs > 2)) { // only quads above 2 x 2 decrease polygon count
                    for (int zz = z; zz < z + zs; zz++) {
                        // mark left side as negative to be able to skip them
                        sizemap[zz * x_dim + x].xs = -xs;
                        sizemap[zz * x_dim + x].zs = -zs;
                        // mark right side as non-quad to connect quads and neightbour regions
                        sizemap[zz * x_dim + x + (xs - 1)].xs = 1;
                        sizemap[zz * x_dim + x + (xs - 1)].zs = 1;
                    }
                    // mark lower side as non-quad to connect quads and neighbour regions
                    for (int xx = x; xx < x + xs; xx++) {
                        sizemap[(z + (zs - 1)) * x_dim + xx].xs = 1;
                        sizemap[(z + (zs - 1)) * x_dim + xx].zs = 1;
                    }
                    // mark first pixel as quad region
                    sizemap[z * x_dim + x].xs = xs;
                    sizemap[z * x_dim + x].zs = zs;
                } else {
                    for (int zz = z; zz < z + zs; zz++) {
                        for (int xx = x; xx < x + xs; xx++) {
                            sizemap[zz * x_dim + xx].xs = 1;
                            sizemap[zz * x_dim + xx].zs = 1;
                        }
                    }
                }
            }

            return q_res;
        }

        private static void calcHeightMapSizes() {
            recursiveCall(0, 0, x_dim, z_dim);
        }

        private static void HeightMapToVertices(bool recalculateSizes, bool write) {
            DataStream datastream = null;

            if (recalculateSizes) calcHeightMapSizes();
            triangle_count = 0;
            if (write) {
                vertices = new VertexBuffer(device, Utilities.SizeOf<Vector4>() * vertex_count, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
                datastream = vertices.Lock(0, 0, LockFlags.None);
            } else {
                vertex_count = 0;
            }
            for (int z = 0; z < z_dim - 1; z++) {
                float zf = (z / (float)z_dim) * (maxz - minz) + minz;
                for (int x = 0; x < x_dim - 1; x++) {
                    float xf = (x / (float)x_dim) * (maxx - minx) + minx;

                    float y1, y2, y3, y4;
                    float red1, red2, red3, red4;

                    int xs = sizemap[z * x_dim + x].xs;
                    int zs = sizemap[z * x_dim + x].zs;
                    if (xs > 0) {
                        y1 = heightmap[z * x_dim + x];
                        if (markedmap[z * x_dim + x]) { red1 = 1.0f; } else { red1 = y1 + 0.5f; }

                        if (xs == 1) { //non-quad
                            if (write) {
                                y2 = heightmap[(z + 1) * x_dim + x];
                                y3 = heightmap[z * x_dim + x + 1];
                                y4 = heightmap[(z + 1) * x_dim + x + 1];
                                if (markedmap[(z + 1) * x_dim + x]) { red2 = 1.0f; } else { red2 = y2 + 0.5f; }
                                if (markedmap[z * x_dim + x + 1]) { red3 = 1.0f; } else { red3 = y3 + 0.5f; }
                                if (markedmap[(z + 1) * x_dim + x + 1]) { red4 = 1.0f; } else { red4 = y4 + 0.5f; }
                                // Mark non-quad regions blue for debugging purposes
                                // red1 = red2 = red3 = red4 = 0.0f;
                                datastream.WriteRange(new[] {
                                    new Vector4(xf, y1, zf, 1.0f),                          // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),          // color
                                    new Vector4(xf, y2, zf + zstep, 1.0f),                  // position
                                    new Vector4(red2, y2 + 0.5f, y2 + 0.5f, 1.0f),          // color
                                    new Vector4(xf + xstep, y3, zf, 1.0f),                  // position
                                    new Vector4(red3, y3 + 0.5f, y3 + 0.5f, 1.0f),          // color

                                    new Vector4(xf + xstep, y3, zf, 1.0f),                  // position
                                    new Vector4(red3, y3 + 0.5f, y3 + 0.5f, 1.0f),          // color
                                    new Vector4(xf, y2, zf + zstep, 1.0f),                  // position
                                    new Vector4(red2, y2 + 0.5f, y2 + 0.5f, 1.0f),          // color
                                    new Vector4(xf + xstep, y4, zf + zstep, 1.0f),          // position
                                    new Vector4(red4, y4 + 0.5f, y4 + 0.5f, 1.0f)           // color
                                });
                            }
                            if (!write) vertex_count += 12;
                            triangle_count += 2;
                        } else { //quad
                            if (write) {
                                float xf2 = xf + (xstep * (xs - 1));
                                float zf2 = zf + (zstep * (zs - 1));
                                datastream.WriteRange(new[] {
                                    new Vector4(xf, y1, zf, 1.0f),                                              // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),                              // color
                                    new Vector4(xf, y1, zf2, 1.0f),                                             // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),                              // color
                                    new Vector4(xf2, y1, zf, 1.0f),                                             // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),                              // color

                                    new Vector4(xf2, y1, zf, 1.0f),                                             // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),                              // color
                                    new Vector4(xf, y1, zf2, 1.0f),                                             // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f),                              // color
                                    new Vector4(xf2, y1, zf2, 1.0f),                                            // position
                                    new Vector4(red1, y1 + 0.5f, y1 + 0.5f, 1.0f)                               // color
                                });
                            }
                            if (!write) vertex_count += 12;
                            triangle_count += 2;
                        }
                    }

                    // skip quad region
                    if (Math.Abs(xs) > 1) {
                        x += Math.Abs(xs) - 2;
                    }
                }
            }
            if (write) {
                vertices.Unlock();
                device.SetStreamSource(0, vertices, 0, Utilities.SizeOf<Vector4>() * 2);
            }
        }

        static void clearMarkedMap() {
            for (int x = 0; x < x_dim; x++) {
                for (int y = 0; y < z_dim; y++) {
                    markedmap[y * x_dim + x] = false;
                }
            }
        }

        [STAThread]
        private static void Main()
        {
            var form = new RenderForm("ShowHeightMap");
            form.SetBounds(0, 0, 1024, 768);

            form.MouseWheel += new System.Windows.Forms.MouseEventHandler(form_MouseWheel);
            form.MouseDown += new System.Windows.Forms.MouseEventHandler(form_MouseDown);
            form.MouseUp += new System.Windows.Forms.MouseEventHandler(form_MouseUp);
            form.MouseMove += new System.Windows.Forms.MouseEventHandler(form_MouseMove);

            form.KeyDown += new System.Windows.Forms.KeyEventHandler(form_KeyDown);
            form.KeyUp += new System.Windows.Forms.KeyEventHandler(form_KeyUp);

            // Creates the Device
            var direct3D = new Direct3D();
            device = new Device(direct3D, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing, new PresentParameters(form.ClientSize.Width, form.ClientSize.Height));

            // Load a PNG file
            Bitmap bmp = (Bitmap)Bitmap.FromFile(heightmap_file);
            x_dim = bmp.Width;
            z_dim = bmp.Height;
            heightmap = new float[x_dim * z_dim];
            markedmap = new bool[x_dim * z_dim];
            sizemap = new sizeXZ[x_dim * z_dim];
            for (int x = 0; x < x_dim; x++) {
                for (int y = 0; y < z_dim; y++) {
                    heightmap[y * x_dim + x] = (bmp.GetPixel(x, y).R - 128) / 256.0f;
                    markedmap[y * x_dim + x] = false;
                }
            }
            bmp.Dispose();

            // Creates the VertexBuffer
            xstep = (maxx - minx) / (float)x_dim;
            zstep = (maxz - minz) / (float)z_dim;
            HeightMapToVertices(true, false); // to get vertex_count
            HeightMapToVertices(true, true);

            // Compiles the effect
            var effect = Effect.FromFile(device, "Shader.fx", ShaderFlags.None);

            // Allocate Vertex Elements
            var vertexElems = new[] {
        		new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        		new VertexElement(0, 16, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Color, 0),
				VertexElement.VertexDeclarationEnd
        	};

            // Creates and sets the Vertex Declaration
            var vertexDecl = new VertexDeclaration(device, vertexElems);
            device.VertexDeclaration = vertexDecl;

            // Disable culling
            device.SetRenderState(RenderState.CullMode, Cull.None);

            // Get the technique
            var technique = effect.GetTechnique(0);
            var pass = effect.GetPass(technique, 0);
            
            // Prepare matrices
            var aspect = form.ClientSize.Width / (float)form.ClientSize.Height;
            var view = Matrix.LookAtLH(new Vector3(0, 5, 0), new Vector3(0, 0, 0), -Vector3.UnitZ);
            var proj = Matrix.OrthoLH(aspect * 2.0f, 2.0f, 0.1f, 100.0f);
            var viewProj = Matrix.Multiply(view, proj);

            // Use clock
            var clock = new Stopwatch();
            clock.Start();
            var frame_count = 0;
            var last_elapsed = clock.ElapsedMilliseconds;

            RenderLoop.Run(form, () =>
            {
                var time = clock.ElapsedMilliseconds / 1000.0f;
                var fps_time = (clock.ElapsedMilliseconds - last_elapsed) / 1000.0f;
                if ((fps_time < 0.1f) || (frame_count == 0)) {
                    frame_count++;
                } else {
                    form.Text = "ShowHeightMap - " + String.Format("{0:f} fps", frame_count / fps_time) + " - " + String.Format("{0:0,0} Dreiecke", triangle_count);
                    frame_count = 0;
                    last_elapsed = clock.ElapsedMilliseconds;
                }
                
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();

                effect.Technique = technique;
                effect.Begin();
                effect.BeginPass(0);

                var worldViewProj = Matrix.Translation(transX, 0.0f, transY) * Matrix.RotationX(rotX) * Matrix.RotationZ(rotY) * Matrix.Scaling(zoom) * viewProj;
                effect.SetValue("worldViewProj", worldViewProj);

                device.DrawPrimitives(PrimitiveType.TriangleList, 0, vertex_count / 6);

                effect.EndPass();
                effect.End();

                device.EndScene();
                device.Present();

            });

            effect.Dispose();
            vertices.Dispose();
            device.Dispose();
            direct3D.Dispose();
        }

        static void markDrill(float x, float z) {
            for (int hz = (int)(z - toolDm); hz <= z + toolDm; hz++) {
                for (int hx = (int)(x - toolDm); hx <= x + toolDm; hx++) {
                    if (Math.Abs(x - hx) * Math.Abs(x - hx) + Math.Abs(z - hz) * Math.Abs(z - hz) <= toolDm * toolDm) {
                        if ((hx > 0) && (hx < x_dim) && (hz > 0) && (hz < z_dim)) {
                            markedmap[hz * x_dim + hx] = true;
                        }
                    }
                }
            }
        }

        static void markLineDrill() {
            float dx = drillX2 - drillX1, dz = drillZ2 - drillZ1;
            float x = drillX1, z = drillZ1;
            float step;
            markDrill(x, z);
            if (Math.Abs(dx) > Math.Abs(dz)) { // Einerschritte in X-Richtung
                step = dx / Math.Abs(dx);
                for(;;) {
                    x += step;
                    if (step < 0) {
                        if (x < drillX2) break;
                    } else {
                        if (x > drillX2) break;
                    }
                    z += dz / Math.Abs(dx);
                    markDrill(x, z);
                }
            } else { // Einerschritte in Z-Richtung
                step = dz / Math.Abs(dz);
                for (;;) {
                    x += dx / Math.Abs(dx);
                    z += step;
                    if (step < 0) {
                        if (z < drillZ2) break;
                    } else {
                        if (z > drillZ2) break;
                    }
                    markDrill(x, z);
                }
            }
            HeightMapToVertices(true, false);
            HeightMapToVertices(true, true);
        }

        static void doDrill(float x, float z) {
            for (int hz = (int)(z - toolDm); hz <= z + toolDm; hz++) {
                for (int hx = (int)(x - toolDm); hx <= x + toolDm; hx++) {
                    if (Math.Abs(x - hx) * Math.Abs(x - hx) + Math.Abs(z - hz) * Math.Abs(z - hz) <= toolDm * toolDm) {
                        if ((hx > 0) && (hx < x_dim) && (hz > 0) && (hz < z_dim)) {
                            heightmap[hz * x_dim + hx] -= 0.01f;
                        }
                    }
                }
            }
        }

        static void doLineDrill() {
            float dx = drillX2 - drillX1, dz = drillZ2 - drillZ1;
            float x = drillX1, z = drillZ1;
            float step;
            doDrill(x, z);
            if (Math.Abs(dx) > Math.Abs(dz)) { // Einerschritte in X-Richtung
                step = dx / Math.Abs(dx);
                for (; ; ) {
                    x += step;
                    if (step < 0) {
                        if (x < drillX2) break;
                    } else {
                        if (x > drillX2) break;
                    }
                    z += dz / Math.Abs(dx);
                    doDrill(x, z);
                }
            } else { // Einerschritte in Z-Richtung
                step = dz / Math.Abs(dz);
                for (; ; ) {
                    x += dx / Math.Abs(dx);
                    z += step;
                    if (step < 0) {
                        if (z < drillZ2) break;
                    } else {
                        if (z > drillZ2) break;
                    }
                    doDrill(x, z);
                }
            }
            HeightMapToVertices(true, false);
            HeightMapToVertices(true, true);
        }

        static void form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == System.Windows.Forms.Keys.N) { // N: Reset view
                rotX = rotY = 0.0f;
                transX = transY = 0.0f;
                zoom = 1.0f;
            } else if (e.KeyCode == System.Windows.Forms.Keys.Space) { // Space: Mark random drill
                if (!drillKeyDown) {
                    float length;
                    do {
                        drillX1 = (float)r.NextDouble() * x_dim;
                        drillZ1 = (float)r.NextDouble() * z_dim;
                        drillX2 = (float)r.NextDouble() * x_dim;
                        drillZ2 = (float)r.NextDouble() * z_dim;
                        length = ((drillX2 - drillX1) * (drillX2 - drillX1) + (drillZ2 - drillZ1) * (drillZ2 - drillZ1));
                    } while (length < ((x_dim / 10.0f) * (x_dim / 10.0f)));
                    toolDm = 5.0f;
                    markLineDrill();
                    drillKeyDown = true;
                }
            }
        }

        static void form_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == System.Windows.Forms.Keys.Space) { // Space: Do random drill
                if (drillKeyDown) {
                    clearMarkedMap();
                    doLineDrill();
                    drillKeyDown = false;
                }
            }
        }

        static void form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if ((e.Button == System.Windows.Forms.MouseButtons.Left) || (e.Button == System.Windows.Forms.MouseButtons.Right)) {
                mouseDrag = true;
                dragButtonLeft = (e.Button == System.Windows.Forms.MouseButtons.Left);
                dragX = e.Location.X;
                dragY = e.Location.Y;
            }
        }

        static void form_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (mouseDrag && dragButtonLeft) {
                rotY += (e.Location.X - dragX) / (1024.0f / 2.0f);
                rotX += (e.Location.Y - dragY) / (768.0f / 2.0f);
                // Rotation begrenzen
                /*if (rotX > 0.0f) rotX = 0.0f;
                if (rotX < -(float)Math.PI / 2.0f) rotX = -(float)Math.PI / 2.0f;
                if (rotY > (float)Math.PI / 2.0f) rotY = (float)Math.PI / 2.0f;
                if (rotY < -(float)Math.PI / 2.0f) rotY = -(float)Math.PI / 2.0f;*/
                dragX = e.Location.X;
                dragY = e.Location.Y;
            } else if (mouseDrag && !dragButtonLeft) {
                transX += (dragX - e.Location.X) / (1024.0f / 2.0f);
                transY -= (dragY - e.Location.Y) / (768.0f / 2.0f);
                dragX = e.Location.X;
                dragY = e.Location.Y;
            }
        }

        static void form_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if ((e.Button == System.Windows.Forms.MouseButtons.Left) || (e.Button == System.Windows.Forms.MouseButtons.Right)) {
                mouseDrag = false;
            }
        }

        static void form_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Delta > 0) {
                zoom *= 1.2f;
                if (zoom > 10.0f) zoom = 10.0f;
            } else if (e.Delta < 0) {
                zoom /= 1.2f;
                if (zoom < 0.5f) zoom = 0.5f;
            }
        }
    }
}