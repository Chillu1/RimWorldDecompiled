using Verse;

namespace RimWorld
{
	public static class TaleUtility
	{
		private const float KilledTaleLongRangeThreshold = 35f;

		private const float KilledTaleMeleeRangeThreshold = 2f;

		private const float MajorEnemyThreshold = 250f;

		private static readonly SimpleCurve MajorThreatCurve = new SimpleCurve
		{
			new CurvePoint(100f, 0f),
			new CurvePoint(400f, 1f)
		};

		public static void Notify_PawnDied(Pawn victim, DamageInfo? dinfo)
		{
			if (Current.ProgramState != ProgramState.Playing || !dinfo.HasValue)
			{
				return;
			}
			Pawn pawn = dinfo.Value.Instigator as Pawn;
			if (pawn != null && pawn.CurJob != null && pawn.jobs.curDriver is JobDriver_Execute)
			{
				return;
			}
			bool flag = !victim.RaceProps.Humanlike && dinfo.Value.Instigator != null && dinfo.Value.Instigator.Spawned && dinfo.Value.Instigator is Pawn && ((Pawn)dinfo.Value.Instigator).jobs.curDriver is JobDriver_Slaughter;
			if (pawn != null)
			{
				if (victim.IsColonist)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledColonist, pawn, victim);
				}
				else if (victim.Faction == Faction.OfPlayer && victim.RaceProps.Animal && !flag)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledColonyAnimal, pawn, victim);
				}
			}
			if ((victim.Faction == Faction.OfPlayer || (pawn != null && pawn.Faction == Faction.OfPlayer)) && !flag)
			{
				TaleRecorder.RecordTale(TaleDefOf.KilledBy, victim, dinfo.Value);
			}
			if (pawn != null)
			{
				if (dinfo.Value.Weapon != null && dinfo.Value.Weapon.building != null && dinfo.Value.Weapon.building.IsMortar)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledMortar, pawn, victim, dinfo.Value.Weapon);
				}
				else if (pawn != null && pawn.Position.DistanceTo(victim.Position) >= 35f)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledLongRange, pawn, victim, dinfo.Value.Weapon);
				}
				else if (dinfo.Value.Weapon != null && dinfo.Value.Weapon.IsMeleeWeapon)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledMelee, pawn, victim, dinfo.Value.Weapon);
				}
				bool flag2 = false;
				if (victim.Faction != null && victim.Faction.leader == victim && victim.Faction != pawn.Faction && victim.Faction.HostileTo(pawn.Faction))
				{
					flag2 = true;
					TaleRecorder.RecordTale(TaleDefOf.DefeatedHostileFactionLeader, pawn, victim);
				}
				if (victim.HostileTo(pawn) && Rand.Chance(MajorThreatCurve.Evaluate(victim.kindDef.combatPower)))
				{
					flag2 = true;
				}
				if (flag2)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledMajorThreat, pawn, victim, dinfo.Value.Weapon);
				}
				PawnCapacityDef pawnCapacityDef = victim.health.ShouldBeDeadFromRequiredCapacity();
				if (pawnCapacityDef != null)
				{
					TaleRecorder.RecordTale(TaleDefOf.KilledCapacity, pawn, victim, pawnCapacityDef);
				}
			}
		}
	}
}
