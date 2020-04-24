using System;

namespace Verse.Noise
{
	public class DistFromAxis : ModuleBase
	{
		public float span;

		public DistFromAxis()
			: base(0)
		{
		}

		public DistFromAxis(float span)
			: base(0)
		{
			this.span = span;
		}

		public override double GetValue(double x, double y, double z)
		{
			return Math.Abs(x) / (double)span;
		}
	}
}
