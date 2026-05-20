using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_Flicker : Graphic_Collection
{
	private const int BaseTicksPerFrameChange = 15;

	private const float MaxOffset = 0.05f;

	public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		if (thingDef == null)
		{
			Vector3 vector = loc;
			Log.ErrorOnce("Fire DrawWorker with null thingDef: " + vector.ToString(), 3427324);
			return;
		}
		if (subGraphics == null)
		{
			Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef, 358773632);
			return;
		}
		int num = Find.TickManager.TicksGame;
		if (thing != null)
		{
			num += Mathf.Abs(thing.thingIDNumber ^ 0x80FD52);
		}
		int num2 = num / 15;
		int num3 = Mathf.Abs(num2 ^ ((thing?.thingIDNumber ?? 0) * 391)) % subGraphics.Length;
		float num4 = 1f;
		CompFireOverlayBase compFireOverlayBase = null;
		Fire fire = thing as Fire;
		CompProperties_FireOverlay compProperties = thingDef.GetCompProperties<CompProperties_FireOverlay>();
		if (fire != null)
		{
			num4 = fire.fireSize;
		}
		else if (thing != null)
		{
			compFireOverlayBase = thing.TryGetComp<CompFireOverlayBase>();
			if (compFireOverlayBase != null)
			{
				num4 = compFireOverlayBase.FireSize;
			}
			else
			{
				compFireOverlayBase = thing.TryGetComp<CompDarklightOverlay>();
				if (compFireOverlayBase != null)
				{
					num4 = compFireOverlayBase.FireSize;
				}
			}
		}
		else if (compProperties != null)
		{
			num4 = compProperties.fireSize;
		}
		if (num3 < 0 || num3 >= subGraphics.Length)
		{
			Log.ErrorOnce("Fire drawing out of range: " + num3, 7453435);
			num3 = 0;
		}
		Graphic graphic = subGraphics[num3];
		float num5 = ((compFireOverlayBase == null) ? Mathf.Min(num4 / 1.2f, 1.2f) : num4);
		Vector3 vector2 = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius;
		vector2 *= 0.05f;
		Vector3 pos = loc + vector2 * num4;
		if (thing?.Graphic?.data != null)
		{
			pos += thing.Graphic.data.DrawOffsetForRot(rot);
		}
		if (compFireOverlayBase != null)
		{
			pos += compFireOverlayBase.Props.DrawOffsetForRot(rot);
		}
		Vector3 s = new Vector3(num5, 1f, num5);
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(pos, Quaternion.identity, s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
	}

	public override string ToString()
	{
		return "Flicker(subGraphic[0]=" + subGraphics[0]?.ToString() + ", count=" + subGraphics.Length + ")";
	}
}
