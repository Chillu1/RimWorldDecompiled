using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public abstract class LogEntry_DamageResult : LogEntry
	{
		protected List<BodyPartRecord> damagedParts;

		protected List<bool> damagedPartsDestroyed;

		protected bool deflected;

		public LogEntry_DamageResult(LogEntryDef def = null)
			: base(def)
		{
		}

		public void FillTargets(List<BodyPartRecord> recipientParts, List<bool> recipientPartsDestroyed, bool deflected)
		{
			damagedParts = recipientParts;
			damagedPartsDestroyed = recipientPartsDestroyed;
			this.deflected = deflected;
			ResetCache();
		}

		protected virtual BodyDef DamagedBody()
		{
			return null;
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			result.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("recipient_part", DamagedBody(), damagedParts, damagedPartsDestroyed, result.Constants));
			result.Constants.Add("deflected", deflected.ToString());
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref damagedParts, "damagedParts", LookMode.BodyPart);
			Scribe_Collections.Look(ref damagedPartsDestroyed, "damagedPartsDestroyed", LookMode.Value);
			Scribe_Values.Look(ref deflected, "deflected", defaultValue: false);
			if (Scribe.mode != LoadSaveMode.PostLoadInit || damagedParts == null)
			{
				return;
			}
			for (int num = damagedParts.Count - 1; num >= 0; num--)
			{
				if (damagedParts[num] == null)
				{
					damagedParts.RemoveAt(num);
					if (num < damagedPartsDestroyed.Count)
					{
						damagedPartsDestroyed.RemoveAt(num);
					}
				}
			}
		}
	}
}
