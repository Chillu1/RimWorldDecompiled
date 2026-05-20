using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualRoleTag : RitualRole
	{
		public string tag;

		public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
		{
			reason = null;
			if (ritual != null && p != null && p.Ideo != ritual.ideo)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustHaveIdeoToDoRole".Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(ritual.ideo.memberName), Find.ActiveLanguageWorker.WithIndefiniteArticle(base.Label));
				}
				return false;
			}
			if ((role != null && role.def.roleTags != null && role.def.roleTags.Contains(tag)) || (substitutable && p != null && ritual != null && p.Ideo == ritual.ideo))
			{
				if (p != null && !p.Faction.IsPlayerSafe())
				{
					if (!skipReason)
					{
						reason = "MessageRitualRoleMustBeColonist".Translate(base.Label);
					}
					return false;
				}
				return true;
			}
			if (p != null)
			{
				IEnumerable<PreceptDef> source = DefDatabase<PreceptDef>.AllDefsListForReading.Where((PreceptDef d) => typeof(Precept_Role).IsAssignableFrom(d.preceptClass) && d.roleTags != null && d.roleTags.Contains(tag));
				if (!skipReason)
				{
					reason = "MessageRitualRoleRequired".Translate(p) + ": " + source.Select((PreceptDef r) => ritual?.ideo.GetPrecept(r)?.LabelCap ?? ((string)r.LabelCap)).ToCommaList();
				}
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref tag, "tag");
		}
	}
}
