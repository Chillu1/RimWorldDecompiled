using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompSanguophageMeetingFire : CompFireOverlayBase
{
	public static readonly Graphic RedlightGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Redlight", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

	public new CompProperties_SanguophageMeetingFire Props => (CompProperties_SanguophageMeetingFire)props;

	public override void PostDraw()
	{
		base.PostDraw();
		CompGlower compGlower = parent.TryGetComp<CompGlower>();
		if (compGlower == null || compGlower.Glows)
		{
			Vector3 drawPos = parent.DrawPos;
			drawPos.y += 0.03658537f;
			RedlightGraphic.Draw(drawPos + Props.offset, Rot4.North, parent);
		}
	}

	public override bool CompPreventClaimingBy(Faction faction)
	{
		return ((Building)parent).GetLord()?.CurLordToil is LordToil_SanguophageMeeting;
	}
}
