using System;

namespace Verse.Noise;

public class DistFromPointAxes : ModuleBase
{
	public float spanX;

	public float spanZ;

	public DistFromPointAxes()
		: base(0)
	{
	}

	public DistFromPointAxes(float spanX, float spanZ)
		: base(0)
	{
		this.spanX = spanX;
		this.spanZ = spanZ;
	}

	public override double GetValue(double x, double y, double z)
	{
		double val = Math.Abs(x) / (double)spanX;
		double val2 = Math.Abs(z) / (double)spanZ;
		return Math.Max(val, val2);
	}
}
