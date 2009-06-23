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

namespace TestBed
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.openGlControl = new Tao.Platform.Windows.SimpleOpenGlControl();
			this.nudHz = new System.Windows.Forms.NumericUpDown();
			this.nudPosIters = new System.Windows.Forms.NumericUpDown();
			this.nudVelIters = new System.Windows.Forms.NumericUpDown();
			this.btnQuit = new System.Windows.Forms.Button();
			this.btnSingleStep = new System.Windows.Forms.Button();
			this.btnPause = new System.Windows.Forms.Button();
			this.gbDraw = new System.Windows.Forms.GroupBox();
			this.flpDraw = new System.Windows.Forms.FlowLayoutPanel();
			this.chkbShapes = new System.Windows.Forms.CheckBox();
			this.chkbJoints = new System.Windows.Forms.CheckBox();
			this.chkbCoreShapes = new System.Windows.Forms.CheckBox();
			this.chkbAabbs = new System.Windows.Forms.CheckBox();
			this.chkbObbs = new System.Windows.Forms.CheckBox();
			this.chkbPairs = new System.Windows.Forms.CheckBox();
			this.chkbCP = new System.Windows.Forms.CheckBox();
			this.chkbCN = new System.Windows.Forms.CheckBox();
			this.chkbCF = new System.Windows.Forms.CheckBox();
			this.chkbFF = new System.Windows.Forms.CheckBox();
			this.chkbCom = new System.Windows.Forms.CheckBox();
			this.chkbStatistics = new System.Windows.Forms.CheckBox();
			this.chkbToi = new System.Windows.Forms.CheckBox();
			this.chkbWarmStart = new System.Windows.Forms.CheckBox();
			this.lblPosIters = new System.Windows.Forms.Label();
			this.lblHz = new System.Windows.Forms.Label();
			this.lblVelIters = new System.Windows.Forms.Label();
			this.lblTests = new System.Windows.Forms.Label();
			this.cmbbTests = new System.Windows.Forms.ComboBox();
			this.redrawTimer = new System.Windows.Forms.Timer(this.components);
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudHz)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudPosIters)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudVelIters)).BeginInit();
			this.gbDraw.SuspendLayout();
			this.flpDraw.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer.IsSplitterFixed = true;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.openGlControl);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.nudHz);
			this.splitContainer.Panel2.Controls.Add(this.nudPosIters);
			this.splitContainer.Panel2.Controls.Add(this.nudVelIters);
			this.splitContainer.Panel2.Controls.Add(this.btnQuit);
			this.splitContainer.Panel2.Controls.Add(this.btnSingleStep);
			this.splitContainer.Panel2.Controls.Add(this.btnPause);
			this.splitContainer.Panel2.Controls.Add(this.gbDraw);
			this.splitContainer.Panel2.Controls.Add(this.chkbToi);
			this.splitContainer.Panel2.Controls.Add(this.chkbWarmStart);
			this.splitContainer.Panel2.Controls.Add(this.lblPosIters);
			this.splitContainer.Panel2.Controls.Add(this.lblHz);
			this.splitContainer.Panel2.Controls.Add(this.lblVelIters);
			this.splitContainer.Panel2.Controls.Add(this.lblTests);
			this.splitContainer.Panel2.Controls.Add(this.cmbbTests);
			this.splitContainer.Size = new System.Drawing.Size(692, 443);
			this.splitContainer.SplitterDistance = 445;
			this.splitContainer.TabIndex = 0;
			// 
			// openGlControl
			// 
			this.openGlControl.AccumBits = ((byte)(0));
			this.openGlControl.AutoCheckErrors = false;
			this.openGlControl.AutoFinish = false;
			this.openGlControl.AutoMakeCurrent = true;
			this.openGlControl.AutoSwapBuffers = true;
			this.openGlControl.BackColor = System.Drawing.Color.Black;
			this.openGlControl.ColorBits = ((byte)(32));
			this.openGlControl.DepthBits = ((byte)(16));
			this.openGlControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.openGlControl.Location = new System.Drawing.Point(0, 0);
			this.openGlControl.Name = "openGlControl";
			this.openGlControl.Size = new System.Drawing.Size(445, 443);
			this.openGlControl.StencilBits = ((byte)(0));
			this.openGlControl.TabIndex = 0;
			this.openGlControl.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.openGlControl_PreviewKeyDown);
			this.openGlControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseMove);
			this.openGlControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseDown);
			this.openGlControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseUp);
			// 
			// nudHz
			// 
			this.nudHz.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nudHz.DecimalPlaces = 2;
			this.nudHz.Location = new System.Drawing.Point(62, 91);
			this.nudHz.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.nudHz.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudHz.Name = "nudHz";
			this.nudHz.Size = new System.Drawing.Size(169, 20);
			this.nudHz.TabIndex = 7;
			this.nudHz.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudHz.ValueChanged += new System.EventHandler(this.nudHz_ValueChanged);
			// 
			// nudPosIters
			// 
			this.nudPosIters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nudPosIters.Location = new System.Drawing.Point(62, 65);
			this.nudPosIters.Name = "nudPosIters";
			this.nudPosIters.Size = new System.Drawing.Size(169, 20);
			this.nudPosIters.TabIndex = 6;
			this.nudPosIters.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudPosIters.ValueChanged += new System.EventHandler(this.nudPosIters_ValueChanged);
			// 
			// nudVelIters
			// 
			this.nudVelIters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nudVelIters.Location = new System.Drawing.Point(62, 39);
			this.nudVelIters.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.nudVelIters.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudVelIters.Name = "nudVelIters";
			this.nudVelIters.Size = new System.Drawing.Size(169, 20);
			this.nudVelIters.TabIndex = 6;
			this.nudVelIters.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudVelIters.ValueChanged += new System.EventHandler(this.nudVelIters_ValueChanged);
			// 
			// btnQuit
			// 
			this.btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnQuit.Location = new System.Drawing.Point(9, 408);
			this.btnQuit.Name = "btnQuit";
			this.btnQuit.Size = new System.Drawing.Size(222, 23);
			this.btnQuit.TabIndex = 5;
			this.btnQuit.Text = "Quit";
			this.btnQuit.UseVisualStyleBackColor = true;
			this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
			// 
			// btnSingleStep
			// 
			this.btnSingleStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnSingleStep.Location = new System.Drawing.Point(9, 382);
			this.btnSingleStep.Name = "btnSingleStep";
			this.btnSingleStep.Size = new System.Drawing.Size(222, 23);
			this.btnSingleStep.TabIndex = 5;
			this.btnSingleStep.Text = "Single Step";
			this.btnSingleStep.UseVisualStyleBackColor = true;
			this.btnSingleStep.Click += new System.EventHandler(this.btnSingleStep_Click);
			// 
			// btnPause
			// 
			this.btnPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnPause.Location = new System.Drawing.Point(9, 353);
			this.btnPause.Name = "btnPause";
			this.btnPause.Size = new System.Drawing.Size(222, 23);
			this.btnPause.TabIndex = 5;
			this.btnPause.Text = "Pause";
			this.btnPause.UseVisualStyleBackColor = true;
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// gbDraw
			// 
			this.gbDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gbDraw.AutoSize = true;
			this.gbDraw.Controls.Add(this.flpDraw);
			this.gbDraw.Location = new System.Drawing.Point(6, 160);
			this.gbDraw.Name = "gbDraw";
			this.gbDraw.Size = new System.Drawing.Size(225, 187);
			this.gbDraw.TabIndex = 4;
			this.gbDraw.TabStop = false;
			this.gbDraw.Text = "Draw";
			// 
			// flpDraw
			// 
			this.flpDraw.AutoScroll = true;
			this.flpDraw.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flpDraw.Controls.Add(this.chkbShapes);
			this.flpDraw.Controls.Add(this.chkbJoints);
			this.flpDraw.Controls.Add(this.chkbCoreShapes);
			this.flpDraw.Controls.Add(this.chkbAabbs);
			this.flpDraw.Controls.Add(this.chkbObbs);
			this.flpDraw.Controls.Add(this.chkbPairs);
			this.flpDraw.Controls.Add(this.chkbCP);
			this.flpDraw.Controls.Add(this.chkbCN);
			this.flpDraw.Controls.Add(this.chkbCF);
			this.flpDraw.Controls.Add(this.chkbFF);
			this.flpDraw.Controls.Add(this.chkbCom);
			this.flpDraw.Controls.Add(this.chkbStatistics);
			this.flpDraw.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpDraw.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flpDraw.Location = new System.Drawing.Point(3, 16);
			this.flpDraw.Name = "flpDraw";
			this.flpDraw.Size = new System.Drawing.Size(219, 168);
			this.flpDraw.TabIndex = 0;
			// 
			// chkbShapes
			// 
			this.chkbShapes.AutoSize = true;
			this.chkbShapes.Location = new System.Drawing.Point(3, 3);
			this.chkbShapes.Name = "chkbShapes";
			this.chkbShapes.Size = new System.Drawing.Size(62, 17);
			this.chkbShapes.TabIndex = 0;
			this.chkbShapes.Text = "Shapes";
			this.chkbShapes.UseVisualStyleBackColor = true;
			this.chkbShapes.CheckedChanged += new System.EventHandler(this.chkbShapes_CheckedChanged);
			// 
			// chkbJoints
			// 
			this.chkbJoints.AutoSize = true;
			this.chkbJoints.Location = new System.Drawing.Point(3, 26);
			this.chkbJoints.Name = "chkbJoints";
			this.chkbJoints.Size = new System.Drawing.Size(53, 17);
			this.chkbJoints.TabIndex = 1;
			this.chkbJoints.Text = "Joints";
			this.chkbJoints.UseVisualStyleBackColor = true;
			this.chkbJoints.CheckedChanged += new System.EventHandler(this.chkbJoints_CheckedChanged);
			// 
			// chkbCoreShapes
			// 
			this.chkbCoreShapes.AutoSize = true;
			this.chkbCoreShapes.Location = new System.Drawing.Point(3, 49);
			this.chkbCoreShapes.Name = "chkbCoreShapes";
			this.chkbCoreShapes.Size = new System.Drawing.Size(87, 17);
			this.chkbCoreShapes.TabIndex = 2;
			this.chkbCoreShapes.Text = "Core Shapes";
			this.chkbCoreShapes.UseVisualStyleBackColor = true;
			this.chkbCoreShapes.CheckedChanged += new System.EventHandler(this.chkbCoreShapes_CheckedChanged);
			// 
			// chkbAabbs
			// 
			this.chkbAabbs.AutoSize = true;
			this.chkbAabbs.Location = new System.Drawing.Point(3, 72);
			this.chkbAabbs.Name = "chkbAabbs";
			this.chkbAabbs.Size = new System.Drawing.Size(59, 17);
			this.chkbAabbs.TabIndex = 3;
			this.chkbAabbs.Text = "AABBs";
			this.chkbAabbs.UseVisualStyleBackColor = true;
			this.chkbAabbs.CheckedChanged += new System.EventHandler(this.chkbAabbs_CheckedChanged);
			// 
			// chkbObbs
			// 
			this.chkbObbs.AutoSize = true;
			this.chkbObbs.Location = new System.Drawing.Point(3, 95);
			this.chkbObbs.Name = "chkbObbs";
			this.chkbObbs.Size = new System.Drawing.Size(53, 17);
			this.chkbObbs.TabIndex = 4;
			this.chkbObbs.Text = "OBBs";
			this.chkbObbs.UseVisualStyleBackColor = true;
			this.chkbObbs.CheckedChanged += new System.EventHandler(this.chkbObbs_CheckedChanged);
			// 
			// chkbPairs
			// 
			this.chkbPairs.AutoSize = true;
			this.chkbPairs.Location = new System.Drawing.Point(3, 118);
			this.chkbPairs.Name = "chkbPairs";
			this.chkbPairs.Size = new System.Drawing.Size(49, 17);
			this.chkbPairs.TabIndex = 5;
			this.chkbPairs.Text = "Pairs";
			this.chkbPairs.UseVisualStyleBackColor = true;
			this.chkbPairs.CheckedChanged += new System.EventHandler(this.chkbPairs_CheckedChanged);
			// 
			// chkbCP
			// 
			this.chkbCP.AutoSize = true;
			this.chkbCP.Location = new System.Drawing.Point(3, 141);
			this.chkbCP.Name = "chkbCP";
			this.chkbCP.Size = new System.Drawing.Size(95, 17);
			this.chkbCP.TabIndex = 6;
			this.chkbCP.Text = "Contact Points";
			this.chkbCP.UseVisualStyleBackColor = true;
			this.chkbCP.CheckedChanged += new System.EventHandler(this.chkbCP_CheckedChanged);
			// 
			// chkbCN
			// 
			this.chkbCN.AutoSize = true;
			this.chkbCN.Location = new System.Drawing.Point(104, 3);
			this.chkbCN.Name = "chkbCN";
			this.chkbCN.Size = new System.Drawing.Size(104, 17);
			this.chkbCN.TabIndex = 7;
			this.chkbCN.Text = "Contact Normals";
			this.chkbCN.UseVisualStyleBackColor = true;
			this.chkbCN.CheckedChanged += new System.EventHandler(this.chkbCN_CheckedChanged);
			// 
			// chkbCF
			// 
			this.chkbCF.AutoSize = true;
			this.chkbCF.Location = new System.Drawing.Point(104, 26);
			this.chkbCF.Name = "chkbCF";
			this.chkbCF.Size = new System.Drawing.Size(98, 17);
			this.chkbCF.TabIndex = 9;
			this.chkbCF.Text = "Contact Forces";
			this.chkbCF.UseVisualStyleBackColor = true;
			this.chkbCF.CheckedChanged += new System.EventHandler(this.chkbCF_CheckedChanged);
			// 
			// chkbFF
			// 
			this.chkbFF.AutoSize = true;
			this.chkbFF.Location = new System.Drawing.Point(104, 49);
			this.chkbFF.Name = "chkbFF";
			this.chkbFF.Size = new System.Drawing.Size(95, 17);
			this.chkbFF.TabIndex = 8;
			this.chkbFF.Text = "Friction Forces";
			this.chkbFF.UseVisualStyleBackColor = true;
			this.chkbFF.CheckedChanged += new System.EventHandler(this.chkbFF_CheckedChanged);
			// 
			// chkbCom
			// 
			this.chkbCom.AutoSize = true;
			this.chkbCom.Location = new System.Drawing.Point(104, 72);
			this.chkbCom.Name = "chkbCom";
			this.chkbCom.Size = new System.Drawing.Size(108, 17);
			this.chkbCom.TabIndex = 10;
			this.chkbCom.Text = "Center of Masses";
			this.chkbCom.UseVisualStyleBackColor = true;
			this.chkbCom.CheckedChanged += new System.EventHandler(this.chkbCom_CheckedChanged);
			// 
			// chkbStatistics
			// 
			this.chkbStatistics.AutoSize = true;
			this.chkbStatistics.Location = new System.Drawing.Point(104, 95);
			this.chkbStatistics.Name = "chkbStatistics";
			this.chkbStatistics.Size = new System.Drawing.Size(68, 17);
			this.chkbStatistics.TabIndex = 11;
			this.chkbStatistics.Text = "Statistics";
			this.chkbStatistics.UseVisualStyleBackColor = true;
			this.chkbStatistics.CheckedChanged += new System.EventHandler(this.chkbStatistics_CheckedChanged);
			// 
			// chkbToi
			// 
			this.chkbToi.AutoSize = true;
			this.chkbToi.Location = new System.Drawing.Point(6, 137);
			this.chkbToi.Name = "chkbToi";
			this.chkbToi.Size = new System.Drawing.Size(96, 17);
			this.chkbToi.TabIndex = 3;
			this.chkbToi.Text = "Time of Impact";
			this.chkbToi.UseVisualStyleBackColor = true;
			this.chkbToi.CheckedChanged += new System.EventHandler(this.chkbToi_CheckedChanged);
			// 
			// chkbWarmStart
			// 
			this.chkbWarmStart.AutoSize = true;
			this.chkbWarmStart.Location = new System.Drawing.Point(6, 114);
			this.chkbWarmStart.Name = "chkbWarmStart";
			this.chkbWarmStart.Size = new System.Drawing.Size(93, 17);
			this.chkbWarmStart.TabIndex = 3;
			this.chkbWarmStart.Text = "Warm Starting";
			this.chkbWarmStart.UseVisualStyleBackColor = true;
			this.chkbWarmStart.CheckedChanged += new System.EventHandler(this.chkbWarmStart_CheckedChanged);
			// 
			// lblPosIters
			// 
			this.lblPosIters.AutoSize = true;
			this.lblPosIters.Location = new System.Drawing.Point(4, 67);
			this.lblPosIters.Name = "lblPosIters";
			this.lblPosIters.Size = new System.Drawing.Size(51, 13);
			this.lblPosIters.TabIndex = 1;
			this.lblPosIters.Text = "Pos Iters:";
			// 
			// lblHz
			// 
			this.lblHz.AutoSize = true;
			this.lblHz.Location = new System.Drawing.Point(3, 93);
			this.lblHz.Name = "lblHz";
			this.lblHz.Size = new System.Drawing.Size(35, 13);
			this.lblHz.TabIndex = 1;
			this.lblHz.Text = "Hertz:";
			// 
			// lblVelIters
			// 
			this.lblVelIters.AutoSize = true;
			this.lblVelIters.Location = new System.Drawing.Point(3, 42);
			this.lblVelIters.Name = "lblVelIters";
			this.lblVelIters.Size = new System.Drawing.Size(48, 13);
			this.lblVelIters.TabIndex = 1;
			this.lblVelIters.Text = "Vel Iters:";
			// 
			// lblTests
			// 
			this.lblTests.AutoSize = true;
			this.lblTests.Location = new System.Drawing.Point(3, 15);
			this.lblTests.Name = "lblTests";
			this.lblTests.Size = new System.Drawing.Size(36, 13);
			this.lblTests.TabIndex = 1;
			this.lblTests.Text = "Tests:";
			// 
			// cmbbTests
			// 
			this.cmbbTests.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbbTests.FormattingEnabled = true;
			this.cmbbTests.Location = new System.Drawing.Point(62, 12);
			this.cmbbTests.MaxDropDownItems = 50;
			this.cmbbTests.Name = "cmbbTests";
			this.cmbbTests.Size = new System.Drawing.Size(169, 21);
			this.cmbbTests.TabIndex = 0;
			this.cmbbTests.SelectedIndexChanged += new System.EventHandler(this.cmbbTests_SelectedIndexChanged);
			// 
			// redrawTimer
			// 
			this.redrawTimer.Interval = 16;
			this.redrawTimer.Tick += new System.EventHandler(this.redrawTimer_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(692, 443);
			this.Controls.Add(this.splitContainer);
			this.MinimumSize = new System.Drawing.Size(700, 470);
			this.Name = "MainForm";
			this.Text = "Box2DX";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.nudHz)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudPosIters)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudVelIters)).EndInit();
			this.gbDraw.ResumeLayout(false);
			this.flpDraw.ResumeLayout(false);
			this.flpDraw.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.Label lblTests;
		private System.Windows.Forms.ComboBox cmbbTests;
		private System.Windows.Forms.GroupBox gbDraw;
		private System.Windows.Forms.CheckBox chkbToi;
		private System.Windows.Forms.CheckBox chkbWarmStart;
		private System.Windows.Forms.Label lblHz;
		private System.Windows.Forms.Label lblVelIters;
		private System.Windows.Forms.FlowLayoutPanel flpDraw;
		private System.Windows.Forms.CheckBox chkbShapes;
		private System.Windows.Forms.CheckBox chkbJoints;
		private System.Windows.Forms.CheckBox chkbCoreShapes;
		private System.Windows.Forms.CheckBox chkbAabbs;
		private System.Windows.Forms.CheckBox chkbObbs;
		private System.Windows.Forms.CheckBox chkbPairs;
		private System.Windows.Forms.CheckBox chkbCP;
		private System.Windows.Forms.CheckBox chkbCN;
		private System.Windows.Forms.CheckBox chkbCF;
		private System.Windows.Forms.CheckBox chkbFF;
		private System.Windows.Forms.CheckBox chkbCom;
		private System.Windows.Forms.CheckBox chkbStatistics;
		private System.Windows.Forms.Button btnQuit;
		private System.Windows.Forms.Button btnSingleStep;
		private System.Windows.Forms.Button btnPause;
		private Tao.Platform.Windows.SimpleOpenGlControl openGlControl;
		private System.Windows.Forms.Timer redrawTimer;
		private System.Windows.Forms.NumericUpDown nudHz;
		private System.Windows.Forms.NumericUpDown nudVelIters;
		private System.Windows.Forms.Label lblPosIters;
		private System.Windows.Forms.NumericUpDown nudPosIters;
	}
}

