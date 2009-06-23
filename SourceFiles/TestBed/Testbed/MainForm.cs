/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx

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

#define GLRender

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;

using Tao.OpenGl;
using Tao.FreeGlut;

using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Collision;

namespace TestBed
{
	public partial class MainForm : Form
	{
		private Test _currentTest;
		public Test CurrentTest
		{
			get { return _currentTest; }
			set { _currentTest = value; }
		}

		private Settings settings = new Settings();
		private float viewZoom = 1f;
		private Vec2 viewCenter = new Vec2(0.0f, 20.0f);
		private TestEntry testEntry;
		private bool rMouseDown = false;
		private Vec2 lastp;		

		public MainForm()
		{
			InitializeComponent();

#if GLRender
			openGlControl.InitializeContexts();
			OpenGLDebugDraw.InitTextRenderer(openGlControl);
#endif //GLRender

			Init();
			SetView();
		}		

		private void MainForm_Load(object sender, EventArgs e)
		{
			chkbAabbs.Checked = settings.drawAABBs == 1 ? true : false;
			chkbCF.Checked = settings.drawContactForces == 1 ? true : false;
			chkbCN.Checked = settings.drawContactNormals == 1 ? true : false;
			chkbCom.Checked = settings.drawCOMs == 1 ? true : false;
			chkbCoreShapes.Checked = settings.drawCoreShapes == 1 ? true : false;
			chkbCP.Checked = settings.drawContactPoints == 1 ? true : false;
			chkbFF.Checked = settings.drawFrictionForces == 1 ? true : false;
			chkbJoints.Checked = settings.drawJoints == 1 ? true : false;
			chkbObbs.Checked = settings.drawOBBs == 1 ? true : false;
			chkbPairs.Checked = settings.drawPairs == 1 ? true : false;
			chkbShapes.Checked = settings.drawShapes == 1 ? true : false;
			chkbStatistics.Checked = settings.drawStats == 1 ? true : false;

			chkbToi.Checked = settings.enableTOI == 1 ? true : false;
			chkbWarmStart.Checked = settings.enableWarmStarting == 1 ? true : false;

			nudVelIters.Value = settings.velocityIterations;
			nudPosIters.Value = settings.positionIterations;
			nudHz.Value = (decimal)settings.hz;

			for (int i = 0; i < Test.g_testEntries.Length; i++)
			{
				cmbbTests.Items.Add(Test.g_testEntries[i]);
			}

			testEntry = Test.g_testEntries[0];
			CurrentTest = testEntry.CreateFcn();
			cmbbTests.SelectedIndex = 0;

			/*timer = new System.Timers.Timer();
			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
			timer.Interval = 1000.0f / settings.hz;
			timer.AutoReset = true;
			timer.Enabled = true;*/

			redrawTimer.Interval = 16;
			redrawTimer.Enabled = true;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			SetView();
		}

		#region Timer

		private void redrawTimer_Tick(object sender, EventArgs e)
		{
			SimulationLoop();
		}

		/*
		private System.Timers.Timer timer;
		private static int syncPoint = 0;
		int i = 0;
		int[] syncs = new int[1000];
		 * 
		void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// This example assumes that overlapping events can be
			// discarded. That is, if an Elapsed event is raised before 
			// the previous event is finished processing, the second
			// event is ignored. 
			//
			// CompareExchange is used to take control of syncPoint, 
			// and to determine whether the attempt was successful. 
			// CompareExchange attempts to put 1 into syncPoint, but
			// only if the current value of syncPoint is zero 
			// (specified by the third parameter). If another thread
			// has set syncPoint to 1, or if the control thread has
			// set syncPoint to -1, the current event is skipped. 
			// (Normally it would not be necessary to use a local 
			// variable for the return value. A local variable is 
			// used here to determine the reason the event was 
			// skipped.)
			
			int sync = Interlocked.CompareExchange(ref syncPoint, 1, 0);

			if (i < syncs.Length)
				syncs[i] = sync;
			else
				i = 0;

			if (sync == 0)
			{
				SimulationLoop();
				syncPoint = 0;
			}
		}*/

		#endregion Timer

		#region Input Handlers

		private void openGlControl_MouseMove(object sender, MouseEventArgs e)
		{
			Vec2 p = ConvertScreenToWorld(e.X, e.Y);
			CurrentTest.MouseMove(p);

			if (rMouseDown)
			{
				Vec2 diff = p - lastp;
				viewCenter.X -= diff.X;
				viewCenter.Y -= diff.Y;
				SetView();
				lastp = ConvertScreenToWorld(e.X, e.Y);
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (e.Location.X > openGlControl.Location.X && e.Location.X < openGlControl.Location.X + openGlControl.Width &&
				e.Location.Y > openGlControl.Location.Y && e.Location.Y < openGlControl.Location.Y + openGlControl.Height)
			{
				if (e.Delta > 0)
				{
					viewZoom /= 1.1f;
				}
				else
				{
					viewZoom *= 1.1f;
				}
				SetView();
			}
		}

		private void openGlControl_MouseUp(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
				CurrentTest.MouseUp();
			else if (e.Button == MouseButtons.Right)
				rMouseDown = false;
		}

		private void openGlControl_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				CurrentTest.MouseDown(ConvertScreenToWorld(e.X, e.Y));
			else if (e.Button == MouseButtons.Right)
			{
				lastp = ConvertScreenToWorld(e.X, e.Y);
				rMouseDown = true;
			}
		}

		private void openGlControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					this.Close();
					break;
				case Keys.Z:
					viewZoom = Box2DX.Common.Math.Min(1.1f * viewZoom, 20.0f);
					SetView();
					break;
				case Keys.X:
					viewZoom = Box2DX.Common.Math.Max(0.9f * viewZoom, 0.02f);
					SetView();
					break;
				case Keys.R:
					CurrentTest = testEntry.CreateFcn();
					break;
				case Keys.Space:
					CurrentTest.LaunchBomb();
					break;
				case Keys.Left:
					viewCenter.X -= 0.5f;
					SetView();
					break;
				case Keys.Right:
					viewCenter.X += 0.5f;
					SetView();
					break;
				case Keys.Down:
					viewCenter.Y -= 0.5f;
					SetView();
					break;
				case Keys.Up:
					viewCenter.Y += 0.5f;
					SetView();
					break;
				case Keys.Home:
					viewZoom = 1.0f;
					viewCenter.Set(0.0f, 20.0f);
					SetView();
					break;
				default:
					CurrentTest.Keyboard(e.KeyCode);
					break;
			}
		}

		#endregion Input Handlers

		#region Controls Events Handlers

		private void nudHz_ValueChanged(object sender, EventArgs e)
		{
			settings.hz = (float)nudHz.Value;
		}

		private void btnQuit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnSingleStep_Click(object sender, EventArgs e)
		{
			settings.singleStep = 1;
			settings.pause = 1;
		}

		private void btnPause_Click(object sender, EventArgs e)
		{
			settings.pause = settings.pause == 0 ? 1 : 0;
		}

		private void chkbWarmStart_CheckedChanged(object sender, EventArgs e)
		{
			settings.enableWarmStarting = chkbWarmStart.Checked ? 1 : 0;
		}

		private void chkbToi_CheckedChanged(object sender, EventArgs e)
		{
			settings.enableTOI = chkbToi.Checked ? 1 : 0;
		}

		private void chkbShapes_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawShapes = chkbShapes.Checked ? 1 : 0;
		}

		private void chkbJoints_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawJoints = chkbJoints.Checked ? 1 : 0;
		}

		private void chkbCoreShapes_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawCoreShapes = chkbCoreShapes.Checked ? 1 : 0;
		}

		private void chkbAabbs_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawAABBs = chkbAabbs.Checked ? 1 : 0;
		}

		private void chkbObbs_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawOBBs = chkbObbs.Checked ? 1 : 0;
		}

		private void chkbPairs_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawPairs = chkbPairs.Checked ? 1 : 0;
		}

		private void chkbCN_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawContactNormals = chkbCN.Checked ? 1 : 0;
		}

		private void chkbCF_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawContactForces = chkbCF.Checked ? 1 : 0;
		}

		private void chkbFF_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawFrictionForces = chkbFF.Checked ? 1 : 0;
		}

		private void chkbCom_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawCOMs = chkbCom.Checked ? 1 : 0;
		}

		private void chkbStatistics_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawStats = chkbStatistics.Checked ? 1 : 0;
		}

		private void chkbCP_CheckedChanged(object sender, EventArgs e)
		{
			settings.drawContactPoints = chkbCP.Checked ? 1 : 0;
		}

		private void cmbbTests_SelectedIndexChanged(object sender, EventArgs e)
		{
			testEntry = cmbbTests.SelectedItem as TestEntry;
			CurrentTest = testEntry.CreateFcn();
			this.Text = "Box2DX " + Application.ProductVersion + " - " + testEntry.ToString();
		}

		private void nudVelIters_ValueChanged(object sender, EventArgs e)
		{
			settings.velocityIterations = (int)nudVelIters.Value;
		}

		private void nudPosIters_ValueChanged(object sender, EventArgs e)
		{
			settings.positionIterations = (int)nudPosIters.Value;
		}

		#endregion Controls Events Handlers

		#region Render

		private void Init()
		{
#if GLRender
			Gl.glShadeModel(Gl.GL_SMOOTH);
			Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			Gl.glClearDepth(1.0f);
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glEnable(Gl.GL_LIGHT0);
			Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);
#endif
		}

		private void SetView()
		{
#if GLRender
			int width = openGlControl.Width;
			int height = openGlControl.Height;

			Gl.glViewport(0, 0, width, height);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();

			float ratio = (float)width / (float)height;

			Vec2 extents = new Vec2(ratio * 25.0f, 25.0f);
			extents *= viewZoom;

			Vec2 lower = viewCenter - extents;
			Vec2 upper = viewCenter + extents;

			// L/R/B/T
			Glu.gluOrtho2D(lower.X, upper.X, lower.Y, upper.Y);

			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
#endif
		}

		private Vec2 ConvertScreenToWorld(float x, float y)
		{
			float tw = openGlControl.Width;
			float th = openGlControl.Height;
			float u = x / tw;
			float v = (th - y) / th;

			float ratio = tw / th;
			Vec2 extents = new Vec2(ratio * 25.0f, 25.0f);
			extents *= viewZoom;

			Vec2 lower = viewCenter - extents;
			Vec2 upper = viewCenter + extents;

			Vec2 p = new Vec2();
			p.X = (1.0f - u) * lower.X + u * upper.X;
			p.Y = (1.0f - v) * lower.Y + v * upper.Y;
			return p;
		}

		private void SimulationLoop()
		{
#if GLRender
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

			CurrentTest.SetTextLine(30);
			CurrentTest.Step(settings);
			OpenGLDebugDraw.DrawString(5, 15, testEntry.Name);

			openGlControl.Draw();

			int errorCode = 0;
			if ((errorCode = Gl.glGetError()) > 0)
			{
				redrawTimer.Stop();
			}
#endif
		}

		#endregion Render		
	}
}
