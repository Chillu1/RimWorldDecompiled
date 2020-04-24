using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale : IExposable, ILoadReferenceable
	{
		public TaleDef def;

		public int id;

		private int uses;

		public int date = -1;

		public TaleData_Surroundings surroundings;

		public string customLabel;

		public int AgeTicks => Find.TickManager.TicksAbs - date;

		public int Uses => uses;

		public bool Unused => uses == 0;

		public virtual Pawn DominantPawn => null;

		public float InterestLevel
		{
			get
			{
				float baseInterest = def.baseInterest;
				baseInterest /= (float)(1 + uses * 3);
				float a = 0f;
				switch (def.type)
				{
				case TaleType.Volatile:
					a = 50f;
					break;
				case TaleType.PermanentHistorical:
					a = 50f;
					break;
				case TaleType.Expirable:
					a = def.expireDays;
					break;
				}
				float value = AgeTicks / 60000;
				baseInterest *= Mathf.InverseLerp(a, 0f, value);
				if (baseInterest < 0.01f)
				{
					baseInterest = 0.01f;
				}
				return baseInterest;
			}
		}

		public bool Expired
		{
			get
			{
				if (!Unused)
				{
					return false;
				}
				if (def.type != TaleType.Expirable)
				{
					return false;
				}
				return (float)AgeTicks > def.expireDays * 60000f;
			}
		}

		public virtual string ShortSummary
		{
			get
			{
				if (!customLabel.NullOrEmpty())
				{
					return customLabel.CapitalizeFirst();
				}
				return def.LabelCap;
			}
		}

		public virtual void GenerateTestData()
		{
			if (Find.CurrentMap == null)
			{
				Log.Error("Can't generate test data because there is no map.");
			}
			date = Rand.Range(-108000000, -7200000);
			surroundings = TaleData_Surroundings.GenerateRandom(Find.CurrentMap);
		}

		public virtual bool Concerns(Thing th)
		{
			return false;
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref id, "id", 0);
			Scribe_Values.Look(ref uses, "uses", 0);
			Scribe_Values.Look(ref date, "date", 0);
			Scribe_Deep.Look(ref surroundings, "surroundings");
			Scribe_Values.Look(ref customLabel, "customLabel");
		}

		public void Notify_NewlyUsed()
		{
			uses++;
		}

		public void Notify_ReferenceDestroyed()
		{
			if (uses == 0)
			{
				Log.Warning("Called reference destroyed method on tale " + this + " but uses count is 0.");
			}
			else
			{
				uses--;
			}
		}

		public IEnumerable<RulePack> GetTextGenerationIncludes()
		{
			if (def.rulePack != null)
			{
				yield return def.rulePack;
			}
		}

		public IEnumerable<Rule> GetTextGenerationRules()
		{
			Vector2 location = Vector2.zero;
			if (surroundings != null && surroundings.tile >= 0)
			{
				location = Find.WorldGrid.LongLatOf(surroundings.tile);
			}
			yield return new Rule_String("DATE", GenDate.DateFullStringAt(date, location));
			if (surroundings != null)
			{
				foreach (Rule rule in surroundings.GetRules())
				{
					yield return rule;
				}
			}
			foreach (Rule item in SpecialTextGenerationRules())
			{
				yield return item;
			}
		}

		protected virtual IEnumerable<Rule> SpecialTextGenerationRules()
		{
			yield break;
		}

		public string GetUniqueLoadID()
		{
			return "Tale_" + id;
		}

		public override int GetHashCode()
		{
			return id;
		}

		public override string ToString()
		{
			string str = "(#" + id + ": " + ShortSummary + "(age=" + ((float)AgeTicks / 60000f).ToString("F2") + " interest=" + InterestLevel;
			if (Unused && def.type == TaleType.Expirable)
			{
				str = str + ", expireDays=" + def.expireDays.ToString("F2");
			}
			return str + ")";
		}
	}
}
