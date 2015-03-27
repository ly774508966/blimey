﻿namespace Cor.Demo
{
    using System;
    using System.Text;
    using System.IO;
    using Abacus.SinglePrecision;
    using Fudge;
    using Cor;
    using System.Collections.Generic;
    using Platform;
    using System.Runtime.InteropServices;

    public class BasicApp : IApp
    {
        IElement[] elements;
        Rgba32 currentColour = Rgba32.Black;
        Rgba32 nextColour = Rgba32.DarkSlateBlue;
        readonly Single colourChangeTime = 10f;
        Single colourChangeProgress = 0f;
        Int32 numCols;
        Int32 numRows;
        Texture tex;
        Shader shader;


        VertPosCol[] testLines = {
            new VertPosCol (new Vector3 (-1, -1, 0.5f), Rgba32.Red),
            new VertPosCol (new Vector3 (1f, 1f, 0.5f), Rgba32.Yellow),
            new VertPosCol (new Vector3 (-1, 1, 0.5f), Rgba32.Blue),
            new VertPosCol (new Vector3 (1f, -1f, 0.5f), Rgba32.Green)
        };

        public void Start (Engine engine)
        {
            shader = ShaderHelper.CreateUnlit (engine);

            Int32 texSize = 256;
            Int32 gridSize = 4;
            Int32 squareSize = texSize / gridSize;

            var colours = new Rgba32 [gridSize*gridSize];

            for (Int32 x = 0; x < gridSize; ++x)
            {
                for (Int32 y = 0; y < gridSize; ++y)
                {
                    colours [x + (y * gridSize)] = RandomColours.GetNext ();
                }
            }

            var texData = new byte[texSize*texSize*4];

            Int32 index = 0;
            for (Int32 x = 0; x < texSize; ++x)
            {
                for (Int32 y = 0; y < texSize; ++y)
                {
                    texData [index++] = colours[(x/squareSize) + (y/squareSize*gridSize)].A;
                    texData [index++] = colours[(x/squareSize) + (y/squareSize*gridSize)].R;
                    texData [index++] = colours[(x/squareSize) + (y/squareSize*gridSize)].G;
                    texData [index++] = colours[(x/squareSize) + (y/squareSize*gridSize)].B;
                }
            }

            tex = engine.Graphics.CreateTexture (TextureFormat.Rgba32, texSize, texSize, texData );

            elements = new IElement[]
            {
                new Element <CubePosTex, VertPosTex> (shader, tex),
                new Element <CylinderPosTex, VertPosTex> (shader, tex),
                new Element <BillboardPosTexCol, VertPosTexCol> (shader, tex),
                new Element <BillboardPosTex, VertPosTex> (shader, tex),
                new Element <CylinderPosNormTex, VertPosNormTex> (shader, tex),
                new Element <CylinderNormTexPos, VertNormTexPos> (shader, tex),
                new Element <FlowerPosCol, VertPosCol> (shader, null),
                new Element <FlowerPos, VertPos> (shader, null),
            };

            Double s = Math.Sqrt (elements.Length);

            numCols = (Int32) Math.Ceiling (s);

            numRows = (Int32) Math.Floor (s);

            while (elements.Length > numCols * numRows) ++numRows;


            foreach (var element in elements) element.Load (engine);
        }

        public Boolean Update (Engine cor, AppTime time)
        {
            if (cor.Input.Keyboard.IsFunctionalKeyDown (FunctionalKey.Escape))
                return true;

            colourChangeProgress += time.Delta / colourChangeTime;

            if (colourChangeProgress >= 1f)
            {
                colourChangeProgress = 0f;
                currentColour = nextColour;
                nextColour = RandomColours.GetNext();
            }

            foreach (var element in elements)
                element.Update (cor, time);

            return false;
        }

        public void Render (Engine cor)
        {
            cor.Graphics.Reset ();
            cor.Graphics.ClearColourBuffer(Rgba32.Lerp (currentColour, nextColour, colourChangeProgress));
            cor.Graphics.ClearDepthBuffer(1f);

            var world = Matrix44.Identity;
            var view = Matrix44.CreateLookAt (Vector3.UnitZ, Vector3.Forward, Vector3.Up);
            var projection = Matrix44.CreateOrthographicOffCenter (-1f, 1f, -1f, 1f, 1f, -1f);
            shader.ResetVariables ();
            shader.ResetSamplers ();
            shader.SetVariable ("World", world);
            shader.SetVariable ("View", view);
            shader.SetVariable ("Projection", projection);
            shader.SetVariable ("Colour", Rgba32.White);
            shader.SetSamplerTarget ("TextureSampler", 0);
            cor.Graphics.SetActive (shader, testLines[0].VertexDeclaration);
            cor.Graphics.DrawUserPrimitives (PrimitiveType.LineList, testLines, 0, testLines.Length / 2);

            // grid index
            Int32 x = 0;
            Int32 y = numRows - 1;

            foreach (var element in elements)
            {
                Single left     = -1f - (x*2f);
                Single right    = -1f + (2f * numCols) - (x*2f);
                Single bottom   = -1f - (y*2f);
                Single top      = -1f + (2f * numRows) - (y*2f);

                Matrix44 proj = Matrix44.CreateOrthographicOffCenter (left, right, bottom, top, 1f, -1f);

                element.Render (cor, proj);

                if (++x >= numCols) {x = 0; --y;}
            }
        }

        public void Stop (Engine cor)
        {
            foreach (var element in elements)
                element.Unload ();
        }
    }

    public interface IElement
    {
        void Load (Engine engine);
        void Unload ();
        void Update(Engine engine, AppTime time);
        void Render (Engine engine, Matrix44 projection);

        Matrix44 World { get; }
        Matrix44 View { get; }
    }

    public sealed class Element <TMesh, TVertType> : IElement
        where TMesh
            : class
            , IMesh<TVertType>
            , new ()
        where TVertType
            : struct
            , IVertexType
    {
        readonly Shader shader;
        readonly Texture texture;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        public Rgba32 Colour { get; private set; }
        public Matrix44 World { get; private set; }
        public Matrix44 View { get; private set; }

        public Element (Shader shader, Texture texture)
        {
            this.shader = shader;
            this.texture = texture;
            this.Colour = Rgba32.LightGoldenrodYellow;
            View = Matrix44.CreateLookAt (Vector3.UnitZ, Vector3.Forward, Vector3.Up);
        }

        public void Load (Engine engine)
        {
            var meshResource = new TMesh ();

            // put the mesh resource onto the gpu
            vertexBuffer = engine.Graphics.CreateVertexBuffer (meshResource.VertexDeclaration, meshResource.VertArray.Length);
            indexBuffer = engine.Graphics.CreateIndexBuffer (meshResource.IndexArray.Length);

            vertexBuffer.SetData <TVertType>(meshResource.VertArray);
            indexBuffer.SetData(meshResource.IndexArray);

            // don't need a reference to the mesh now as it lives on the GPU
            meshResource = null;
        }

        public void Unload ()
        {
            indexBuffer.Dispose ();
            vertexBuffer.Dispose ();

            indexBuffer = null;
            vertexBuffer = null;
        }

        public void Update(Engine engine, AppTime time)
        {
            Matrix44 rotation =
                Matrix44.CreateFromAxisAngle(Vector3.Backward, Maths.Sin(time.Elapsed)) *
                Matrix44.CreateFromAxisAngle(Vector3.Left, Maths.Sin(time.Elapsed)) *
                Matrix44.CreateFromAxisAngle(Vector3.Down, Maths.Sin(time.Elapsed));

            Matrix44 worldScale = Matrix44.CreateScale (0.9f);

            World = worldScale * rotation;
        }

        public void Render (Engine engine, Matrix44 projection)
        {
            engine.Graphics.SetActive (vertexBuffer);
            engine.Graphics.SetActive (indexBuffer);
            engine.Graphics.SetActive (texture, 0);

            // set the variable on the shader to our desired variables
            shader.ResetSamplers ();
            shader.ResetVariables ();
            shader.SetVariable ("World", World);
            shader.SetVariable ("View", View);
            shader.SetVariable ("Projection", projection);
            shader.SetVariable ("Colour", Colour);
            shader.SetSamplerTarget ("TextureSampler", 0);

            engine.Graphics.SetActive (shader);

            engine.Graphics.DrawIndexedPrimitives (
                PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
        }
    }

    public interface IMesh<T> where T : IVertexType
    {
        VertexDeclaration VertexDeclaration { get; }
        T[] VertArray { get; }
        Int32[] IndexArray { get; }
    }

    public static class RandomColours
    {
        readonly static Random random = new Random();

        public static Rgba32 GetNext()
        {
            const Single min = 0.25f;
            const Single max = 1f;

            Single r = (Single)random.NextDouble() * (max - min) + min;
            Single g = (Single)random.NextDouble() * (max - min) + min;
            Single b = (Single)random.NextDouble() * (max - min) + min;
            Single a = 1f;

            return new Rgba32(r, g, b, a);
        }
    }

    #region Vertex Formats

    [StructLayout (LayoutKind.Sequential)]
    public struct VertPos : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertPos ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0));
        }

        public Vector3 Position;

        public VertPos (Vector3 position)
        {
            this.Position = position;
        }

        public VertexDeclaration VertexDeclaration { get { return _vertexDeclaration; } }
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct VertPosTexCol : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertPosTexCol ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),
                new VertexElement (
                    12,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0),
                new VertexElement (
                    20,
                    VertexElementFormat.Colour,
                    VertexElementUsage.Colour,
                    0));
        }

        public Vector3 Position;
        public Vector2 UV;
        public Rgba32 Colour;

        public VertPosTexCol (Vector3 position, Vector2 uv, Rgba32 colour)
        {
            this.Position = position;
            this.UV = uv;
            this.Colour = colour;
        }

        public VertexDeclaration VertexDeclaration { get { return _vertexDeclaration; } }
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct VertPosTex : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertPosTex ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),
                new VertexElement (
                    12,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0));
        }

        public Vector3 Position;
        public Vector2 UV;

        public VertPosTex (Vector3 position, Vector2 uv)
        {
            this.Position = position;
            this.UV = uv;
        }

        public VertexDeclaration VertexDeclaration { get { return _vertexDeclaration; } }
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct VertPosNormTex : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertPosNormTex ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),
                new VertexElement (
                    12,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Normal,
                    0),
                new VertexElement (
                    24,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0));
        }

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public VertPosNormTex (Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV = uv;
        }

        public VertexDeclaration VertexDeclaration { get { return _vertexDeclaration; } }
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct VertNormTexPos : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertNormTexPos ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Normal,
                    0),
                new VertexElement (
                    12,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0),
                new VertexElement (
                    20,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0));
        }

        public Vector3 Normal;
        public Vector2 UV;
        public Vector3 Position;

        public VertNormTexPos (Vector3 normal, Vector2 uv, Vector3 position)
        {
            this.Normal = normal;
            this.UV = uv;
            this.Position = position;
        }

        public VertexDeclaration VertexDeclaration { get { return _vertexDeclaration; } }
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct VertPosCol : IVertexType
    {
        readonly static VertexDeclaration _vertexDeclaration;

        static VertPosCol ()
        {
            _vertexDeclaration = new VertexDeclaration (
                new VertexElement (
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),
                new VertexElement (
                    12,
                    VertexElementFormat.Colour,
                    VertexElementUsage.Colour,
                    0)
            );
        }

        public Vector3 Position;
        public Rgba32 Colour;

        public VertPosCol (Vector3 position, Rgba32 colour)
        {
            this.Position = position;
            this.Colour = colour;
        }

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }
    }

    #endregion

    #region Meshs

    public class BillboardPosTexCol : IMesh <VertPosTexCol>
    {
        readonly VertPosTexCol[] vertArray = new VertPosTexCol[4];
        readonly Int32[] indexArray = new Int32[6];

        #region IMesh <VertPosTexCol>

        public VertPosTexCol[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        public BillboardPosTexCol()
        {
            // Six indices (two triangles) per face.
            indexArray[0] = 0;
            indexArray[1] = 3;
            indexArray[2] = 2;

            indexArray[3] = 0;
            indexArray[4] = 1;
            indexArray[5] = 3;

            // Four vertices per face.
            vertArray[0] = new VertPosTexCol(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0f, 0f), Rgba32.Yellow);
            vertArray[1] = new VertPosTexCol(new Vector3(-0.5f,  0.5f, 0f), new Vector2(0f, 1f), Rgba32.Green);
            vertArray[2] = new VertPosTexCol(new Vector3( 0.5f, -0.5f, 0f), new Vector2(1f, 0f), Rgba32.Blue);
            vertArray[3] = new VertPosTexCol(new Vector3( 0.5f,  0.5f, 0f), new Vector2(1f, 1f), Rgba32.Red);
        }
    }

    public class BillboardPosTex : IMesh <VertPosTex>
    {
        readonly VertPosTex[] vertArray = new VertPosTex[4];
        readonly Int32[] indexArray = new Int32[6];

        #region IMesh <VertPosTexCol>

        public VertPosTex[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        public BillboardPosTex()
        {
            // Six indices (two triangles) per face.
            indexArray[0] = 0;
            indexArray[1] = 3;
            indexArray[2] = 2;

            indexArray[3] = 0;
            indexArray[4] = 1;
            indexArray[5] = 3;

            // Four vertices per face.
            vertArray[0] = new VertPosTex(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0f, 0f));
            vertArray[1] = new VertPosTex(new Vector3(-0.5f,  0.5f, 0f), new Vector2(0f, 1f));
            vertArray[2] = new VertPosTex(new Vector3( 0.5f, -0.5f, 0f), new Vector2(1f, 0f));
            vertArray[3] = new VertPosTex(new Vector3( 0.5f,  0.5f, 0f), new Vector2(1f, 1f));
        }
    }

    public class FlowerPosCol : IMesh <VertPosCol>
    {
        readonly VertPosCol[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPosCol>

        public VertPosCol[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        public FlowerPosCol ()
        {
            vertArray = new[]
            {
                new VertPosCol( new Vector3(0.0f, 0.0f, 0.0f), RandomColours.GetNext() ),
                // Top
                new VertPosCol( new Vector3(-0.2f, 0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.2f, 0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.0f, 0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.0f, 1.0f, 0.0f), RandomColours.GetNext() ),
                // Bottom
                new VertPosCol( new Vector3(-0.2f, -0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.2f, -0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.0f, -0.8f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.0f, -1.0f, 0.0f), RandomColours.GetNext() ),
                // Left
                new VertPosCol( new Vector3(-0.8f, -0.2f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(-0.8f, 0.2f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(-0.8f, 0.0f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(-1.0f, 0.0f, 0.0f), RandomColours.GetNext() ),
                // Right
                new VertPosCol( new Vector3(0.8f, -0.2f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.8f, 0.2f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(0.8f, 0.0f, 0.0f), RandomColours.GetNext() ),
                new VertPosCol( new Vector3(1.0f, 0.0f, 0.0f), RandomColours.GetNext() ),
            };

            indexArray = new [] {
                // Top
                0, 1, 3, 0, 3, 2, 3, 1, 4, 3, 4, 2,
                // Bottom
                0, 7, 5, 0, 6, 7, 7, 8, 5, 7, 6, 8,
                // Left
                0, 9, 11, 0, 11, 10, 11, 9, 12, 11, 12, 10,
                // Right
                0, 15, 13, 0, 14, 15, 15, 16, 13, 15, 14, 16
            };
        }
    }

    public class FlowerPos : IMesh <VertPos>
    {
        readonly VertPos[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPos>

        public VertPos[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        public FlowerPos ()
        {
            vertArray = new[]
            {
                new VertPos( new Vector3(0.0f, 0.0f, 0.0f) ),
                // Top
                new VertPos( new Vector3(-0.2f, 0.8f, 0.0f) ),
                new VertPos( new Vector3(0.2f, 0.8f, 0.0f) ),
                new VertPos( new Vector3(0.0f, 0.8f, 0.0f) ),
                new VertPos( new Vector3(0.0f, 1.0f, 0.0f) ),
                // Bottom
                new VertPos( new Vector3(-0.2f, -0.8f, 0.0f) ),
                new VertPos( new Vector3(0.2f, -0.8f, 0.0f) ),
                new VertPos( new Vector3(0.0f, -0.8f, 0.0f) ),
                new VertPos( new Vector3(0.0f, -1.0f, 0.0f) ),
                // Left
                new VertPos( new Vector3(-0.8f, -0.2f, 0.0f) ),
                new VertPos( new Vector3(-0.8f, 0.2f, 0.0f) ),
                new VertPos( new Vector3(-0.8f, 0.0f, 0.0f) ),
                new VertPos( new Vector3(-1.0f, 0.0f, 0.0f) ),
                // Right
                new VertPos( new Vector3(0.8f, -0.2f, 0.0f) ),
                new VertPos( new Vector3(0.8f, 0.2f, 0.0f) ),
                new VertPos( new Vector3(0.8f, 0.0f, 0.0f) ),
                new VertPos( new Vector3(1.0f, 0.0f, 0.0f) ),
            };

            indexArray = new [] {
                // Top
                0, 1, 3, 0, 3, 2, 3, 1, 4, 3, 4, 2,
                // Bottom
                0, 7, 5, 0, 6, 7, 7, 8, 5, 7, 6, 8,
                // Left
                0, 9, 11, 0, 11, 10, 11, 9, 12, 11, 12, 10,
                // Right
                0, 15, 13, 0, 14, 15, 15, 16, 13, 15, 14, 16
            };
        }
    }

    public class CubePosTex : IMesh <VertPosTex>
    {
        readonly VertPosTex[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPosTex>

        public VertPosTex[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        public CubePosTex()
        {
            var normals = new []
            {
                new Vector3 (0, 0, 1),
                new Vector3 (0, 0, -1),
                new Vector3 (1, 0, 0),
                new Vector3 (-1, 0, 0),
                new Vector3 (0, 1, 0),
                new Vector3 (0, -1, 0),
            };

            var indexList = new List<Int32>();
            var vertList = new List<VertPosTex>();

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2;

                Vector3 n = normal;
                Vector3.Cross(ref n, ref side1, out side2);

                // Six indices (two triangles) per face.
                indexList.Add (vertList.Count + 0);
                indexList.Add (vertList.Count + 1);
                indexList.Add (vertList.Count + 2);

                indexList.Add (vertList.Count + 0);
                indexList.Add (vertList.Count + 2);
                indexList.Add (vertList.Count + 3);

                // Four vertices per face.
                vertList.Add(new VertPosTex((normal - side1 - side2) / 2f, /*normal,*/ new Vector2(0f, 0f)));
                vertList.Add(new VertPosTex((normal - side1 + side2) / 2f, /*normal,*/ new Vector2(1f, 0f)));
                vertList.Add(new VertPosTex((normal + side1 + side2) / 2f, /*normal,*/ new Vector2(1f, 1f)));
                vertList.Add(new VertPosTex((normal + side1 - side2) / 2f, /*normal,*/ new Vector2(0f, 1f)));
            }

            vertArray = vertList.ToArray ();
            indexArray = indexList.ToArray ();
        }
    }

    public class CylinderNormTexPos : IMesh <VertNormTexPos>
    {
        readonly VertNormTexPos[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPosNormTex>

        public VertNormTexPos[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        const int tessellation = 9; // must be greater han 2
        const float height = 0.5f;
        const float radius = 0.5f;

        public CylinderNormTexPos ()
        {
            var vertList = new List<VertNormTexPos>();
            var indexList = new List<Int32>();

            // Create a ring of triangles around the outside of the cylinder.
            for (Int32 i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i);

                Vector3 topPos = normal * radius + Vector3.Up * height;
                Vector3 botPos = normal * radius + Vector3.Down * height;

                Single howFarRound = (Single)i / (Single)(tessellation);

                Vector2 topUV = new Vector2(howFarRound * 3f, 0f);
                Vector2 botUV = new Vector2(howFarRound * 3f, 1f);

                vertList.Add(new VertNormTexPos(normal, topUV, topPos));
                vertList.Add(new VertNormTexPos(normal, botUV, botPos));
            }

            for (Int32 i = 0; i < tessellation; i++)
            {
                indexList.Add(i * 2);
                indexList.Add(i * 2 + 1);
                indexList.Add(i * 2 + 2);

                indexList.Add(i * 2 + 1);
                indexList.Add(i * 2 + 3);
                indexList.Add(i * 2 + 2);
            }


            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(vertList, indexList, Vector3.Up);
            CreateCap(vertList, indexList, Vector3.Down);

            vertArray = vertList.ToArray ();
            indexArray = indexList.ToArray ();
        }

        /// Helper method creates a triangle fan to close the ends of the cylinder.
        static void CreateCap(List<VertNormTexPos> vertList, List<Int32> indexList, Vector3 normal)
        {
            // Create cap indices.
            for (int i = 0; i < tessellation - 2; i++)
            {
                if (normal.Y > 0)
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                }
                else
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                }
            }

            // Create cap vertices.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 circleVec = GetCircleVector(i);
                Vector3 position = circleVec * radius +
                    normal * height;

                vertList.Add(
                    new VertNormTexPos(
                        normal,
                        new Vector2((circleVec.X + 1f) / 2f, (circleVec.Z + 1f) / 2f),
                        position));
            }
        }


        /// Helper method computes a point on a circle.
        static Vector3 GetCircleVector(int i)
        {
            Single tau; Maths.Tau(out tau);
            float angle = i * tau / tessellation;

            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }

    public class CylinderPosNormTex : IMesh <VertPosNormTex>
    {
        readonly VertPosNormTex[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPosNormTex>

        public VertPosNormTex[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        const int tessellation = 9; // must be greater han 2
        const float height = 0.5f;
        const float radius = 0.5f;

        public CylinderPosNormTex ()
        {
            var vertList = new List<VertPosNormTex>();
            var indexList = new List<Int32>();

            // Create a ring of triangles around the outside of the cylinder.
            for (Int32 i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i);

                Vector3 topPos = normal * radius + Vector3.Up * height;
                Vector3 botPos = normal * radius + Vector3.Down * height;

                Single howFarRound = (Single)i / (Single)(tessellation);

                Vector2 topUV = new Vector2(howFarRound * 3f, 0f);
                Vector2 botUV = new Vector2(howFarRound * 3f, 1f);

                vertList.Add(new VertPosNormTex(topPos, normal, topUV));
                vertList.Add(new VertPosNormTex(botPos, normal, botUV));
            }

            for (Int32 i = 0; i < tessellation; i++)
            {
                indexList.Add(i * 2);
                indexList.Add(i * 2 + 1);
                indexList.Add(i * 2 + 2);

                indexList.Add(i * 2 + 1);
                indexList.Add(i * 2 + 3);
                indexList.Add(i * 2 + 2);
            }


            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(vertList, indexList, Vector3.Up);
            CreateCap(vertList, indexList, Vector3.Down);

            vertArray = vertList.ToArray ();
            indexArray = indexList.ToArray ();
        }

        /// Helper method creates a triangle fan to close the ends of the cylinder.
        static void CreateCap(List<VertPosNormTex> vertList, List<Int32> indexList, Vector3 normal)
        {
            // Create cap indices.
            for (int i = 0; i < tessellation - 2; i++)
            {
                if (normal.Y > 0)
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                }
                else
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                }
            }

            // Create cap vertices.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 circleVec = GetCircleVector(i);
                Vector3 position = circleVec * radius +
                    normal * height;

                vertList.Add(
                    new VertPosNormTex(
                        position,
                        normal,
                        new Vector2((circleVec.X + 1f) / 2f, (circleVec.Z + 1f) / 2f)));
            }
        }


        /// Helper method computes a point on a circle.
        static Vector3 GetCircleVector(int i)
        {
            Single tau; Maths.Tau(out tau);
            float angle = i * tau / tessellation;

            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }

    public class CylinderPosTex : IMesh <VertPosTex>
    {
        readonly VertPosTex[] vertArray;
        readonly Int32[] indexArray;

        #region IMesh <VertPosTex>

        public VertPosTex[] VertArray { get { return vertArray; } }
        public Int32[] IndexArray { get { return indexArray; } }
        public VertexDeclaration VertexDeclaration { get { return vertArray [0].VertexDeclaration; } }

        #endregion

        const int tessellation = 9; // must be greater han 2
        const float height = 0.5f;
        const float radius = 0.5f;

        public CylinderPosTex ()
        {
            var vertList = new List<VertPosTex>();
            var indexList = new List<Int32>();

            // Create a ring of triangles around the outside of the cylinder.
            for (Int32 i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i);

                Vector3 topPos = normal * radius + Vector3.Up * height;
                Vector3 botPos = normal * radius + Vector3.Down * height;

                Single howFarRound = (Single)i / (Single)(tessellation);

                Vector2 topUV = new Vector2(howFarRound * 3f, 0f);
                Vector2 botUV = new Vector2(howFarRound * 3f, 1f);

                vertList.Add(new VertPosTex(topPos, topUV));
                vertList.Add(new VertPosTex(botPos, botUV));
            }

            for (Int32 i = 0; i < tessellation; i++)
            {
                indexList.Add(i * 2);
                indexList.Add(i * 2 + 1);
                indexList.Add((i * 2 + 2));

                indexList.Add(i * 2 + 1);
                indexList.Add(i * 2 + 3);
                indexList.Add(i * 2 + 2);
            }


            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(vertList, indexList, Vector3.Up);
            CreateCap(vertList, indexList, Vector3.Down);

            vertArray = vertList.ToArray ();
            indexArray = indexList.ToArray ();
        }

        /// Helper method creates a triangle fan to close the ends of the cylinder.
        static void CreateCap(List<VertPosTex> vertList, List<Int32> indexList, Vector3 normal)
        {
            // Create cap indices.
            for (int i = 0; i < tessellation - 2; i++)
            {
                if (normal.Y > 0)
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                }
                else
                {
                    indexList.Add(vertList.Count);
                    indexList.Add(vertList.Count + (i + 2) % tessellation);
                    indexList.Add(vertList.Count + (i + 1) % tessellation);
                }
            }

            // Create cap vertices.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 circleVec = GetCircleVector(i);
                Vector3 position = circleVec * radius +
                    normal * height;

                vertList.Add(
                    new VertPosTex(
                        position,
                        new Vector2((circleVec.X + 1f) / 2f, (circleVec.Z + 1f) / 2f)));
            }
        }


        /// Helper method computes a point on a circle.
        static Vector3 GetCircleVector(int i)
        {
            Single tau; Maths.Tau(out tau);
            float angle = i * tau / tessellation;

            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }

    #endregion

    #region Shader Definitions

    public static class ShaderHelper
    {
        static ShaderFormat GetShaderFormat ()
        {
#if COR_PLATFORM_MONOMAC
            return ShaderFormat.GLSL;
#elif COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            return ShaderFormat.GLSL_ES;
#elif COR_PLATFORM_XNA4
            return ShaderFormat.HLSL;
#endif
        }

        static ShaderDeclaration GetUnlitShaderDeclaration ()
        {
            return new ShaderDeclaration {
                Name = "Demo Unlit Shader",
                InputDeclarations = new List<ShaderInputDeclaration> {
                    new ShaderInputDeclaration {
                        Name = "a_vertPosition",
                        NiceName = "Position",
                        Optional = false,
                        Usage = VertexElementUsage.Position,
                        DefaultValue = Vector3.Zero
                    },
                    new ShaderInputDeclaration {
                        Name = "a_vertTexcoord",
                        NiceName = "TextureCoordinate",
                        Optional = true,
                        Usage = VertexElementUsage.TextureCoordinate,
                        DefaultValue = Vector2.Zero
                    },
                    new ShaderInputDeclaration {
                        Name = "a_vertColour",
                        NiceName = "Colour",
                        Optional = true,
                        Usage = VertexElementUsage.Colour,
                        DefaultValue = Rgba32.White
                    }
                },
                VariableDeclarations = new List<ShaderVariableDeclaration> {
                    new ShaderVariableDeclaration {
                        Name = "u_world",
                        NiceName = "World",
                        DefaultValue = Matrix44.Identity
                    },
                    new ShaderVariableDeclaration {
                        Name = "u_view",
                        NiceName = "View",
                        DefaultValue = Matrix44.Identity
                    },
                    new ShaderVariableDeclaration {
                        Name = "u_proj",
                        NiceName = "Projection",
                        DefaultValue = Matrix44.Identity
                    },
                    new ShaderVariableDeclaration {
                        Name = "u_colour",
                        NiceName = "Colour",
                        DefaultValue = Rgba32.White
                    }
                },
                SamplerDeclarations = new List<ShaderSamplerDeclaration> {
                    new ShaderSamplerDeclaration {
                        Name = "s_tex0",
                        NiceName = "TextureSampler",
                        Optional = true
                    }
                }
            };
        }

        static String ConvertToES (String source)
        {
            source = source.Replace ("float ", "mediump float ");
            source = source.Replace ("vec2 ", "mediump vec2 ");
            source = source.Replace ("vec3 ", "mediump vec3 ");
            source = source.Replace ("vec4 ", "mediump vec4 ");
            source = source.Replace ("mat4 ", "mediump mat4 ");
            source = source.Replace ("sampler2D ", "mediump sampler2D ");
            return source;
        }

        static Byte[] GetUnlit_VertPos ()
        {
            String source = "";
#if COR_PLATFORM_MONOMAC || COR_PLATFORM_XIOS || COR_PLATFORM_PSM
                source =
@"Vertex Position
=VSH=
attribute vec4 a_vertPosition;
uniform mat4 u_world;
uniform mat4 u_view;
uniform mat4 u_proj;
uniform vec4 u_colour;
varying vec4 v_tint;
void main()
{
    gl_Position = u_proj * u_view * u_world * a_vertPosition;
    v_tint = u_colour;
}
=FSH=
varying vec4 v_tint;
void main()
{
    gl_FragColor = v_tint;
}
";
#endif

#if COR_PLATFORM_XNA4_X86
            source = "";
#endif

#if COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source = ConvertToES (source);
#endif

            return Encoding.UTF8.GetBytes (source);
        }

        static Byte[] GetUnlit_VertPosTex ()
        {
            String source = "";

#if COR_PLATFORM_MONOMAC || COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source =
@"Vertex Position & Texture Coordinate
=VSH=
attribute vec4 a_vertPosition;
attribute vec2 a_vertTexcoord;
uniform mat4 u_world;
uniform mat4 u_view;
uniform mat4 u_proj;
uniform vec4 u_colour;
varying vec2 v_texCoord;
varying vec4 v_tint;
void main()
{
    gl_Position = u_proj * u_view * u_world * a_vertPosition;
    v_texCoord = a_vertTexcoord;
    v_tint = u_colour;
}
=FSH=
uniform sampler2D s_tex0;
varying vec2 v_texCoord;
varying vec4 v_tint;
void main()
{
    gl_FragColor = v_tint * texture2D(s_tex0, v_texCoord);
}
";
#endif

#if COR_PLATFORM_XNA4_X86
            source = "";
#endif

#if COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source = ConvertToES (source);
#endif

            return Encoding.UTF8.GetBytes (source);
        }

        static Byte[] GetUnlit_VertPosCol ()
        {
            String source = "";
#if COR_PLATFORM_MONOMAC || COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source =
@"Vertex Position & Colour
=VSH=
attribute vec4 a_vertPosition;
attribute vec4 a_vertColour;
uniform mat4 u_world;
uniform mat4 u_view;
uniform mat4 u_proj;
uniform vec4 u_colour;
varying vec4 v_tint;
void main()
{
    gl_Position = u_proj * u_view * u_world * a_vertPosition;
    v_tint = a_vertColour * u_colour;
}
=FSH=
varying vec4 v_tint;
void main()
{
    gl_FragColor = v_tint;
}
";
#endif

#if COR_PLATFORM_XNA4_X86
            source = "";
#endif

#if COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source = ConvertToES (source);
#endif

            return Encoding.UTF8.GetBytes (source);
        }

        static Byte[] GetUnlit_VertPosTexCol ()
        {
            String source = "";
#if COR_PLATFORM_MONOMAC || COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source =
@"Vertex Position, Texture Coordinate & Colour
=VSH=
attribute vec4 a_vertPosition;
attribute vec2 a_vertTexcoord;
attribute vec4 a_vertColour;
uniform mat4 u_world;
uniform mat4 u_view;
uniform mat4 u_proj;
uniform vec4 u_colour;
varying vec2 v_texCoord;
varying vec4 v_tint;
void main()
{
    gl_Position = u_proj * u_view * u_world * a_vertPosition;
    v_texCoord = a_vertTexcoord;
    v_tint = a_vertColour * u_colour;
}
=FSH=
uniform sampler2D s_tex0;
varying vec2 v_texCoord;
varying vec4 v_tint;
void main()
{
    gl_FragColor = v_tint * texture2D(s_tex0, v_texCoord);
}
";
#endif

#if COR_PLATFORM_XNA4_X86
           source = "";
#endif

#if COR_PLATFORM_XIOS || COR_PLATFORM_PSM
            source = ConvertToES (source);
#endif
            return Encoding.UTF8.GetBytes (source);
        }

        static Byte[] GetUnlitShaderSource ()
        {
            var encodedVariants = new [] {
                GetUnlit_VertPos (),
                GetUnlit_VertPosTex (),
                GetUnlit_VertPosCol (),
                GetUnlit_VertPosTexCol ()
            };

            using (var mem = new MemoryStream ())
            {
                using (var bin = new BinaryWriter (mem))
                {
                    bin.Write ((Byte)encodedVariants.Length);
                    foreach (var variant in encodedVariants)
                    {
                        bin.Write (variant.Length);
                        bin.Write (variant);
                    }
                }
                return mem.GetBuffer ();
            }

        }

        public static Shader CreateUnlit (Engine engine)
        {
            return engine.Graphics.CreateShader (
                GetUnlitShaderDeclaration (),
                GetShaderFormat (),
                GetUnlitShaderSource ()
            );
        }
    }

    #endregion
}
