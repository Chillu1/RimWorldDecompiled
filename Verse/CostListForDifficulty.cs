using System.Collections.Generic;
using System.Reflection;
using RimWorld;

namespace Verse
{
	public class CostListForDifficulty
	{
		public string difficultyVar;

		public List<ThingDefCountClass> costList;

		public int costStuffCount;

		public bool invert;

		private bool cachedApplies;

		private Difficulty cachedDifficulty;

		public bool Applies
		{
			get
			{
				if (Find.Storyteller == null)
				{
					return false;
				}
				if (cachedDifficulty != Find.Storyteller.difficulty)
				{
					RecacheApplies();
				}
				return cachedApplies;
			}
		}

		public void RecacheApplies()
		{
			cachedDifficulty = Find.Storyteller.difficulty;
			if (difficultyVar.NullOrEmpty())
			{
				cachedApplies = false;
				return;
			}
			FieldInfo field = typeof(Difficulty).GetField(difficultyVar, BindingFlags.Instance | BindingFlags.Public);
			cachedApplies = (bool)field.GetValue(cachedDifficulty);
			if (invert)
			{
				cachedApplies = !cachedApplies;
			}
		}
	}
}
