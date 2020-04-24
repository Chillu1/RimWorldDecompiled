namespace Verse.Noise
{
	public class Translate : ModuleBase
	{
		private double m_x = 1.0;

		private double m_y = 1.0;

		private double m_z = 1.0;

		public double X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
			}
		}

		public double Y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_y = value;
			}
		}

		public double Z
		{
			get
			{
				return m_z;
			}
			set
			{
				m_z = value;
			}
		}

		public Translate()
			: base(1)
		{
		}

		public Translate(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public Translate(double x, double y, double z, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			X = x;
			Y = y;
			Z = z;
		}

		public override double GetValue(double x, double y, double z)
		{
			return modules[0].GetValue(x + m_x, y + m_y, z + m_z);
		}
	}
}
