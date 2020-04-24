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
				if (quest != null)
				{
					yield return Option_ViewInQuestsTab();
				}
				if (!hyperlinkThingDefs.NullOrEmpty())
				{
					for (int i = 0; i < hyperlinkThingDefs.Count; i++)
					{
						yield return Option_ViewInfoCard(i);
					}
				}
			}
		}
	}
}
