using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Apparel : ThingWithComps
	{
		private bool wornByCorpseInt;

		public Pawn Wearer => (base.ParentHolder as Pawn_ApparelTracker)?.pawn;

		public bool WornByCorpse => wornByCorpseInt;

		public override string DescriptionDetailed
		{
			get
			{
				string text = base.DescriptionDetailed;
				if (WornByCorpse)
				{
					text += "\n" + "WasWornByCorpse".Translate();
				}
				return text;
			}
		}

		public void Notify_PawnKilled()
		{
			if (def.apparel.careIfWornByCorpse)
			{
				wornByCorpseInt = true;
			}
		}

		public void Notify_PawnResurrected()
		{
			wornByCorpseInt = false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref wornByCorpseInt, "wornByCorpse", defaultValue: false);
		}

		public virtual void DrawWornExtras()
		{
		}

		public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			return false;
		}

		public virtual bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb)
		{
			return true;
		}

		public virtual IEnumerable<Gizmo> GetWornGizmos()
		{
			yield break;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			RoyalTitleDef royalTitleDef = (from t in DefDatabase<FactionDef>.AllDefsListForReading.SelectMany((FactionDef f) => f.RoyalTitlesAwardableInSeniorityOrderForReading)
				where t.requiredApparel != null && t.requiredApparel.Any((RoyalTitleDef.ApparelRequirement req) => req.ApparelMeetsRequirement(def, allowUnmatched: false))
				orderby t.seniority descending
				select t).FirstOrDefault();
			if (royalTitleDef != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_MaxSatisfiedTitle".Translate(), royalTitleDef.GetLabelCapForBothGenders(), "Stat_Thing_Apparel_MaxSatisfiedTitle_Desc".Translate(), 2752, null, new Dialog_InfoCard.Hyperlink[1]
				{
					new Dialog_InfoCard.Hyperlink(royalTitleDef)
				});
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (WornByCorpse)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text += "WasWornByCorpse".Translate();
			}
			return text;
		}

		public virtual float GetSpecialApparelScoreOffset()
		{
			return 0f;
		}
	}
}
