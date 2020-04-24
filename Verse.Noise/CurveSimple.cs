namespace Verse.Noise
{
	public class CurveSimple : ModuleBase
	{
		private SimpleCurve curve;

		public CurveSimple(ModuleBase input, SimpleCurve curve)
			: base(1)
		{
			modules[0] = input;
			this.curve = curve;
		}

		public override double GetValue(double x, double y, double z)
		{
			return curve.Evaluate((float)modules[0].GetValue(x, y, z));
		}
	}
}
