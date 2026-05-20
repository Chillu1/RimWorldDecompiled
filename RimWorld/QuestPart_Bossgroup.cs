using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_Bossgroup : QuestPart_MakeLord
	{
		public List<Pawn> bosses = new List<Pawn>();

		public IntVec3 stageLocation;

		protected override Lord MakeLord()
		{
			LordJob_BossgroupAssaultColony lordJob = new LordJob_BossgroupAssaultColony(faction, stageLocation, bosses);
			return LordMaker.MakeNewLord(faction, lordJob, mapParent.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref stageLocation, "stageLocation");
			Scribe_Collections.Look(ref bosses, "bosses", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				bosses.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
