using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mine : JobDriver
	{
		private int ticksToPickHit = -1000;

		private Effecter effecter;

		public const int BaseTicksBetweenPickHits = 100;

		private const int BaseDamagePerPickHit_NaturalRock = 80;

		private const int BaseDamagePerPickHit_NotNaturalRock = 40;

		private const float MinMiningSpeedFactorForNPCs = 0.6f;

		private Thing MineTarget => job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(MineTarget, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnCellMissingDesignation(TargetIndex.A, DesignationDefOf.Mine);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil mine = new Toil();
			mine.tickAction = delegate
			{
				Pawn actor = mine.actor;
				Thing mineTarget = MineTarget;
				if (ticksToPickHit < -100)
				{
					ResetTicksToPickHit();
				}
				if (actor.skills != null && (mineTarget.Faction != actor.Faction || actor.Faction == null))
				{
					actor.skills.Learn(SkillDefOf.Mining, 0.07f);
				}
				ticksToPickHit--;
				if (ticksToPickHit <= 0)
				{
					IntVec3 position = mineTarget.Position;
					if (effecter == null)
					{
						effecter = EffecterDefOf.Mine.Spawn();
					}
					effecter.Trigger(actor, mineTarget);
					int num = mineTarget.def.building.isNaturalRock ? 80 : 40;
					Mineable mineable = mineTarget as Mineable;
					if (mineable == null || mineTarget.HitPoints > num)
					{
						DamageInfo dinfo = new DamageInfo(DamageDefOf.Mining, num, 0f, -1f, mine.actor);
						mineTarget.TakeDamage(dinfo);
					}
					else
					{
						mineable.Notify_TookMiningDamage(mineTarget.HitPoints, mine.actor);
						mineable.HitPoints = 0;
						mineable.DestroyMined(actor);
					}
					if (mineTarget.Destroyed)
					{
						actor.Map.mineStrikeManager.CheckStruckOre(position, mineTarget.def, actor);
						actor.records.Increment(RecordDefOf.CellsMined);
						if (pawn.Faction != Faction.OfPlayer)
						{
							List<Thing> thingList = position.GetThingList(base.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								thingList[i].SetForbidden(value: true, warnOnFail: false);
							}
						}
						if (pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsVeryValuable(mineTarget.def))
						{
							TaleRecorder.RecordTale(TaleDefOf.MinedValuable, pawn, mineTarget.def.building.mineableThing);
						}
						if (pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsValuable(mineTarget.def) && !pawn.Map.IsPlayerHome)
						{
							TaleRecorder.RecordTale(TaleDefOf.CaravanRemoteMining, pawn, mineTarget.def.building.mineableThing);
						}
						ReadyForNextToil();
					}
					else
					{
						ResetTicksToPickHit();
					}
				}
			};
			mine.defaultCompleteMode = ToilCompleteMode.Never;
			mine.WithProgressBar(TargetIndex.A, () => 1f - (float)MineTarget.HitPoints / (float)MineTarget.MaxHitPoints);
			mine.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			mine.activeSkill = (() => SkillDefOf.Mining);
			yield return mine;
		}

		private void ResetTicksToPickHit()
		{
			float num = pawn.GetStatValue(StatDefOf.MiningSpeed);
			if (num < 0.6f && pawn.Faction != Faction.OfPlayer)
			{
				num = 0.6f;
			}
			ticksToPickHit = (int)Math.Round(100f / num);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToPickHit, "ticksToPickHit", 0);
		}
	}
}
