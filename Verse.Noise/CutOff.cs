namespace Verse.Noise;

public class CutOff : ModuleBase
{
	private bool inverted;

	private bool zAxis;

	public CutOff()
		: base(0)
	{
	}

	public CutOff(bool invert, bool zAxis = false)
		: base(0)
	{
		inverted = invert;
		this.zAxis = zAxis;
	}

	public override double GetValue(double x, double y, double z)
	{
		double num = (zAxis ? z : x);
		if (inverted)
		{
			return (num < 0.0) ? 1 : 0;
		}
		return (num >= 0.0) ? 1 : 0;
	}
}
