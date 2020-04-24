using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic_Cluster : Graphic_Collection
	{
		private const float PositionVariance = 0.45f;

		private const float SizeVariance = 0.2f;

		private const float SizeFactorMin = 0.8f;

		private const float SizeFactorMax = 1.2f;

		public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Log.ErrorOnce("Graphic_Scatter cannot draw realtime.", 9432243);
		}

		public override void Print(SectionLayer layer, Thing thing)
		{
			Vector3 a = thing.TrueCenter();
			Rand.PushState();
			Rand.Seed = thing.Position.GetHashCode();
			int num = (thing as Filth)?.thickness ?? 3;
			for (int i = 0; i < num; i++)
			{
				Material matSingle = MatSingle;
				Vector3 center = a + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.45f, 0.45f));
				Vector2 size = new Vector2(Rand.Range(data.drawSize.x * 0.8f, data.drawSize.x * 1.2f), Rand.Range(data.drawSize.y * 0.8f, data.drawSize.y * 1.2f));
				float rot = Rand.RangeInclusive(0, 360);
				bool flipUv = Rand.Value < 0.5f;
				Printer_Plane.PrintPlane(layer, center, size, matSingle, rot, flipUv);
			}
			Rand.PopState();
		}

		public override string ToString()
		{
			return "Scatter(subGraphic[0]=" + subGraphics[0].ToString() + ", count=" + subGraphics.Length + ")";
		}
	}
}
