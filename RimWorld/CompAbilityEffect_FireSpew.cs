using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_FireSpew : CompAbilityEffect
{
	private readonly List<IntVec3> tmpCells = new List<IntVec3>();

	private new CompProperties_AbilityFireSpew Props => (CompProperties_AbilityFireSpew)props;

	private Pawn Pawn => parent.pawn;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		IntVec3 cell = target.Cell;
		Map mapHeld = parent.pawn.MapHeld;
		DamageDef flame = DamageDefOf.Flame;
		Pawn pawn = Pawn;
		ThingDef filthDef = Props.filthDef;
		int damAmount = Props.damAmount;
		SimpleCurve flammabilityAttachFireChanceCurve = parent.verb.verbProps.flammabilityAttachFireChanceCurve;
		List<IntVec3> overrideCells = AffectedCells(target);
		GenExplosion.DoExplosion(cell, mapHeld, 0f, flame, pawn, damAmount, -1f, null, null, null, null, filthDef, 1f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 1f, damageFalloff: false, null, null, null, doVisualEffects: false, 0.6f, 0f, doSoundEffects: false, null, 1f, flammabilityAttachFireChanceCurve, overrideCells);
		base.Apply(target, dest);
	}

	public override IEnumerable<PreCastAction> GetPreCastActions()
	{
		if (Props.effecterDef != null)
		{
			yield return new PreCastAction
			{
				action = delegate(LocalTargetInfo a, LocalTargetInfo b)
				{
					parent.AddEffecterToMaintain(Props.effecterDef.Spawn(parent.pawn.Position, a.Cell, parent.pawn.Map), Pawn.Position, a.Cell, 17, Pawn.MapHeld);
				},
				ticksAwayFromCast = 17
			};
		}
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawFieldEdges(AffectedCells(target));
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (Pawn.Faction != null)
		{
			foreach (IntVec3 item in AffectedCells(target))
			{
				List<Thing> thingList = item.GetThingList(Pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].Faction == Pawn.Faction)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private List<IntVec3> AffectedCells(LocalTargetInfo target)
	{
		tmpCells.Clear();
		Vector3 vector = Pawn.Position.ToVector3Shifted().Yto0();
		IntVec3 intVec = target.Cell.ClampInsideMap(Pawn.Map);
		if (Pawn.Position == intVec)
		{
			return tmpCells;
		}
		float lengthHorizontal = (intVec - Pawn.Position).LengthHorizontal;
		float num = (float)(intVec.x - Pawn.Position.x) / lengthHorizontal;
		float num2 = (float)(intVec.z - Pawn.Position.z) / lengthHorizontal;
		intVec.x = Mathf.RoundToInt((float)Pawn.Position.x + num * Props.range);
		intVec.z = Mathf.RoundToInt((float)Pawn.Position.z + num2 * Props.range);
		float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
		float num3 = Props.lineWidthEnd / 2f;
		float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
		float num5 = 57.29578f * Mathf.Asin(num3 / num4);
		int num6 = GenRadial.NumCellsInRadius(Props.range);
		for (int i = 0; i < num6; i++)
		{
			IntVec3 intVec2 = Pawn.Position + GenRadial.RadialPattern[i];
			if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
			{
				tmpCells.Add(intVec2);
			}
		}
		List<IntVec3> list = GenSight.BresenhamCellsBetween(Pawn.Position, intVec);
		for (int j = 0; j < list.Count; j++)
		{
			IntVec3 intVec3 = list[j];
			if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
			{
				tmpCells.Add(intVec3);
			}
		}
		return tmpCells;
		bool CanUseCell(IntVec3 c)
		{
			if (!c.InBounds(Pawn.Map))
			{
				return false;
			}
			if (c == Pawn.Position)
			{
				return false;
			}
			if (!Props.canHitFilledCells && c.Filled(Pawn.Map))
			{
				return false;
			}
			if (!c.InHorDistOf(Pawn.Position, Props.range))
			{
				return false;
			}
			ShootLine resultingLine;
			return parent.verb.TryFindShootLineFromTo(parent.pawn.Position, c, out resultingLine);
		}
	}
}
