using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_Cluster : Graphic_Collection
{
	public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

	protected virtual float PositionVariance => 0.45f;

	protected virtual float SizeVariance => 0.2f;

	private float SizeFactorMin => 1f - SizeVariance;

	private float SizeFactorMax => 1f + SizeVariance;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Log.ErrorOnce("Graphic_Cluster cannot draw realtime.", 9432243);
	}

	protected virtual Vector3 GetCenter(Thing thing, int index)
	{
		return thing.TrueCenter();
	}

	protected virtual int ScatterCount(Thing thing)
	{
		if (!(thing is Filth { thickness: var thickness }))
		{
			return 3;
		}
		return thickness;
	}

	public Graphic SubGraphicFor(Thing thing)
	{
		if (thing == null)
		{
			return subGraphics[0];
		}
		int num = thing.OverrideGraphicIndex ?? thing.thingIDNumber;
		return subGraphics[num % subGraphics.Length];
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		Rand.PushState(thing.Position.GetHashCode());
		int num = ScatterCount(thing);
		Filth filth = thing as Filth;
		for (int i = 0; i < num; i++)
		{
			Material material = MatSingle;
			Vector3 center = GetCenter(thing, i) + new Vector3(Rand.Range(0f - PositionVariance, PositionVariance), 0f, Rand.Range(0f - PositionVariance, PositionVariance));
			Vector2 size = new Vector2(Rand.Range(data.drawSize.x * SizeFactorMin, data.drawSize.x * SizeFactorMax), Rand.Range(data.drawSize.y * SizeFactorMin, data.drawSize.y * SizeFactorMax));
			float rot = (float)Rand.RangeInclusive(0, 360) + extraRotation;
			if (filth?.drawInstances != null && filth.drawInstances.Count == num)
			{
				rot = filth.drawInstances[i].rotation;
			}
			bool flipUv = Rand.Value < 0.5f;
			Graphic.TryGetTextureAtlasReplacementInfo(material, thing.def.category.ToAtlasGroup(), flipUv, vertexColors: true, out material, out var uvs, out var vertexColor);
			Printer_Plane.PrintPlane(layer, center, size, material, rot, flipUv, uvs, new Color32[4] { vertexColor, vertexColor, vertexColor, vertexColor });
		}
		Rand.PopState();
	}

	public override string ToString()
	{
		return "Scatter(subGraphic[0]=" + subGraphics[0]?.ToString() + ", count=" + subGraphics.Length + ")";
	}
}
