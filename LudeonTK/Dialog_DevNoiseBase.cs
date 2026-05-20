using System;
using System.Collections.Generic;
using System.Globalization;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace LudeonTK;

[StaticConstructorOnStartup]
public abstract class Dialog_DevNoiseBase : Window_DevListing
{
	private int seedOffset;

	private int seedKey;

	private string seedString;

	private string freqStr;

	private string lacStr;

	private string octStr;

	private string persStr;

	private double freq;

	private double lac;

	private double pers;

	private bool freqFine;

	private bool lacFine;

	private bool persFine;

	private float octSliderPos;

	private int oct;

	private float seedOffsetSlider;

	private bool toggleSliders;

	private bool toggleUpdate = true;

	private PlanetLayer planetLayer;

	public Perlin noise;

	public float cutoff;

	public bool cutoffEnabled = true;

	public float alpha = 1f;

	private const string TooltipFreq = "Scale of the noise-map.\n\nLarger values zoom the noise out.\nSmaller values zoom it in.";

	private const string TooltipOct = "How many octaves contribute to the final value.\n\nHigher octaves contribute according to lacunarity and persistence values.";

	private const string TooltipLac = "Muliplies frequency at each octave (adjusts frequency)\n\n1: octaves have same frequency.\n< 1: successive octaves are smoother.\n> 1: successive octaves are rougher.\na";

	private const string TooltipPers = "How much each octave contributes to the final value (adjusts amplitude)\n\n1: octaves contribute equally.\n< 1: successive octaves contribute less.\n> 1: successive octaves contribute more.\na";

	public override bool AutoUpdate => false;

	protected abstract string Title { get; }

	protected abstract void OnNoiseChanged();

	protected abstract int GetSeed();

	public override void PreOpen()
	{
		base.PreOpen();
		noise = new Perlin(0.017000000923871994, 2.0, 0.5, 6, normalized: true, invert: false, GetSeed(), QualityMode.Medium);
	}

	protected override void DoWindowListing()
	{
		PrintLabel(Title, GameFont.Small);
		Gap(4f);
		if (planetLayer != PlanetLayer.Selected)
		{
			RefreshNoiseVars();
		}
		Toggle("Sliders: " + (toggleSliders ? "Enabled" : "Disabled"), ref toggleSliders);
		Toggle("Updating: " + (toggleUpdate ? "Enabled" : "Disabled"), ref toggleUpdate);
		Gap(17f);
		if (toggleSliders)
		{
			DoSliderControls();
		}
		else
		{
			DoTextControls();
		}
		Gap(17f);
		if (SliderFieldInt($"Seed Offset: {seedOffset}", ref seedOffsetSlider, ref seedOffset, -10, 10))
		{
			noise.Seed = Gen.HashCombineInt(GetSeed() + seedOffset, seedKey);
		}
		if (TextFieldInt("Seed part", ref seedString, ref seedKey))
		{
			noise.Seed = Gen.HashCombineInt(GetSeed() + seedOffset, seedKey);
		}
		Gap(17f);
		if (DevGUI.ButtonText(TakeRow(), $"Quality Mode: {noise.Quality}"))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (QualityMode value3 in Enum.GetValues(typeof(QualityMode)))
			{
				QualityMode local = value3;
				list.Add(new FloatMenuOption(local.ToStringSafe(), delegate
				{
					noise.Quality = local;
				}));
				base.Dirty = true;
			}
			FloatMenu window = new FloatMenu(list, "Select quality mode");
			Find.WindowStack.Add(window);
		}
		bool value = noise.Normalized;
		if (Toggle("Normalized: " + (value ? "Enabled" : "Disabled"), ref value))
		{
			noise.Normalized = value;
		}
		bool value2 = noise.Invert;
		if (Toggle("Invert: " + (value2 ? "Enabled" : "Disabled"), ref value2))
		{
			noise.Invert = value2;
		}
		Gap(17f);
		SliderFieldFloat($"Cutoff: {cutoff:0.##}", ref cutoff, ref cutoffEnabled);
		SliderFieldFloat($"Alpha: {alpha * 100f:0}%", ref alpha);
		Gap(17f);
		if (base.Dirty && toggleUpdate)
		{
			noise.Frequency = freq;
			noise.Lacunarity = lac;
			noise.Persistence = pers;
			noise.OctaveCount = oct;
			OnNoiseChanged();
			OnChanged();
		}
	}

	private void RefreshNoiseVars()
	{
		freq = noise.Frequency;
		lac = noise.Lacunarity;
		oct = noise.OctaveCount;
		pers = noise.Persistence;
		freqStr = freq.ToString(CultureInfo.InvariantCulture);
		lacStr = lac.ToString(CultureInfo.InvariantCulture);
		octStr = oct.ToString(CultureInfo.InvariantCulture);
		persStr = pers.ToString(CultureInfo.InvariantCulture);
		planetLayer = PlanetLayer.Selected;
	}

	private void DoSliderControls()
	{
		SliderFieldDouble($"Frequency: {noise.Frequency:0.####}", ref freq, ref freqFine, 0f, 0.2f, "Scale of the noise-map.\n\nLarger values zoom the noise out.\nSmaller values zoom it in.");
		SliderFieldDouble($"Lacunarity: {noise.Lacunarity:0.##}", ref lac, ref lacFine, 0f, 5f, "Muliplies frequency at each octave (adjusts frequency)\n\n1: octaves have same frequency.\n< 1: successive octaves are smoother.\n> 1: successive octaves are rougher.\na");
		SliderFieldDouble($"Persistence: {noise.Persistence:0.##}", ref pers, ref persFine, 0f, 5f, "How much each octave contributes to the final value (adjusts amplitude)\n\n1: octaves contribute equally.\n< 1: successive octaves contribute less.\n> 1: successive octaves contribute more.\na");
		SliderFieldInt($"Octave Count: {noise.OctaveCount}", ref octSliderPos, ref oct, 0, 8, "How many octaves contribute to the final value.\n\nHigher octaves contribute according to lacunarity and persistence values.");
		if (base.Dirty)
		{
			freqStr = freq.ToString(CultureInfo.InvariantCulture);
			lacStr = lac.ToString(CultureInfo.InvariantCulture);
			octStr = oct.ToString(CultureInfo.InvariantCulture);
			persStr = pers.ToString(CultureInfo.InvariantCulture);
		}
	}

	private void DoTextControls()
	{
		TextFieldDouble("Frequency", ref freqStr, ref freq, "Scale of the noise-map.\n\nLarger values zoom the noise out.\nSmaller values zoom it in.");
		TextFieldDouble("Lacunarity", ref lacStr, ref lac, "Muliplies frequency at each octave (adjusts frequency)\n\n1: octaves have same frequency.\n< 1: successive octaves are smoother.\n> 1: successive octaves are rougher.\na");
		TextFieldDouble("Persistence", ref persStr, ref pers, "How much each octave contributes to the final value (adjusts amplitude)\n\n1: octaves contribute equally.\n< 1: successive octaves contribute less.\n> 1: successive octaves contribute more.\na");
		TextFieldInt("Octave Count", ref octStr, ref oct, "How many octaves contribute to the final value.\n\nHigher octaves contribute according to lacunarity and persistence values.");
	}
}
