using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_GameEnded : ChoiceLetter
{
	public override bool CanDismissWithRightClick => false;

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (base.ArchivedOnly)
			{
				yield return base.Option_Close;
				yield break;
			}
			float hoursRemaining;
			if (!Find.GameEnder.CanSpawnNewWanderers())
			{
				yield return new DiaOption("GameOverKeepWatching".Translate())
				{
					resolveTree = true
				};
			}
			else
			{
				yield return new DiaOption("GameOverKeepWatchingForNow".Translate())
				{
					resolveTree = true
				};
				hoursRemaining = (float)(20000 - (GenTicks.TicksGame - arrivalTick)) / 2500f;
				yield return new DiaOption("GameOverCreateNewWanderers".Translate())
				{
					action = delegate
					{
						Find.WindowStack.Add(new Dialog_ChooseNewWanderers());
					},
					resolveTree = true,
					disabled = !CanCreateNewWanderers(),
					disabledReason = CanCreateNewWanderers().Reason
				};
			}
			yield return new DiaOption("GameOverMainMenu".Translate())
			{
				action = GenScene.GoToMainMenu,
				resolveTree = true
			};
			AcceptanceReport CanCreateNewWanderers()
			{
				if (hoursRemaining > 0f)
				{
					return "GameOverCreateNewWanderersWait".Translate(Math.Ceiling(hoursRemaining));
				}
				bool flag = false;
				if (Current.Game == null)
				{
					return false;
				}
				foreach (Map playerHomeMap in Current.Game.PlayerHomeMaps)
				{
					if (playerHomeMap.Tile.Layer.IsRootSurface)
					{
						return true;
					}
					flag = true;
				}
				if (flag)
				{
					return "NoWandererDestination".Translate();
				}
				return "NoColony".Translate();
			}
		}
	}
}
