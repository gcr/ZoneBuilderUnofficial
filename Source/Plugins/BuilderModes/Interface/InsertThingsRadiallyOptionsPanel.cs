using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal partial class InsertThingsRadiallyOptionsPanel : UserControl
	{
		public event EventHandler OnValueChanged;
		private bool blockEvents;

		public int Number { get { return (int)number.Value; } set { blockEvents = true; number.Value = value; blockEvents = false; } }
		public int MaxNumber { get { return (int)number.Maximum; } set { number.Maximum = value; } }
		public int MinNumber { get { return (int)number.Minimum; } set { number.Minimum = value; } }
		public int Radius { get { return (int)radius.Value; } set { blockEvents = true; radius.Value = value; blockEvents = false; } }
		public int MaxRadius { get { return (int)radius.Maximum; } set { radius.Maximum = value; } }
		public int MinRadius { get { return (int)radius.Minimum; } set { radius.Minimum = value; } }
        public bool SnapToGrid { get { return snaptogrid.Checked; } set { snaptogrid.Checked = value; } }
        public int Type { get { return (int)type.Value; } set { type.Value = value; } }
        public int Parameter { get { return (int)parameter.Value; } set { parameter.Value = value; } }

        public InsertThingsRadiallyOptionsPanel() 
		{
			InitializeComponent();
		}

		public void Register() 
		{
            number.ValueChanged += ValueChanged;
            radius.ValueChanged += ValueChanged;
            snaptogrid.CheckedChanged += ValueChanged;
            type.ValueChanged += ValueChanged;
            parameter.ValueChanged += ValueChanged;

            General.Interface.AddButton(numberlabel);
            General.Interface.AddButton(number);
            General.Interface.AddButton(radiuslabel);
			General.Interface.AddButton(radius);
            General.Interface.AddButton(snaptogrid);
            General.Interface.AddButton(typelabel);
            General.Interface.AddButton(type);
            General.Interface.AddButton(parameterlabel);
            General.Interface.AddButton(parameter);
            General.Interface.AddButton(reset);
		}

		public void Unregister() 
		{
			General.Interface.RemoveButton(reset);
            General.Interface.RemoveButton(parameter);
            General.Interface.RemoveButton(parameterlabel);
            General.Interface.RemoveButton(type);
            General.Interface.RemoveButton(typelabel);
            General.Interface.RemoveButton(snaptogrid);
            General.Interface.RemoveButton(radius);
			General.Interface.RemoveButton(radiuslabel);
            General.Interface.RemoveButton(number);
            General.Interface.RemoveButton(numberlabel);
        }

        private void ValueChanged(object sender, EventArgs e) 
		{
			if(!blockEvents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}

		private void reset_Click(object sender, EventArgs e) 
		{
			blockEvents = true;
            parameter.Value = 0;
            type.Value = 1;
            snaptogrid.Checked = false;
            radius.Value = 32;
			blockEvents = false;
            number.Value = 4;
        }
    }
}
