using System.Text;
using Verse;

namespace RimWorld.Planet;

public static class CaravanAbandonOrBanishUtility
{
	public static void TryAbandonOrBanishViaInterface(Thing t, Caravan caravan)
	{
		Pawn p = t as Pawn;
		if (p != null)
		{
			if (!caravan.PawnsListForReading.Any((Pawn x) => x != p && caravan.IsOwner(x)))
			{
				Messages.Message("MessageCantBanishLastColonist".Translate(), caravan, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				PawnBanishUtility.ShowBanishPawnConfirmationDialog(p);
			}
			return;
		}
		Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("ConfirmAbandonItemDialog".Translate(t.Label), delegate
		{
			Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, t);
			if (ownerOf == null)
			{
				Log.Error("Could not find owner of " + t);
			}
			else
			{
				t.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
				ownerOf.inventory.innerContainer.Remove(t);
				t.Destroy();
				caravan.RecacheInventory();
			}
		}, destructive: true);
		Find.WindowStack.Add(window);
	}

	public static void TryAbandonOrBanishViaInterface(TransferableImmutable t, Caravan caravan)
	{
		if (t.AnyThing is Pawn t2)
		{
			TryAbandonOrBanishViaInterface(t2, caravan);
			return;
		}
		Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("ConfirmAbandonItemDialog".Translate(t.LabelWithTotalStackCount), delegate
		{
			for (int i = 0; i < t.things.Count; i++)
			{
				Thing thing = t.things[i];
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
				if (ownerOf == null)
				{
					Log.Error("Could not find owner of " + thing);
					return;
				}
				thing.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
				ownerOf.inventory.innerContainer.Remove(thing);
				thing.Destroy();
			}
			caravan.RecacheInventory();
		}, destructive: true);
		Find.WindowStack.Add(window);
	}

	public static void TryAbandonSpecificCountViaInterface(Thing t, Caravan caravan)
	{
		Find.WindowStack.Add(new Dialog_Slider("AbandonSliderText".Translate(t.LabelNoCount), 1, t.stackCount, delegate(int x)
		{
			Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, t);
			if (ownerOf == null)
			{
				Log.Error("Could not find owner of " + t);
			}
			else
			{
				if (x >= t.stackCount)
				{
					t.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
					ownerOf.inventory.innerContainer.Remove(t);
					t.Destroy();
				}
				else
				{
					Thing thing = t.SplitOff(x);
					thing.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
					thing.Destroy();
				}
				caravan.RecacheInventory();
			}
		}));
	}

	public static void TryAbandonSpecificCountViaInterface(TransferableImmutable t, Caravan caravan)
	{
		Find.WindowStack.Add(new Dialog_Slider("AbandonSliderText".Translate(t.Label), 1, t.TotalStackCount, delegate(int x)
		{
			int num = x;
			for (int i = 0; i < t.things.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				Thing thing = t.things[i];
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
				if (ownerOf == null)
				{
					Log.Error("Could not find owner of " + thing);
					return;
				}
				if (num >= thing.stackCount)
				{
					num -= thing.stackCount;
					thing.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
					ownerOf.inventory.innerContainer.Remove(thing);
					thing.Destroy();
				}
				else
				{
					Thing thing2 = thing.SplitOff(num);
					thing2.Notify_AbandonedAtTile(caravan.GetTileCurrentlyOver());
					thing2.Destroy();
					num = 0;
				}
			}
			caravan.RecacheInventory();
		}));
	}

	public static string GetAbandonOrBanishButtonTooltip(Thing t, bool abandonSpecificCount)
	{
		if (t is Pawn pawn)
		{
			return PawnBanishUtility.GetBanishButtonTip(pawn);
		}
		return GetAbandonItemButtonTooltip(t.stackCount, abandonSpecificCount);
	}

	public static string GetAbandonOrBanishButtonTooltip(TransferableImmutable t, bool abandonSpecificCount)
	{
		if (t.AnyThing is Pawn pawn)
		{
			return PawnBanishUtility.GetBanishButtonTip(pawn);
		}
		return GetAbandonItemButtonTooltip(t.TotalStackCount, abandonSpecificCount);
	}

	private static string GetAbandonItemButtonTooltip(int currentStackCount, bool abandonSpecificCount)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (currentStackCount == 1)
		{
			stringBuilder.AppendLine("AbandonTip".Translate());
		}
		else if (abandonSpecificCount)
		{
			stringBuilder.AppendLine("AbandonSpecificCountTip".Translate());
		}
		else
		{
			stringBuilder.AppendLine("AbandonAllTip".Translate());
		}
		stringBuilder.AppendLine();
		stringBuilder.Append("AbandonItemTipExtraText".Translate());
		return stringBuilder.ToString();
	}
}
