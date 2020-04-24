using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PowerNetGraphics
	{
		private const AltitudeLayer WireAltitude = AltitudeLayer.SmallWire;

		private static readonly Material WireMat = MaterialPool.MatFrom("Things/Special/Power/Wire");

		public static void PrintWirePieceConnecting(SectionLayer layer, Thing A, Thing B, bool forPowerOverlay)
		{
			Material mat = WireMat;
			float y = AltitudeLayer.SmallWire.AltitudeFor();
			if (forPowerOverlay)
			{
				mat = PowerOverlayMats.MatConnectorLine;
				y = AltitudeLayer.MapDataOverlay.AltitudeFor();
			}
			Vector3 center = (A.TrueCenter() + B.TrueCenter()) / 2f;
			center.y = y;
			Vector3 v = B.TrueCenter() - A.TrueCenter();
			Vector2 size = new Vector2(1f, v.MagnitudeHorizontal());
			float rot = v.AngleFlat();
			Printer_Plane.PrintPlane(layer, center, size, mat, rot);
		}

		public static void RenderAnticipatedWirePieceConnecting(IntVec3 userPos, Rot4 rotation, IntVec2 thingSize, Thing transmitter)
		{
			Vector3 vector = GenThing.TrueCenter(userPos, rotation, thingSize, AltitudeLayer.MapDataOverlay.AltitudeFor());
			if (userPos != transmitter.Position)
			{
				Vector3 vector2 = transmitter.TrueCenter();
				vector2.y = AltitudeLayer.MapDataOverlay.AltitudeFor();
				Vector3 pos = (vector + vector2) / 2f;
				Vector3 v = vector2 - vector;
				Vector3 s = new Vector3(1f, 1f, v.MagnitudeHorizontal());
				Quaternion q = Quaternion.LookRotation(vector2 - vector);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(pos, q, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, PowerOverlayMats.MatConnectorAnticipated, 0);
			}
		}

		public static void PrintOverlayConnectorBaseFor(SectionLayer layer, Thing t)
		{
			Vector3 center = t.TrueCenter();
			center.y = AltitudeLayer.MapDataOverlay.AltitudeFor();
			Printer_Plane.PrintPlane(layer, center, new Vector2(1f, 1f), PowerOverlayMats.MatConnectorBase);
		}
	}
}
