namespace Verse.Noise
{
	public class Turbulence : ModuleBase
	{
		private const double X0 = 0.189422607421875;

		private const double Y0 = 0.99371337890625;

		private const double Z0 = 0.4781646728515625;

		private const double X1 = 0.4046478271484375;

		private const double Y1 = 0.276611328125;

		private const double Z1 = 0.9230499267578125;

		private const double X2 = 0.82122802734375;

		private const double Y2 = 0.1710968017578125;

		private const double Z2 = 0.6842803955078125;

		private double m_power = 1.0;

		private Perlin m_xDistort;

		private Perlin m_yDistort;

		private Perlin m_zDistort;

		public double Frequency
		{
			get
			{
				return m_xDistort.Frequency;
			}
			set
			{
				m_xDistort.Frequency = value;
				m_yDistort.Frequency = value;
				m_zDistort.Frequency = value;
			}
		}

		public double Power
		{
			get
			{
				return m_power;
			}
			set
			{
				m_power = value;
			}
		}

		public int Roughness
		{
			get
			{
				return m_xDistort.OctaveCount;
			}
			set
			{
				m_xDistort.OctaveCount = value;
				m_yDistort.OctaveCount = value;
				m_zDistort.OctaveCount = value;
			}
		}

		public int Seed
		{
			get
			{
				return m_xDistort.Seed;
			}
			set
			{
				m_xDistort.Seed = value;
				m_yDistort.Seed = value + 1;
				m_zDistort.Seed = value + 2;
			}
		}

		public Turbulence()
			: base(1)
		{
			m_xDistort = new Perlin();
			m_yDistort = new Perlin();
			m_zDistort = new Perlin();
		}

		public Turbulence(ModuleBase input)
			: base(1)
		{
			m_xDistort = new Perlin();
			m_yDistort = new Perlin();
			m_zDistort = new Perlin();
			modules[0] = input;
		}

		public Turbulence(double power, ModuleBase input)
			: this(new Perlin(), new Perlin(), new Perlin(), power, input)
		{
		}

		public Turbulence(Perlin x, Perlin y, Perlin z, double power, ModuleBase input)
			: base(1)
		{
			m_xDistort = x;
			m_yDistort = y;
			m_zDistort = z;
			modules[0] = input;
			Power = power;
		}

		public override double GetValue(double x, double y, double z)
		{
			double x2 = x + m_xDistort.GetValue(x + 0.189422607421875, y + 0.99371337890625, z + 0.4781646728515625) * m_power;
			double y2 = y + m_yDistort.GetValue(x + 0.4046478271484375, y + 0.276611328125, z + 0.9230499267578125) * m_power;
			double z2 = z + m_zDistort.GetValue(x + 0.82122802734375, y + 0.1710968017578125, z + 0.6842803955078125) * m_power;
			return modules[0].GetValue(x2, y2, z2);
		}
	}
}
