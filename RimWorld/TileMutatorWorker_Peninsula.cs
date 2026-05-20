using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Peninsula : TileMutatorWorker_Coast
{
	private const float PeninsulaRadius = 0.6f;

	private const float PeninsulaOffset = 0.1f;

	public TileMutatorWorker_Peninsula(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			float coastAngle = GetCoastAngle(map.Tile);
			ModuleBase input = new DistFromPoint((float)map.Size.x * 0.6f);
			input = new ScaleBias(-1.0, 1.0, input);
			input = new Multiply(input, new CutOff(invert: true));
			ModuleBase input2 = new DistFromAxis((float)map.Size.x * 0.6f);
			input2 = new Rotate(0.0, 90.0, 0.0, input2);
			input2 = new ScaleBias(-1.0, 1.0, input2);
			input2 = new Multiply(input2, new CutOff());
			coastNoise = new Add(input, input2);
			coastNoise = new Translate((float)map.Size.x * 0.1f, 0.0, 0.0, coastNoise);
			coastNoise = new Rotate(0.0, coastAngle, 0.0, coastNoise);
			coastNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, coastNoise);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "Peninsula shape", map.Size.ToIntVec2);
			coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.006f, 30f, 2);
			coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.015f, 25f);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "Peninsula", map.Size.ToIntVec2);
		}
	}
}
