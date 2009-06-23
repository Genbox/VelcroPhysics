/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;
using Tao.FreeGlut;
using ISE;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	using Box2DXMath = Box2DX.Common.Math;
	using SysMath = System.Math;

	// This class implements debug drawing callbacks that are invoked
	// inside World.Step.
	public class OpenGLDebugDraw : DebugDraw
	{
		public override void DrawPolygon(Vec2[] vertices, int vertexCount, Color color)
		{
			Gl.glColor3f(color.R, color.G, color.B);
			Gl.glBegin(Gl.GL_LINE_LOOP);
			for (int i = 0; i < vertexCount; ++i)
			{
				Gl.glVertex2f(vertices[i].X, vertices[i].Y);
			}
			Gl.glEnd();			
		}

		public override void DrawSolidPolygon(Vec2[] vertices, int vertexCount, Color color)
		{
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glColor4f(0.5f * color.R, 0.5f * color.G, 0.5f * color.B, 0.5f);
			Gl.glBegin(Gl.GL_TRIANGLE_FAN);
			for (int i = 0; i < vertexCount; ++i)
			{
				Gl.glVertex2f(vertices[i].X, vertices[i].Y);
			}
			Gl.glEnd();
			Gl.glDisable(Gl.GL_BLEND);

			Gl.glColor4f(color.R, color.G, color.B, 1.0f);
			Gl.glBegin(Gl.GL_LINE_LOOP);
			for (int i = 0; i < vertexCount; ++i)
			{
				Gl.glVertex2f(vertices[i].X, vertices[i].Y);
			}
			Gl.glEnd();
		}

		public override void DrawCircle(Vec2 center, float radius, Color color)
		{
			float k_segments = 16.0f;
			float k_increment = 2.0f * Box2DX.Common.Settings.Pi / k_segments;
			float theta = 0.0f;
			Gl.glColor3f(color.R, color.G, color.B);
			Gl.glBegin(Gl.GL_LINE_LOOP);
			for (int i = 0; i < k_segments; ++i)
			{
				Vec2 v = center + radius * new Vec2((float)SysMath.Cos(theta), (float)SysMath.Sin(theta));
				Gl.glVertex2f(v.X, v.Y);
				theta += k_increment;
			}
			Gl.glEnd();
		}

		public override void DrawSolidCircle(Vec2 center, float radius, Vec2 axis, Color color)
		{
			float k_segments = 16.0f;
			float k_increment = 2.0f * Box2DX.Common.Settings.Pi / k_segments;
			float theta = 0.0f;
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glColor4f(0.5f * color.R, 0.5f * color.G, 0.5f * color.B, 0.5f);
			Gl.glBegin(Gl.GL_TRIANGLE_FAN);
			for (int i = 0; i < k_segments; ++i)
			{
				Vec2 v = center + radius * new Vec2((float)SysMath.Cos(theta), (float)SysMath.Sin(theta));
				Gl.glVertex2f(v.X, v.Y);
				theta += k_increment;
			}
			Gl.glEnd();
			Gl.glDisable(Gl.GL_BLEND);

			theta = 0.0f;
			Gl.glColor4f(color.R, color.G, color.B, 1.0f);
			Gl.glBegin(Gl.GL_LINE_LOOP);
			for (int i = 0; i < k_segments; ++i)
			{
				Vec2 v = center + radius * new Vec2((float)SysMath.Cos(theta), (float)SysMath.Sin(theta));
				Gl.glVertex2f(v.X, v.Y);
				theta += k_increment;
			}
			Gl.glEnd();

			Vec2 p = center + radius * axis;
			Gl.glBegin(Gl.GL_LINES);
			Gl.glVertex2f(center.X, center.Y);
			Gl.glVertex2f(p.X, p.Y);
			Gl.glEnd();
		}

		public override void DrawSegment(Vec2 p1, Vec2 p2, Color color)
		{
			Gl.glColor3f(color.R, color.G, color.B);
			Gl.glBegin(Gl.GL_LINES);
			Gl.glVertex2f(p1.X, p1.Y);
			Gl.glVertex2f(p2.X, p2.Y);
			Gl.glEnd();
		}

		public override void DrawXForm(XForm xf)
		{
			Vec2 p1 = xf.Position, p2;
			float k_axisScale = 0.4f;
			Gl.glBegin(Gl.GL_LINES);

			Gl.glColor3f(1.0f, 0.0f, 0.0f);
			Gl.glVertex2f(p1.X, p1.Y);
			p2 = p1 + k_axisScale * xf.R.Col1;
			Gl.glVertex2f(p2.X, p2.Y);

			Gl.glColor3f(0.0f, 1.0f, 0.0f);
			Gl.glVertex2f(p1.X, p1.Y);
			p2 = p1 + k_axisScale * xf.R.Col2;
			Gl.glVertex2f(p2.X, p2.Y);

			Gl.glEnd();
		}

		public static void DrawSegment(Vec2 p1, Vec2 p2, Color color, params object[] p)
		{
			Gl.glColor3f(color.R, color.G, color.B);
			Gl.glBegin(Gl.GL_LINES);
			Gl.glVertex2f(p1.X, p1.Y);
			Gl.glVertex2f(p2.X, p2.Y);
			Gl.glEnd();
		}

		public static void DrawPoint(Vec2 p, float size, Color color)
		{
			Gl.glPointSize(size);
			Gl.glBegin(Gl.GL_POINTS);
			Gl.glColor3f(color.R, color.G, color.B);
			Gl.glVertex2f(p.X, p.Y);
			Gl.glEnd();
			Gl.glPointSize(1.0f);
		}

		static FTFont sysfont;

		static Tao.Platform.Windows.SimpleOpenGlControl openGlControl;
		private static bool sIsTextRendererInitialized = false;
		public static void InitTextRenderer(Tao.Platform.Windows.SimpleOpenGlControl openGlCtrl)
		{
			openGlControl = openGlCtrl;

			try
			{
				int Errors = 0;
				// CREATE FONT
				sysfont = new FTFont("FreeSans.ttf", out Errors);
				// INITIALISE FONT AS A PER_CHARACTER TEXTURE MAPPED FONT
				sysfont.ftRenderToTexture(12, 196);
				// SET the sample font to align CENTERED
				sysfont.FT_ALIGN = FTFontAlign.FT_ALIGN_LEFT;
				sIsTextRendererInitialized = true;
			}
			catch (Exception)
			{
				sIsTextRendererInitialized = false;
			}
		}

		public static void DrawString(int x, int y, string str)
		{
			if (sIsTextRendererInitialized)
			{
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();

				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();

				float xOffset = -0.95f + (float)x / ((float)openGlControl.Width / 2f);
				float yOffset = 0.95f - (float)y / ((float)openGlControl.Height / 2f);
				// Offset the font on the screen
				Gl.glTranslatef(xOffset, yOffset, 0);

				Gl.glColor3f(0.9f, 0.6f, 0.6f);
				// Scale the font
				Gl.glScalef(0.0035f, 0.0035f, 0.0035f);

				// Begin writing the font
				sysfont.ftBeginFont();
				sysfont.ftWrite(str);
				// Stop writing the font and restore old OpenGL parameters
				sysfont.ftEndFont();

				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
			}
		}

		public static void DrawAABB(AABB aabb, Color c)
		{
			Gl.glColor3f(c.R, c.G, c.B);
			Gl.glBegin(Gl.GL_LINE_LOOP);
			Gl.glVertex2f(aabb.LowerBound.X, aabb.LowerBound.Y);
			Gl.glVertex2f(aabb.UpperBound.X, aabb.LowerBound.Y);
			Gl.glVertex2f(aabb.UpperBound.X, aabb.UpperBound.Y);
			Gl.glVertex2f(aabb.LowerBound.X, aabb.UpperBound.Y);
			Gl.glEnd();
		}
	}
}
