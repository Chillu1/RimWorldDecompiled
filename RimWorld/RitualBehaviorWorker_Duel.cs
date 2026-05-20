using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class RitualBehaviorWorker_Duel : RitualBehaviorWorker
	{
		private Sustainer soundPlaying;

		public static readonly IntRange RadiusRangeSpectators = new IntRange(5, 7);

		public static readonly int RadiusRangeLeader = 3;

		public static readonly int RadiusRangeDuelists = 2;

		public override Sustainer SoundPlaying => soundPlaying;

		public RitualBehaviorWorker_Duel()
		{
		}

		public RitualBehaviorWorker_Duel(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
		{
			string text = base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
			if (text != null)
			{
				return text;
			}
			Room room = target.Cell.GetRoom(target.Map);
			if (StandableCellsInRange(new IntRange(RadiusRangeLeader, RadiusRangeLeader)) < 1)
			{
				return "CantStartNotEnoughSpaceDuelSpeaker".Translate(RadiusRangeLeader.Named("MINRADIUS"));
			}
			if (StandableCellsInRange(new IntRange(RadiusRangeDuelists, RadiusRangeDuelists)) < 2)
			{
				return "CantStartNotEnoughSpaceDuelDuelists".Translate(RadiusRangeSpectators.Average.Named("MINRADIUS"), 2);
			}
			if (StandableCellsInRange(RadiusRangeSpectators) < 20)
			{
				return "CantStartNotEnoughSpaceDuelSpectators".Translate(RadiusRangeSpectators.Average.Named("MINRADIUS"), 20);
			}
			return null;
			int StandableCellsInRange(IntRange range)
			{
				int num = 0;
				foreach (IntVec3 item in CellRect.CenteredOn(target.Cell, range.max))
				{
					float num2 = item.DistanceTo(target.Cell);
					if (num2 >= (float)range.min && num2 <= (float)range.max && item.Standable(target.Map) && item.GetRoom(target.Map) == room)
					{
						num++;
					}
				}
				return num;
			}
		}

		protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
		{
			return new LordJob_Ritual_Duel(target, ritual, obligation, def.stages, assignments, organizer);
		}

		public override void Tick(LordJob_Ritual ritual)
		{
			base.Tick(ritual);
			if (ritual.StageIndex == 4)
			{
				if (soundPlaying == null || soundPlaying.Ended)
				{
					TargetInfo selectedTarget = ritual.selectedTarget;
					soundPlaying = SoundDefOf.DuelMusic.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(selectedTarget.Cell, selectedTarget.Map), MaintenanceType.PerTick));
				}
				soundPlaying?.Maintain();
			}
		}

		public override void Cleanup(LordJob_Ritual ritual)
		{
			soundPlaying = null;
		}

		public override void DrawPreviewOnTarget(TargetInfo targetInfo)
		{
			base.DrawPreviewOnTarget(targetInfo);
			GenDraw.DrawRadiusRing(targetInfo.CenterCell, 5.9f);
		}
	}
}
