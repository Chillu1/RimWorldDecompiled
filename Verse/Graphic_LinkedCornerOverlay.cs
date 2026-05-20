using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_LinkedCornerOverlay : Graphic_Linked
{
	public Graphic_Random overlayGraphic;

	public Graphic_LinkedCornerOverlay(Graphic subGraphic)
		: base(subGraphic)
	{
		base.subGraphic = subGraphic;
		data = subGraphic.data;
		overlayGraphic = GraphicDatabase.Get<Graphic_Random>(data.cornerOverlayPath, ShaderDatabase.Transparent, drawSize, color) as Graphic_Random;
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		base.Print(layer, thing, extraRotation);
		IntVec3 position = thing.Position;
		if (ShouldLinkWith(position + IntVec3.East, thing) && ShouldLinkWith(position + IntVec3.North, thing) && ShouldLinkWith(position + IntVec3.NorthEast, thing))
		{
			Rand.PushState(thing.thingIDNumber * 9);
			Material material = overlayGraphic.MatSingleFor(thing);
			float rot = Rand.Range(0f, 360f);
			bool flipUv = Rand.Bool;
			Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv, vertexColors: false, out material, out var uvs, out var _);
			Printer_Plane.PrintPlane(layer, thing.TrueCenter() + new Vector3(0.5f, 0.1f, 0.5f), Vector3.one, material, rot, flipUv: false, uvs);
			Rand.PopState();
		}
	}
}
