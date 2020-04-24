using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class Hediff_Pregnant : HediffWithComps
	{
		public Pawn father;

		private const int MiscarryCheckInterval = 1000;

		private const float MTBMiscarryStarvingDays = 0.5f;

		private const float MTBMiscarryWoundedDays = 0.5f;

		public float GestationProgress
		{
			get
			{
				return Severity;
			}
			private set
			{
				Severity = value;
			}
		}

		private bool IsSeverelyWounded
		{
			get
			{
				float num = 0f;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i] is Hediff_Injury && !hediffs[i].IsPermanent())
					{
						num += hediffs[i].Severity;
					}
				}
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
				{
					if (missingPartsCommonAncestors[j].IsFreshNonSolidExtremity)
					{
						num += missingPartsCommonAncestors[j].Part.def.GetMaxHealth(pawn);
					}
				}
				return num > 38f * pawn.RaceProps.baseHealthScale;
			}
		}

		public override void Tick()
		{
			ageTicks++;
			if (pawn.IsHashIntervalTick(1000))
			{
				if (pawn.needs.food != null && pawn.needs.food.CurCategory == HungerCategory.Starving && pawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition).Severity > 0.25f && Rand.MTBEventOccurs(0.5f, 60000f, 1000f))
				{
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						string value = pawn.Name.Numerical ? pawn.LabelShort : (pawn.LabelShort + " (" + pawn.kindDef.label + ")");
						Messages.Message("MessageMiscarriedStarvation".Translate(value, pawn), pawn, MessageTypeDefOf.NegativeHealthEvent);
					}
					Miscarry();
					return;
				}
				if (IsSeverelyWounded && Rand.MTBEventOccurs(0.5f, 60000f, 1000f))
				{
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						string value2 = pawn.Name.Numerical ? pawn.LabelShort : (pawn.LabelShort + " (" + pawn.kindDef.label + ")");
						Messages.Message("MessageMiscarriedPoorHealth".Translate(value2, pawn), pawn, MessageTypeDefOf.NegativeHealthEvent);
					}
					Miscarry();
					return;
				}
			}
			GestationProgress += 1f / (pawn.RaceProps.gestationPeriodDays * 60000f);
			if (GestationProgress >= 1f)
			{
				if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("MessageGaveBirth".Translate(pawn), pawn, MessageTypeDefOf.PositiveEvent);
				}
				DoBirthSpawn(pawn, father);
				pawn.health.RemoveHediff(this);
			}
		}

		private void Miscarry()
		{
			pawn.health.RemoveHediff(this);
		}

		public static void DoBirthSpawn(Pawn mother, Pawn father)
		{
			int num = (mother.RaceProps.litterSizeCurve == null) ? 1 : Mathf.RoundToInt(Rand.ByCurve(mother.RaceProps.litterSizeCurve));
			if (num < 1)
			{
				num = 1;
			}
			PawnGenerationRequest request = new PawnGenerationRequest(mother.kindDef, mother.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: true);
			Pawn pawn = null;
			for (int i = 0; i < num; i++)
			{
				pawn = PawnGenerator.GeneratePawn(request);
				if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, mother))
				{
					if (pawn.playerSettings != null && mother.playerSettings != null)
					{
						pawn.playerSettings.AreaRestriction = mother.playerSettings.AreaRestriction;
					}
					if (pawn.RaceProps.IsFlesh)
					{
						pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
						if (father != null)
						{
							pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
						}
					}
				}
				else
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				TaleRecorder.RecordTale(TaleDefOf.GaveBirth, mother, pawn);
			}
			if (mother.Spawned)
			{
				FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5);
				if (mother.caller != null)
				{
					mother.caller.DoCall();
				}
				if (pawn.caller != null)
				{
					pawn.caller.DoCall();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref father, "father");
		}

		public override string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.DebugString());
			stringBuilder.AppendLine("Gestation progress: " + GestationProgress.ToStringPercent());
			stringBuilder.AppendLine("Time left: " + ((int)((1f - GestationProgress) * pawn.RaceProps.gestationPeriodDays * 60000f)).ToStringTicksToPeriod());
			return stringBuilder.ToString();
		}
	}
}
