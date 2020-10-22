using System.Collections.Generic;

namespace Verse
{
	public class StandardLetter : ChoiceLetter
	{
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				if (lookTargets.IsValid())
				{
					yield return base.Option_JumpToLocation;
				}
				if (quest != null && !quest.hidden)
				{
					yield return Option_ViewInQuestsTab();
				}
				if (!hyperlinkThingDefs.NullOrEmpty())
				{
					for (int k = 0; k < hyperlinkThingDefs.Count; k++)
					{
						yield return Option_ViewInfoCard(k);
					}
				}
				if (!hyperlinkHediffDefs.NullOrEmpty())
				{
					int k = ((hyperlinkThingDefs != null) ? hyperlinkThingDefs.Count : 0);
					for (int i = 0; i < hyperlinkHediffDefs.Count; i++)
					{
						yield return Option_ViewInfoCard(k + i);
					}
				}
			}
		}
	}
}
