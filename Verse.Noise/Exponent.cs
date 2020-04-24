using System;

namespace Verse.Noise
{
	public class Exponent : ModuleBase
	{
		private double m_exponent = 1.0;

		public double Value
		{
			get
			{
				return m_exponent;
			}
			set
			{
				m_exponent = value;
			}
		}

		public Exponent()
			: base(1)
		{
		}

		public Exponent(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public Exponent(double exponent, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			Value = exponent;
		}

		public override double GetValue(double x, double y, double z)
		{
			return Math.Pow(Math.Abs((modules[0].GetValue(x, y, z) + 1.0) / 2.0), m_exponent) * 2.0 - 1.0;
		}
	}
}
