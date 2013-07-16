// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ Blimey - Fast, efficient, high level engine built upon Cor & Abacus    │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Brought to you by:                                                     │ \\
// │          _________                    .__               __             │ \\
// │         /   _____/__ __  ____    ____ |__|____    _____/  |_           │ \\
// │         \_____  \|  |  \/    \  / ___\|  \__  \  /    \   __\          │ \\
// │         /        \  |  /   |  \/ /_/  >  |/ __ \|   |  \  |            │ \\
// │        /_______  /____/|___|  /\___  /|__(____  /___|  /__|            │ \\
// │                \/           \//_____/         \/     \/                │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2013 A.J.Pook (http://sungiant.github.com)                 │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Permission is hereby granted, free of charge, to any person obtaining  │ \\
// │ a copy of this software and associated documentation files (the        │ \\
// │ "Software"), to deal in the Software without restriction, including    │ \\
// │ without limitation the rights to use, copy, modify, merge, publish,    │ \\
// │ distribute, sublicense, and/or sellcopies of the Software, and to      │ \\
// │ permit persons to whom the Software is furnished to do so, subject to  │ \\
// │ the following conditions:                                              │ \\
// │                                                                        │ \\
// │ The above copyright notice and this permission notice shall be         │ \\
// │ included in all copies or substantial portions of the Software.        │ \\
// │                                                                        │ \\
// │ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        │ \\
// │ EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     │ \\
// │ MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. │ \\
// │ IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   │ \\
// │ CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   │ \\
// │ TORT OR OTHERWISE, ARISING FROM,OUT OF OR IN CONNECTION WITH THE       │ \\
// │ SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 │ \\
// └────────────────────────────────────────────────────────────────────────┘ \\

using System;
using System.Collections.Generic;
using Sungiant.Abacus;
using Sungiant.Abacus.Packed;
using Sungiant.Abacus.SinglePrecision;
using Sungiant.Cor;
using Sungiant.Abacus.Int32Precision;
using System.Linq;

namespace Sungiant.Blimey
{

	/// <summary>
	/// A system for handling rendering of various debug shapes.
	/// </summary>
	/// <remarks>
	/// The DebugShapeRenderer allows for rendering line-base shapes in a batched fashion. Games
	/// will call one of the many Add* methods to add a shape to the renderer and then a call to
	/// Draw will cause all shapes to be rendered. This mechanism was chosen because it allows
	/// game code to call the Add* methods wherever is most convenient, rather than having to
	/// add draw methods to all of the necessary objects.
	/// 
	/// Additionally the renderer supports a lifetime for all shapes added. This allows for things
	/// like visualization of raycast bullets. The game would call the AddLine overload with the
	/// lifetime parameter and pass in a positive value. The renderer will then draw that shape
	/// for the given amount of time without any more calls to AddLine being required.
	/// 
	/// The renderer's batching mechanism uses a cache system to avoid garbage and also draws as
	/// many lines in one call to DrawUserPrimitives as possible. If the renderer is trying to draw
	/// more lines than are allowed in the Reach profile, it will break them up into multiple draw
	/// calls to make sure the game continues to work for any game.</remarks>
	public class DebugShapeRenderer
	{

		public int NumActiveShapes { get { return activeShapes.Count; } }

		// A single shape in our debug renderer
		class DebugShape
		{
			public string RenderPass = "Default";

			/// <summary>
			/// The array of vertices the shape can use.
			/// </summary>
			public VertexPositionColour[] Vertices;

			/// <summary>
			/// The number of lines to draw for this shape.
			/// </summary>
			public int LineCount;

			/// <summary>
			/// The length of time to keep this shape visible.
			/// </summary>
			public float Lifetime;
		}
		
		// We use a cache system to reuse our DebugShape instances to avoid creating garbage
		readonly List<DebugShape> cachedShapes = new List<DebugShape>();
		readonly List<DebugShape> activeShapes = new List<DebugShape>();

		// Allocate an array to hold our vertices; this will grow as needed by our renderer
		VertexPositionColour[] verts = new VertexPositionColour[64];

		// An array we use to get corners from frustums and bounding boxes
		Vector3[] corners = new Vector3[8];

		// This holds the vertices for our unit sphere that we will use when drawing bounding spheres
		const int sphereResolution = 30;
		const int sphereLineCount = (sphereResolution + 1) * 3;
		Vector3[] unitSphere;

		Dictionary<string, Material> materials = new Dictionary<string, Material>();

		ICor cor;

		public DebugShapeRenderer(ICor cor, List<string> renderPasses)
		{
			this.cor = cor;
			var shader = cor.Resources.LoadShader(ShaderType.Unlit );

			foreach (string pass in renderPasses)
			{
				materials[pass] = new Material(pass, shader);
			}

			// Create our unit sphere vertices
			InitializeSphere();
		}

		public void AddQuad(string renderPass, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Rgba32 rgba)
		{
			AddQuad (renderPass, a, b, c, d, rgba, 0f);
		}

		public void AddQuad(string renderPass, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Rgba32 rgba, float life)
		{
			AddLine(renderPass, a, b, rgba, life);
			AddLine(renderPass, b, c, rgba, life);
			AddLine(renderPass, c, d, rgba, life);
			AddLine(renderPass, d, a, rgba, life);
		}

		
		public void AddLine(string renderPass, Vector3 a, Vector3 b, Rgba32 rgba)
		{
			AddLine(renderPass, a, b, rgba, 0f);
		}


		public void AddLine(string renderPass, Vector3 a, Vector3 b, Rgba32 rgba, float life)
		{
			// Get a DebugShape we can use to draw the line
			DebugShape shape = GetShapeForLines(1, life);
			shape.RenderPass = renderPass;

			// Add the two vertices to the shape
			shape.Vertices[0] = new VertexPositionColour(a, rgba);
			shape.Vertices[1] = new VertexPositionColour(b, rgba);
		}

		public void AddTriangle(string renderPass, Vector3 a, Vector3 b, Vector3 c, Rgba32 rgba)
		{
			AddTriangle(renderPass, a, b, c, rgba, 0f);
		}

		public void AddTriangle(string renderPass, Vector3 a, Vector3 b, Vector3 c, Rgba32 rgba, float life)
		{
			// Get a DebugShape we can use to draw the triangle
			DebugShape shape = GetShapeForLines(3, life);
			shape.RenderPass = renderPass;

			// Add the vertices to the shape
			shape.Vertices[0] = new VertexPositionColour(a, rgba);
			shape.Vertices[1] = new VertexPositionColour(b, rgba);
			shape.Vertices[2] = new VertexPositionColour(b, rgba);
			shape.Vertices[3] = new VertexPositionColour(c, rgba);
			shape.Vertices[4] = new VertexPositionColour(c, rgba);
			shape.Vertices[5] = new VertexPositionColour(a, rgba);
		}
		/*
		public void AddBoundingFrustum(string renderPass, BoundingFrustum frustum, Rgba32 rgba)
		{
			AddBoundingFrustum(renderPass, frustum, rgba, 0f);
		}

		public void AddBoundingFrustum(string renderPass, BoundingFrustum frustum, Rgba32 rgba, float life)
		{
			// Get a DebugShape we can use to draw the frustum
			DebugShape shape = GetShapeForLines(12, life);
			shape.RenderPass = renderPass;

			// Get the corners of the frustum
            corners = frustum.GetCorners();

			// Fill in the vertices for the bottom of the frustum
			shape.Vertices[0] = new VertexPositionColour(corners[0], rgba);
			shape.Vertices[1] = new VertexPositionColour(corners[1], rgba);
			shape.Vertices[2] = new VertexPositionColour(corners[1], rgba);
			shape.Vertices[3] = new VertexPositionColour(corners[2], rgba);
			shape.Vertices[4] = new VertexPositionColour(corners[2], rgba);
			shape.Vertices[5] = new VertexPositionColour(corners[3], rgba);
			shape.Vertices[6] = new VertexPositionColour(corners[3], rgba);
			shape.Vertices[7] = new VertexPositionColour(corners[0], rgba);

			// Fill in the vertices for the top of the frustum
			shape.Vertices[8] = new VertexPositionColour(corners[4], rgba);
			shape.Vertices[9] = new VertexPositionColour(corners[5], rgba);
			shape.Vertices[10] = new VertexPositionColour(corners[5], rgba);
			shape.Vertices[11] = new VertexPositionColour(corners[6], rgba);
			shape.Vertices[12] = new VertexPositionColour(corners[6], rgba);
			shape.Vertices[13] = new VertexPositionColour(corners[7], rgba);
			shape.Vertices[14] = new VertexPositionColour(corners[7], rgba);
			shape.Vertices[15] = new VertexPositionColour(corners[4], rgba);

			// Fill in the vertices for the vertical sides of the frustum
			shape.Vertices[16] = new VertexPositionColour(corners[0], rgba);
			shape.Vertices[17] = new VertexPositionColour(corners[4], rgba);
			shape.Vertices[18] = new VertexPositionColour(corners[1], rgba);
			shape.Vertices[19] = new VertexPositionColour(corners[5], rgba);
			shape.Vertices[20] = new VertexPositionColour(corners[2], rgba);
			shape.Vertices[21] = new VertexPositionColour(corners[6], rgba);
			shape.Vertices[22] = new VertexPositionColour(corners[3], rgba);
			shape.Vertices[23] = new VertexPositionColour(corners[7], rgba);
		}

		public void AddBoundingBox(string renderPass, BoundingBox box, Rgba32 col)
		{
			AddBoundingBox(renderPass, box, col, 0f);
		}

		public void AddBoundingBox(string renderPass, BoundingBox box, Rgba32 col, float life)
		{
			// Get a DebugShape we can use to draw the box
			DebugShape shape = GetShapeForLines(12, life);
			shape.RenderPass = renderPass;

			// Get the corners of the box
            corners = box.GetCorners();

			// Fill in the vertices for the bottom of the box
			shape.Vertices[0] = new VertexPositionColour(corners[0], col);
			shape.Vertices[1] = new VertexPositionColour(corners[1], col);
			shape.Vertices[2] = new VertexPositionColour(corners[1], col);
			shape.Vertices[3] = new VertexPositionColour(corners[2], col);
			shape.Vertices[4] = new VertexPositionColour(corners[2], col);
			shape.Vertices[5] = new VertexPositionColour(corners[3], col);
			shape.Vertices[6] = new VertexPositionColour(corners[3], col);
			shape.Vertices[7] = new VertexPositionColour(corners[0], col);

			// Fill in the vertices for the top of the box
			shape.Vertices[8] = new VertexPositionColour(corners[4], col);
			shape.Vertices[9] = new VertexPositionColour(corners[5], col);
			shape.Vertices[10] = new VertexPositionColour(corners[5], col);
			shape.Vertices[11] = new VertexPositionColour(corners[6], col);
			shape.Vertices[12] = new VertexPositionColour(corners[6], col);
			shape.Vertices[13] = new VertexPositionColour(corners[7], col);
			shape.Vertices[14] = new VertexPositionColour(corners[7], col);
			shape.Vertices[15] = new VertexPositionColour(corners[4], col);

			// Fill in the vertices for the vertical sides of the box
			shape.Vertices[16] = new VertexPositionColour(corners[0], col);
			shape.Vertices[17] = new VertexPositionColour(corners[4], col);
			shape.Vertices[18] = new VertexPositionColour(corners[1], col);
			shape.Vertices[19] = new VertexPositionColour(corners[5], col);
			shape.Vertices[20] = new VertexPositionColour(corners[2], col);
			shape.Vertices[21] = new VertexPositionColour(corners[6], col);
			shape.Vertices[22] = new VertexPositionColour(corners[3], col);
			shape.Vertices[23] = new VertexPositionColour(corners[7], col);
		}

		public void AddBoundingSphere(string renderPass, BoundingSphere sphere, Rgba32 col)
		{
			AddBoundingSphere(renderPass, sphere, col, 0f);
		}

		public void AddBoundingSphere(string renderPass, BoundingSphere sphere, Rgba32 col, float life)
		{
			// Get a DebugShape we can use to draw the sphere
			DebugShape shape = GetShapeForLines(sphereLineCount, life);
			shape.RenderPass = renderPass;

			// Iterate our unit sphere vertices
			for (int i = 0; i < unitSphere.Length; i++)
			{
				// Compute the vertex position by transforming the point by the radius and center of the sphere
				Vector3 vertPos = unitSphere[i] * sphere.Radius + sphere.Center;

				// Add the vertex to the shape
				shape.Vertices[i] = new VertexPositionColour(vertPos, col);
			}
		}
		*/
        /*
		public void AddRect(string renderPass, Rectangle rect, Single z, Rgba32 colour, Single life = 0f)
		{
			Int32 width = this.cor.Graphics.DisplayStatus.CurrentWidth;
			Int32 height = this.cor.Graphics.DisplayStatus.CurrentHeight;

			this.cor.System.GetEffectiveDisplaySize(ref width, ref height);

			Single l = (Single) rect.Left / (Single) width;
			Single r = (Single) rect.Right / (Single) width;
			Single t = (Single) rect.Top / (Single) height;
			Single b = (Single) rect.Bottom / (Single) height;

			this.AddLine(
				renderPass,
				new Vector3(l, t, z),
				new Vector3(r, t, z),
				colour,
				life );

			this.AddLine(
				renderPass,
				new Vector3(l, b, z),
				new Vector3(r, b, z),
				colour,
				life );

			this.AddLine(
				renderPass,
				new Vector3(l, b, z),
				new Vector3(l, t, z),
				colour,
				life );

			this.AddLine(
				renderPass,
				new Vector3(r, b, z),
				new Vector3(r, t, z),
				colour,
				life );


		}*/
		
		internal void Update(AppTime time)
		{
			// Go through our active shapes and retire any shapes that have expired to the
			// cache list. 
			Boolean resort = false;
			for (int i = this.activeShapes.Count - 1; i >= 0; i--)
			{
				DebugShape s = activeShapes[i];

				if (s.Lifetime < 0)
				{
					this.cachedShapes.Add(s);
					this.activeShapes.RemoveAt(i);
					resort = true;
				}
				else
				{
					s.Lifetime -= time.Delta;
				}
			}

			// If we move any shapes around, we need to resort the cached list
			// to ensure that the smallest shapes are first in the list.
			if (resort)
				this.cachedShapes.Sort(CachedShapesSort);
		}
		

		internal void Render(IGraphicsManager zGfx, string pass, Matrix44 zView, Matrix44 zProjection)
		{
			if (!materials.ContainsKey(pass))
				return;

			var material = materials[pass];

			if( material == null )
				return;

			// Update our effect with the matrices.
			material.CalibrateShader (
				Matrix44.Identity,
				zView,
				zProjection
				);

			
			BlendMode.Apply (BlendMode.Default, zGfx);

			var shader = material.GetShader ();

			if( shader == null )
				return;

			zGfx.GpuUtils.BeginEvent(Rgba32.Red, "DebugRenderer.Render");

			var shapesForThisPass = this.activeShapes.Where(e => pass == e.RenderPass);

			// Calculate the total number of vertices we're going to be rendering.
			int vertexCount = 0;
			foreach (var shape in shapesForThisPass)
				vertexCount += shape.LineCount * 2;

			// If we have some vertices to draw
			if (vertexCount > 0)
			{
				// Make sure our array is large enough
				if (verts.Length < vertexCount)
				{
					// If we have to resize, we make our array twice as large as necessary so
					// we hopefully won't have to resize it for a while.
					verts = new VertexPositionColour[vertexCount * 2];
				}

				// Now go through the shapes again to move the vertices to our array and
				// add up the number of lines to draw.
				int lineCount = 0;
				int vertIndex = 0;
				foreach (DebugShape shape in shapesForThisPass)
				{
					lineCount += shape.LineCount;
					int shapeVerts = shape.LineCount * 2;
					for (int i = 0; i < shapeVerts; i++)
						verts[vertIndex++] = shape.Vertices[i];
				}

				// Start our effect to begin rendering.
				foreach (IShaderPass effectPass in shader.Passes)
				{
					effectPass.Activate (VertexPositionColour.Default.VertexDeclaration);

					// We draw in a loop because the Reach profile only supports 65,535 primitives. While it's
					// not incredibly likely, if a game tries to render more than 65,535 lines we don't want to
					// crash. We handle this by doing a loop and drawing as many lines as we can at a time, capped
					// at our limit. We then move ahead in our vertex array and draw the next set of lines.
					int vertexOffset = 0;
					while (lineCount > 0)
					{
						// Figure out how many lines we're going to draw
						int linesToDraw = Math.Min(lineCount, 65535);

						zGfx.DrawUserPrimitives(
							PrimitiveType.LineList, 
							verts,
							vertexOffset,
							linesToDraw,
							VertexPositionColour.Default.VertexDeclaration
							);
	
						// Move our vertex offset ahead based on the lines we drew
						vertexOffset += linesToDraw * 2;
	
						// Remove these lines from our total line count
						lineCount -= linesToDraw;
					}
				}
				
				zGfx.GpuUtils.EndEvent();
			}
		}
		

		void InitializeSphere()
		{
			// We need two vertices per line, so we can allocate our vertices
			unitSphere = new Vector3[sphereLineCount * 2];

			float tau; RealMaths.Tau(out tau);

			// Compute our step around each circle
			float step = tau / sphereResolution;

			// Used to track the index into our vertex array
			int index = 0;

			// Create the loop on the XY plane first
			for (float a = 0f; a < tau; a += step)
			{
				unitSphere[index++] = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f);
				unitSphere[index++] = new Vector3((float)Math.Cos(a + step), (float)Math.Sin(a + step), 0f);
			}

			// Next on the XZ plane
			for (float a = 0f; a < tau; a += step)
			{
				unitSphere[index++] = new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a));
				unitSphere[index++] = new Vector3((float)Math.Cos(a + step), 0f, (float)Math.Sin(a + step));
			}

			// Finally on the YZ plane
			for (float a = 0f; a < tau; a += step)
			{
				unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a));
				unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a + step), (float)Math.Sin(a + step));
			}
		}

		static int CachedShapesSort(DebugShape s1, DebugShape s2)
		{
			return s1.Vertices.Length.CompareTo(s2.Vertices.Length);
		}

		DebugShape GetShapeForLines(int lineCount, float life)
		{
			DebugShape shape = null;

			// We go through our cached list trying to find a shape that contains
			// a large enough array to hold our desired line count. If we find such
			// a shape, we move it from our cached list to our active list and break
			// out of the loop.
			int vertCount = lineCount * 2;
			for (int i = 0; i < cachedShapes.Count; i++)
			{
				if (cachedShapes[i].Vertices.Length >= vertCount)
				{
					shape = cachedShapes[i];
					cachedShapes.RemoveAt(i);
					activeShapes.Add(shape);
					break;
				}
			}

			// If we didn't find a shape in our cache, we create a new shape and add it
			// to the active list.
			if (shape == null)
			{
				shape = new DebugShape { Vertices = new VertexPositionColour[vertCount] };
				activeShapes.Add(shape);
			}

			// Set the line count and lifetime of the shape based on our parameters.
			shape.LineCount = lineCount;
			shape.Lifetime = life;

			return shape;
		}
	}
}
