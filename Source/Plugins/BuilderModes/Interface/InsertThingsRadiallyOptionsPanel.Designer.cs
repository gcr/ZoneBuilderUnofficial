namespace CodeImp.DoomBuilder.BuilderModes
{
	partial class InsertThingsRadiallyOptionsPanel
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
			if(disposing && (components != null)) 
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.numberlabel = new System.Windows.Forms.ToolStripLabel();
            this.number = new CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown();
            this.radiuslabel = new System.Windows.Forms.ToolStripLabel();
            this.radius = new CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown();
            this.reset = new System.Windows.Forms.ToolStripButton();
            this.snaptogrid = new CodeImp.DoomBuilder.Controls.ToolStripCheckBox();
            this.typelabel = new System.Windows.Forms.ToolStripLabel();
            this.type = new CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown();
            this.browse = new System.Windows.Forms.ToolStripButton();
            this.parameterlabel = new System.Windows.Forms.ToolStripLabel();
            this.parameter = new CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.numberlabel,
            this.number,
            this.radiuslabel,
            this.radius,
            this.reset,
            this.snaptogrid,
            this.typelabel,
            this.type,
            this.browse,
            this.parameterlabel,
            this.parameter});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(573, 26);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // numberlabel
            // 
            this.numberlabel.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Gear;
            this.numberlabel.Name = "numberlabel";
            this.numberlabel.Size = new System.Drawing.Size(70, 23);
            this.numberlabel.Text = "Number:";
            // 
            // number
            // 
            this.number.AutoSize = false;
            this.number.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.number.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.number.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.number.Name = "number";
            this.number.Size = new System.Drawing.Size(56, 20);
            this.number.Text = "8";
            this.number.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.number.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // radiuslabel
            // 
            this.radiuslabel.Name = "radiuslabel";
            this.radiuslabel.Size = new System.Drawing.Size(45, 23);
            this.radiuslabel.Text = "Radius:";
            // 
            // radius
            // 
            this.radius.AutoSize = false;
            this.radius.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.radius.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.radius.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.radius.Name = "radius";
            this.radius.Size = new System.Drawing.Size(56, 20);
            this.radius.Text = "64";
            this.radius.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.radius.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // reset
            // 
            this.reset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.reset.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Reset;
            this.reset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(23, 23);
            this.reset.Text = "Reset";
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // snaptogrid
            // 
            this.snaptogrid.Checked = false;
            this.snaptogrid.Name = "snaptogrid";
            this.snaptogrid.Size = new System.Drawing.Size(90, 23);
            this.snaptogrid.Text = "Snap to grid";
            // 
            // typelabel
            // 
            this.typelabel.Name = "typelabel";
            this.typelabel.Size = new System.Drawing.Size(35, 23);
            this.typelabel.Text = "Type:";
            // 
            // type
            // 
            this.type.Maximum = new decimal(new int[] {
            4095,
            0,
            0,
            0});
            this.type.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.type.Name = "type";
            this.type.Size = new System.Drawing.Size(47, 23);
            this.type.Text = "1";
            this.type.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // browse
            // 
            this.browse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.browse.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.List;
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(23, 23);
            this.browse.Text = "browse";
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // parameterlabel
            // 
            this.parameterlabel.Name = "parameterlabel";
            this.parameterlabel.Size = new System.Drawing.Size(64, 23);
            this.parameterlabel.Text = "Parameter:";
            // 
            // parameter
            // 
            this.parameter.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.parameter.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.parameter.Name = "parameter";
            this.parameter.Size = new System.Drawing.Size(35, 23);
            this.parameter.Text = "0";
            this.parameter.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // InsertThingsRadiallyOptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.toolStrip1);
            this.Name = "InsertThingsRadiallyOptionsPanel";
            this.Size = new System.Drawing.Size(573, 60);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel numberlabel;
		private CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown number;
		private System.Windows.Forms.ToolStripLabel radiuslabel;
		private CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown radius;
		private System.Windows.Forms.ToolStripButton reset;
        private Controls.ToolStripCheckBox snaptogrid;
        private CodeImp.DoomBuilder.Controls.ToolStripNumericUpDown type;
        private System.Windows.Forms.ToolStripLabel typelabel;
        private System.Windows.Forms.ToolStripLabel parameterlabel;
        private Controls.ToolStripNumericUpDown parameter;
        private System.Windows.Forms.ToolStripButton browse;
    }
}
