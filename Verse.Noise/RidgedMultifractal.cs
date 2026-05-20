using System;
using UnityEngine;

namespace Verse.Noise;

public class RidgedMultifractal : ModuleBase
{
	private double frequency = 1.0;

	private double lacunarity = 2.0;

	private QualityMode quality = QualityMode.Medium;

	private int octaveCount = 6;

	private int seed;

	private double[] weights = new double[30];

	public double Frequency
	{
		get
		{
			return frequency;
		}
		set
		{
			frequency = value;
		}
	}

	public double Lacunarity
	{
		get
		{
			return lacunarity;
		}
		set
		{
			lacunarity = value;
			UpdateWeights();
		}
	}

	public QualityMode Quality
	{
		get
		{
			return quality;
		}
		set
		{
			quality = value;
		}
	}

	public int OctaveCount
	{
		get
		{
			return octaveCount;
		}
		set
		{
			octaveCount = Mathf.Clamp(value, 1, 30);
		}
	}

	public int Seed
	{
		get
		{
			return seed;
		}
		set
		{
			seed = value;
		}
	}

	public RidgedMultifractal()
		: base(0)
	{
		UpdateWeights();
	}

	public RidgedMultifractal(double frequency, double lacunarity, int octaves, int seed, QualityMode quality)
		: base(0)
	{
		Frequency = frequency;
		Lacunarity = lacunarity;
		OctaveCount = octaves;
		Seed = seed;
		Quality = quality;
	}

	private void UpdateWeights()
	{
		double num = 1.0;
		for (int i = 0; i < 30; i++)
		{
			weights[i] = Math.Pow(num, -1.0);
			num *= lacunarity;
		}
	}

	public override double GetValue(double x, double y, double z)
	{
		x *= frequency;
		y *= frequency;
		z *= frequency;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 1.0;
		double num4 = 1.0;
		double num5 = 2.0;
		for (int i = 0; i < octaveCount; i++)
		{
			double x2 = Utils.MakeInt32Range(x);
			double y2 = Utils.MakeInt32Range(y);
			double z2 = Utils.MakeInt32Range(z);
			long num6 = (seed + i) & 0x7FFFFFFF;
			num = Utils.GradientCoherentNoise3D(x2, y2, z2, num6, quality);
			num = Math.Abs(num);
			num = num4 - num;
			num *= num;
			num *= num3;
			num3 = num * num5;
			if (num3 > 1.0)
			{
				num3 = 1.0;
			}
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			num2 += num * weights[i];
			x *= lacunarity;
			y *= lacunarity;
			z *= lacunarity;
		}
		return num2 * 1.25 - 1.0;
	}
}
