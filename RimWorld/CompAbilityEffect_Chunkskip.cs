using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Chunkskip : CompAbilityEffect
{
	public static Color DustColor = new Color(0.55f, 0.55f, 0.55f, 3f);

	private List<Thing> foundChunksTemp;

	private int lastChunkUpdateFrame;

	public new CompProperties_AbilityChunkskip Props => (CompProperties_AbilityChunkskip)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		IEnumerable<Thing> enumerable = FindClosestChunks(target);
		Map map = parent.pawn.Map;
		foreach (Thing item in enumerable)
		{
			if (FindFreeCell(target.Cell, map, out var result))
			{
				AbilityUtility.DoClamor(item.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
				AbilityUtility.DoClamor(result, Props.clamorRadius, parent.pawn, Props.clamorType);
				parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(item.Position, map, 0.72f), item.Position, 60);
				parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(result, map, 0.72f), result, 60);
				FleckMaker.ThrowDustPuffThick(result.ToVector3(), map, Rand.Range(1.5f, 3f), DustColor);
				item.Position = result;
				CompAbilityEffect_Teleport.SendSkipUsedSignal(result, parent.pawn);
			}
		}
		SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, map));
		base.Apply(target, dest);
	}

	public override IEnumerable<PreCastAction> GetPreCastActions()
	{
		yield return new PreCastAction
		{
			action = delegate(LocalTargetInfo t, LocalTargetInfo d)
			{
				foreach (Thing item in FindClosestChunks(t))
				{
					FleckMaker.Static(item.TrueCenter(), parent.pawn.Map, FleckDefOf.PsycastSkipFlashEntry, 0.72f);
				}
			},
			ticksAwayFromCast = 5
		};
	}

	private IEnumerable<Thing> FindClosestChunks(LocalTargetInfo target)
	{
		if (lastChunkUpdateFrame == Time.frameCount && foundChunksTemp != null)
		{
			return foundChunksTemp;
		}
		if (foundChunksTemp == null)
		{
			foundChunksTemp = new List<Thing>();
		}
		foundChunksTemp.Clear();
		RegionTraverser.BreadthFirstTraverse(target.Cell, parent.pawn.Map, (Region from, Region to) => true, delegate(Region x)
		{
			List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Chunk);
			for (int i = 0; i < list.Count; i++)
			{
				if (foundChunksTemp.Count >= Props.chunkCount)
				{
					break;
				}
				Thing thing = list[i];
				if (!thing.Fogged() && !foundChunksTemp.Contains(thing))
				{
					foundChunksTemp.Add(thing);
				}
			}
			return foundChunksTemp.Count >= Props.chunkCount;
		}, 999999, RegionType.Set_All);
		lastChunkUpdateFrame = Time.frameCount;
		return foundChunksTemp;
	}

	private bool FindFreeCell(IntVec3 target, Map map, out IntVec3 result)
	{
		return CellFinder.TryFindRandomCellNear(target, map, Mathf.RoundToInt(Props.scatterRadius) - 1, (IntVec3 cell) => CompAbilityEffect_WithDest.CanTeleportThingTo(cell, map) && GenSight.LineOfSight(cell, target, map, skipFirstCell: true), out result);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		foreach (Thing item in FindClosestChunks(target))
		{
			GenDraw.DrawLineBetween(item.TrueCenter(), target.CenterVector3);
			GenDraw.DrawTargetHighlight(item);
		}
		GenDraw.DrawRadiusRing(target.Cell, Props.scatterRadius);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (!target.Cell.Standable(parent.pawn.Map))
		{
			return false;
		}
		if (target.Cell.Filled(parent.pawn.Map))
		{
			return false;
		}
		if (!FindClosestChunks(target).Any())
		{
			return false;
		}
		if (!FindFreeCell(target.Cell, parent.pawn.Map, out var _))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNotEnoughFreeSpace".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return base.Valid(target, throwMessages);
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!FindClosestChunks(target).Any())
		{
			return false;
		}
		if (!FindFreeCell(target.Cell, parent.pawn.Map, out var _))
		{
			return false;
		}
		return base.CanApplyOn(target, dest);
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		if (target.IsValid && !target.Cell.Filled(parent.pawn.Map) && !FindClosestChunks(target).Any())
		{
			return "AbilityNoChunkToSkip".Translate();
		}
		return base.ExtraLabelMouseAttachment(target);
	}
}
