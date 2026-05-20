using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class HediffComp_DissolveGearOnDeath : HediffComp
{
	public HediffCompProperties_DissolveGearOnDeath Props => (HediffCompProperties_DissolveGearOnDeath)props;

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		base.Notify_PawnDied(dinfo, culprit);
		if (Props.injuryCreatedOnDeath == null)
		{
			return;
		}
		List<BodyPartRecord> list = new List<BodyPartRecord>(from part in base.Pawn.health.hediffSet.GetNotMissingParts()
			where part.coverageAbs > 0f
			select part);
		int num = Mathf.Min(Props.injuryCount.RandomInRange, list.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			int index = Rand.Range(0, list.Count);
			BodyPartRecord bodyPartRecord = list[index];
			list.RemoveAt(index);
			if (base.Pawn.health.hediffSet.GetNotMissingParts().Contains(bodyPartRecord))
			{
				base.Pawn.health.AddHediff(Props.injuryCreatedOnDeath, bodyPartRecord);
			}
		}
	}

	public override void Notify_PawnKilled()
	{
		base.Pawn.equipment.DestroyAllEquipment();
		base.Pawn.apparel.DestroyAll();
		if (!base.Pawn.Spawned)
		{
			return;
		}
		if (Props.mote != null || Props.fleck != null)
		{
			Vector3 drawPos = base.Pawn.DrawPos;
			for (int i = 0; i < Props.moteCount; i++)
			{
				Vector2 vector = Rand.InsideUnitCircle * Props.moteOffsetRange.RandomInRange * Rand.Sign;
				Vector3 loc = new Vector3(drawPos.x + vector.x, drawPos.y, drawPos.z + vector.y);
				if (Props.mote != null)
				{
					MoteMaker.MakeStaticMote(loc, base.Pawn.Map, Props.mote);
				}
				else
				{
					FleckMaker.Static(loc, base.Pawn.Map, Props.fleck);
				}
			}
		}
		if (Props.filth != null)
		{
			FilthMaker.TryMakeFilth(base.Pawn.Position, base.Pawn.Map, Props.filth, Props.filthCount);
		}
		if (Props.sound != null)
		{
			Props.sound.PlayOneShot(SoundInfo.InMap(base.Pawn));
		}
	}
}
