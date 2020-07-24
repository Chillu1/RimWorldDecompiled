using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_SkillTracker : IExposable
	{
		private Pawn pawn;

		public List<SkillRecord> skills = new List<SkillRecord>();

		private int lastXpSinceMidnightResetTimestamp = -1;

		public Pawn_SkillTracker(Pawn newPawn)
		{
			pawn = newPawn;
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				skills.Add(new SkillRecord(pawn, allDef));
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref skills, "skills", LookMode.Deep, pawn);
			Scribe_Values.Look(ref lastXpSinceMidnightResetTimestamp, "lastXpSinceMidnightResetTimestamp", 0);
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (skills.RemoveAll((SkillRecord x) => x == null) != 0)
			{
				Log.Error("Some skills were null after loading for " + pawn.ToStringSafe());
			}
			if (skills.RemoveAll((SkillRecord x) => x.def == null) != 0)
			{
				Log.Error("Some skills had null def after loading for " + pawn.ToStringSafe());
			}
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < skills.Count; j++)
				{
					if (skills[j].def == allDefsListForReading[i])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Log.Warning(pawn.ToStringSafe() + " had no " + allDefsListForReading[i].ToStringSafe() + " skill. Adding.");
					skills.Add(new SkillRecord(pawn, allDefsListForReading[i]));
				}
			}
		}

		public SkillRecord GetSkill(SkillDef skillDef)
		{
			for (int i = 0; i < skills.Count; i++)
			{
				if (skills[i].def == skillDef)
				{
					return skills[i];
				}
			}
			Log.Error(string.Concat("Did not find skill of def ", skillDef, ", returning ", skills[0]));
			return skills[0];
		}

		public void SkillsTick()
		{
			if (!pawn.IsHashIntervalTick(200))
			{
				return;
			}
			if (GenLocalDate.HourInteger(pawn) == 0 && (lastXpSinceMidnightResetTimestamp < 0 || Find.TickManager.TicksGame - lastXpSinceMidnightResetTimestamp >= 30000))
			{
				for (int i = 0; i < skills.Count; i++)
				{
					skills[i].xpSinceMidnight = 0f;
				}
				lastXpSinceMidnightResetTimestamp = Find.TickManager.TicksGame;
			}
			for (int j = 0; j < skills.Count; j++)
			{
				skills[j].Interval();
			}
		}

		public void Learn(SkillDef sDef, float xp, bool direct = false)
		{
			GetSkill(sDef).Learn(xp, direct);
		}

		public float AverageOfRelevantSkillsFor(WorkTypeDef workDef)
		{
			if (workDef.relevantSkills.Count == 0)
			{
				return 3f;
			}
			float num = 0f;
			for (int i = 0; i < workDef.relevantSkills.Count; i++)
			{
				num += (float)GetSkill(workDef.relevantSkills[i]).Level;
			}
			return num / (float)workDef.relevantSkills.Count;
		}

		public Passion MaxPassionOfRelevantSkillsFor(WorkTypeDef workDef)
		{
			if (workDef.relevantSkills.Count == 0)
			{
				return Passion.None;
			}
			Passion passion = Passion.None;
			for (int i = 0; i < workDef.relevantSkills.Count; i++)
			{
				Passion passion2 = GetSkill(workDef.relevantSkills[i]).passion;
				if ((int)passion2 > (int)passion)
				{
					passion = passion2;
				}
			}
			return passion;
		}

		public void Notify_SkillDisablesChanged()
		{
			for (int i = 0; i < skills.Count; i++)
			{
				skills[i].Notify_SkillDisablesChanged();
			}
		}
	}
}
