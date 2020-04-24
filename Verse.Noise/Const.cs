namespace Verse.Noise
{
	public class Const : ModuleBase
	{
		private double val;

		public double Value
		{
			get
			{
				return val;
			}
			set
			{
				val = value;
			}
		}

		public Const()
			: base(0)
		{
		}

		public Const(double value)
			: base(0)
		{
			Value = value;
		}

		public override double GetValue(double x, double y, double z)
		{
			return val;
		}
	}
}
