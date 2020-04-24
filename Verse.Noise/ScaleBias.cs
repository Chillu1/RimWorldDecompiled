namespace Verse.Noise
{
	public class ScaleBias : ModuleBase
	{
		private double scale = 1.0;

		private double bias;

		public double Bias
		{
			get
			{
				return bias;
			}
			set
			{
				bias = value;
			}
		}

		public double Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
			}
		}

		public ScaleBias()
			: base(1)
		{
		}

		public ScaleBias(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public ScaleBias(double scale, double bias, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			Bias = bias;
			Scale = scale;
		}

		public override double GetValue(double x, double y, double z)
		{
			return modules[0].GetValue(x, y, z) * scale + bias;
		}
	}
}
