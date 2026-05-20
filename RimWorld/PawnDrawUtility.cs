using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PawnDrawUtility
{
	public const string AnchorTagEyeLeft = "LeftEye";

	public const string AnchorTagEyeRight = "RightEye";

	public static bool AnchorUsable(Pawn pawn, BodyTypeDef.WoundAnchor anchor, Rot4 pawnRot)
	{
		if ((!anchor.rotation.HasValue || anchor.rotation.Value == pawnRot || (anchor.canMirror && anchor.rotation.Value == pawnRot.Opposite)) && (!anchor.narrowCrown.HasValue || (pawn.story != null && pawn.story.headType.narrow == anchor.narrowCrown.Value)))
		{
			if (!pawn.health.hediffSet.HasHead)
			{
				return anchor.layer != PawnOverlayDrawer.OverlayLayer.Head;
			}
			return true;
		}
		return false;
	}

	public static IEnumerable<BodyTypeDef.WoundAnchor> FindAnchors(Pawn pawn, BodyPartRecord curPart)
	{
		if (pawn.story == null || pawn.story.bodyType == null || pawn.story.bodyType.woundAnchors.NullOrEmpty())
		{
			yield break;
		}
		int iterations = 0;
		int found = 0;
		while (found == 0 && curPart != null && iterations < 100)
		{
			if (curPart.woundAnchorTag != null)
			{
				foreach (BodyTypeDef.WoundAnchor woundAnchor in pawn.story.bodyType.woundAnchors)
				{
					if (woundAnchor.tag == curPart.woundAnchorTag)
					{
						found++;
						yield return woundAnchor;
					}
				}
			}
			else
			{
				foreach (BodyTypeDef.WoundAnchor woundAnchor2 in pawn.story.bodyType.woundAnchors)
				{
					if (curPart.IsInGroup(woundAnchor2.group))
					{
						found++;
						yield return woundAnchor2;
					}
				}
			}
			curPart = curPart.parent;
			iterations++;
		}
		if (iterations == 100)
		{
			Log.Error("PawnWoundDrawer.RenderOverBody.FindAnchors while() loop ran into iteration limit! This is never supposed to happen! Is there a cyclic body part parent reference?");
		}
	}

	public static void CalcAnchorData(Pawn pawn, BodyTypeDef.WoundAnchor anchor, Rot4 pawnRot, out Vector3 anchorOffset, out float range)
	{
		anchorOffset = anchor.offset;
		if (anchor.rotation == pawnRot.Opposite)
		{
			anchorOffset.x *= -1f;
		}
		if ((anchor.tag == "LeftEye" || anchor.tag == "RightEye") && pawnRot.IsHorizontal && (!ModsConfig.BiotechActive || !pawn.DevelopmentalStage.Juvenile()))
		{
			Vector3? eyeOffsetEastWest = pawn.story.headType.eyeOffsetEastWest;
			if (eyeOffsetEastWest.HasValue)
			{
				if (pawnRot == Rot4.East)
				{
					anchorOffset = eyeOffsetEastWest.Value;
				}
				else
				{
					anchorOffset = eyeOffsetEastWest.Value.ScaledBy(new Vector3(-1f, 1f, 1f));
				}
			}
		}
		range = anchor.range;
	}
}
