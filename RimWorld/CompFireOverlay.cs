using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompFireOverlay : ThingComp
	{
		protected CompRefuelable refuelableComp;

		public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

		public CompProperties_FireOverlay Props => (CompProperties_FireOverlay)props;

		public override void PostDraw()
		{
			base.PostDraw();
			if (refuelableComp == null || refuelableComp.HasFuel)
			{
				Vector3 drawPos = parent.DrawPos;
				drawPos.y += 0.0454545468f;
				FireGraphic.Draw(drawPos, Rot4.North, parent);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			refuelableComp = parent.GetComp<CompRefuelable>();
		}
	}
}
