using System;
using System.Collections.Generic;

namespace Verse.Noise;

public class DistFromPointRects : ModuleBase
{
	public List<CellRect> rects;

	public DistFromPointRects()
		: base(0)
	{
	}

	public DistFromPointRects(List<CellRect> rects)
		: base(0)
	{
		this.rects = rects;
	}

	public override double GetValue(double x, double y, double z)
	{
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < rects.Count; i++)
		{
			double num3 = 1.0 - Math.Abs(x - (double)rects[i].CenterCell.x) / (double)((float)rects[i].Width / 2f);
			double num4 = 1.0 - Math.Abs(z - (double)rects[i].CenterCell.z) / (double)((float)rects[i].Height / 2f);
			if (num3 > num)
			{
				num = num3;
			}
			if (num4 > num2)
			{
				num2 = num4;
			}
		}
		return Math.Min(num, num2);
	}
}
