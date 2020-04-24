namespace Verse.Noise
{
	public class AxisAsValueZ : ModuleBase
	{
		public AxisAsValueZ()
			: base(0)
		{
		}

		public override double GetValue(double x, double y, double z)
		{
			return z;
		}
	}
}
