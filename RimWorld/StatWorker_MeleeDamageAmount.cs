using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class StatWorker_MeleeDamageAmount : StatWorker
	{
		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			float num = base.GetValueUnfinalized(req);
			ThingDef def = (ThingDef)req.Def;
			if (req.StuffDef != null)
			{
				StatDef statDef = null;
				if (CategoryOfDamage(def) != null)
				{
					statDef = CategoryOfDamage(def).multStat;
				}
				if (statDef != null)
				{
					num *= req.StuffDef.GetStatValueAbstract(statDef);
				}
			}
			return num;
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetExplanationUnfinalized(req, numberSense));
			ThingDef def = (ThingDef)req.Def;
			if (req.StuffDef != null)
			{
				StatDef statDef = null;
				if (CategoryOfDamage(def) != null)
				{
					statDef = CategoryOfDamage(def).multStat;
				}
				if (statDef != null)
				{
					stringBuilder.AppendLine(req.StuffDef.LabelCap + ": x" + req.StuffDef.GetStatValueAbstract(statDef).ToStringPercent());
				}
			}
			return stringBuilder.ToString();
		}

		protected abstract DamageArmorCategoryDef CategoryOfDamage(ThingDef def);
	}
}
