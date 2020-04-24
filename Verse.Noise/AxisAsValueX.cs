namespace Verse.Noise
{
	public class AxisAsValueX : ModuleBase
	{
		public AxisAsValueX()
			: base(0)
		{
		}

		public override double GetValue(double x, double y, double z)
		{
			return x;
		}
	}
}
