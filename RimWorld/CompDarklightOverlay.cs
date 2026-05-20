using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompDarklightOverlay : CompFireOverlayBase
{
	protected CompRefuelable refuelableComp;

	public static readonly Graphic DarklightGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Darklight", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

	public new CompProperties_DarklightOverlay Props => (CompProperties_DarklightOverlay)props;

	public override void PostDraw()
	{
		base.PostDraw();
		if (refuelableComp == null || refuelableComp.HasFuel)
		{
			Vector3 drawPos = parent.DrawPos;
			drawPos.y += 0.03658537f;
			DarklightGraphic.Draw(drawPos, Rot4.North, parent);
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		refuelableComp = parent.GetComp<CompRefuelable>();
	}
}
