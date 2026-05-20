using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Page_ConfigureFluidIdeo : Page_ConfigureIdeo
{
	public override void PostOpen()
	{
		IdeoUIUtility.selected = Faction.OfPlayer.ideos?.PrimaryIdeo;
		base.PostOpen();
	}

	public override void DoIdeos(Rect rect)
	{
		IdeoUIUtility.DoIdeoListAndDetails(GetMainRect(rect), ref scrollPosition_ideoList, ref scrollViewHeight_ideoList, ref scrollPosition_ideoDetails, ref scrollViewHeight_ideoDetails, editMode: true, showCreateIdeoButton: false, null, null, null, forArchonexusRestart: false, null, loadFluidOrNpcIdeo);
		void loadFluidOrNpcIdeo(Ideo loadedIdeo)
		{
			List<Faction> factionsWithIdeo = Find.IdeoManager.GetFactionsWithIdeo(IdeoUIUtility.selected, onlyPrimary: true, onlyNpcFactions: true);
			if (factionsWithIdeo.Count == 0)
			{
				loadedIdeo.Fluid = true;
				if (TryFindIncompatibleMemes(loadedIdeo, out var memes))
				{
					TaggedString text = "ConfirmChangeMemesForFluidIdeo".Translate();
					if (!memes.NullOrEmpty())
					{
						string text2 = memes.Select((MemeDef m) => m.LabelCap.Resolve()).ToLineList("- ");
						text += " " + "ConfirmIncompatibleMemes".Translate() + ":\n\n" + text2;
					}
					text += "\n\n" + "ConfirmChangeMemesForFluidIdeoContinue".Translate();
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
					{
						loadedIdeo.memes.RemoveAll((MemeDef m) => m.category == MemeCategory.Normal);
						loadedIdeo.ClearPrecepts();
						Find.WindowStack.Add(new Dialog_ChooseMemes(loadedIdeo, MemeCategory.Normal, initialSelection: true, delegate
						{
							SetLoadedIdeo(loadedIdeo);
						}));
					}));
				}
				else
				{
					SetLoadedIdeo(loadedIdeo);
				}
			}
			else
			{
				if (!Find.IdeoManager.IdeosListForReading.Contains(loadedIdeo))
				{
					Find.IdeoManager.Add(loadedIdeo);
				}
				IdeoUIUtility.SetSelected(loadedIdeo);
				foreach (Faction item in factionsWithIdeo)
				{
					item.ideos.SetPrimary(loadedIdeo);
				}
				Find.IdeoManager.RemoveUnusedStartingIdeos();
			}
		}
	}

	private void SetLoadedIdeo(Ideo loadedIdeo)
	{
		ideo = loadedIdeo;
		IdeoUIUtility.MakeLoadedIdeoPrimary(loadedIdeo);
		ideo.Fluid = true;
	}

	private bool TryFindIncompatibleMemes(Ideo ideo, out List<MemeDef> memes)
	{
		memes = null;
		if (ideo.memes.Count((MemeDef m) => m.category == MemeCategory.Normal) > IdeoFoundation.MemeCountRangeFluidAbsolute.max)
		{
			return true;
		}
		for (int num = 0; num < ideo.memes.Count; num++)
		{
			if (!IdeoUtility.IsMemeAllowedForInitialFluidIdeo(ideo.memes[num]))
			{
				if (memes == null)
				{
					memes = new List<MemeDef>();
				}
				memes.Add(ideo.memes[num]);
			}
		}
		if (memes != null)
		{
			return memes.Count > 0;
		}
		return false;
	}

	public override void Notify_ClosedChooseMemesDialog()
	{
		if (ideo != null && !ideo.memes.Any((MemeDef x) => x.category == MemeCategory.Normal) && prev != null)
		{
			DoBack();
		}
	}
}
