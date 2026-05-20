using System;

namespace Verse.Noise;

public class Voronoi2D : ModuleBase
{
	private double frequency = 1.0;

	private int seed;

	private float xRandomness = 1f;

	private float zRandomness = 1f;

	private bool staggered;

	public Voronoi2D()
		: base(0)
	{
	}

	public Voronoi2D(double frequency, int seed, float xRandomness = 1f, float zRandomness = 1f, bool staggered = false)
		: base(0)
	{
		this.frequency = frequency;
		this.seed = seed;
		this.xRandomness = xRandomness;
		this.zRandomness = zRandomness;
		this.staggered = staggered;
	}

	public Voronoi2D(double frequency, int seed, float randomness = 1f, bool staggered = false)
		: base(0)
	{
		this.frequency = frequency;
		this.seed = seed;
		xRandomness = randomness;
		zRandomness = randomness;
		this.staggered = staggered;
	}

	public override double GetValue(double x, double y, double z)
	{
		x *= frequency;
		z *= frequency;
		int num = ((x > 0.0) ? ((int)x) : ((int)x - 1));
		int num2 = ((z > 0.0) ? ((int)z) : ((int)z - 1));
		double num3 = 2147483647.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int i = num2 - 2; i <= num2 + 2; i++)
		{
			for (int j = num - 2; j <= num + 2; j++)
			{
				double num6 = (double)j + Utils.ValueNoise3D(j, 0, i, seed) * (double)xRandomness;
				double num7 = (double)i + Utils.ValueNoise3D(j, 0, i, seed + 2) * (double)zRandomness;
				if (staggered && i % 2 == 0)
				{
					num6 += 0.5;
				}
				double num8 = num6 - x;
				double num9 = num7 - z;
				double num10 = num8 * num8 + num9 * num9;
				if (num10 < num3)
				{
					num3 = num10;
					num4 = num6;
					num5 = num7;
				}
			}
		}
		double num11 = num4 - x;
		double num12 = num5 - z;
		return Math.Sqrt(num11 * num11 + num12 * num12);
	}
}
