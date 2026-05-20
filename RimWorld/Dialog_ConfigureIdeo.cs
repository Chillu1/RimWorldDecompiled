using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_ConfigureIdeo : Window
	{
		public const float TitleAreaHeight = 45f;

		private static readonly Vector2 ChangeOrDeleteIdeoBtnSize = new Vector2(250f, 40f);

		private static readonly Vector2 AssignColonistsBtnSize = new Vector2(250f, 40f);

		private Action nextAction;

		private bool forArchonexusRestart;

		private Vector2 scrollPosition_ideoList;

		private float scrollViewHeight_ideoList;

		private Vector2 scrollPosition_ideoDetails;

		private float scrollViewHeight_ideoDetails;

		private Ideo initialPrimaryIdeo;

		private Ideo newPrimaryIdeo;

		private Ideo customOrLoadedIdeo;

		private Dictionary<Pawn, Ideo> pawnConvertToIdeo = new Dictionary<Pawn, Ideo>();

		private List<Pawn> pawns = new List<Pawn>();

		public override Vector2 InitialSize => new Vector2(1100f, Mathf.Min(1000f, UI.screenHeight));

		public Ideo CurrentPrimaryIdeo => newPrimaryIdeo ?? initialPrimaryIdeo;

		public Dialog_ConfigureIdeo(IEnumerable<Pawn> pawns, Action nextAction, bool forArchonexusRestart = false)
		{
			if (!ModLister.CheckIdeology("Configure ideo dialog"))
			{
				return;
			}
			forcePause = true;
			doCloseX = false;
			doCloseButton = false;
			closeOnClickedOutside = false;
			absorbInputAroundWindow = true;
			closeOnCancel = false;
			forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
			openMenuOnCancel = true;
			preventSave = true;
			this.nextAction = nextAction;
			this.forArchonexusRestart = forArchonexusRestart;
			this.pawns.AddRange(pawns);
			initialPrimaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
			foreach (Pawn pawn in pawns)
			{
				pawnConvertToIdeo[pawn] = null;
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			IdeoUIUtility.SetSelected(Faction.OfPlayer.ideos.PrimaryIdeo);
		}

		public override void PostClose()
		{
			base.PostClose();
			IdeoUIUtility.UnselectCurrent();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 45f), "ConfigureIdeoligion".Translate());
			Text.Font = GameFont.Small;
			IdeoUIUtility.DoIdeoListAndDetails(new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 55f - 45f), ref scrollPosition_ideoList, ref scrollViewHeight_ideoList, ref scrollPosition_ideoDetails, ref scrollViewHeight_ideoDetails, customOrLoadedIdeo != null, showCreateIdeoButton: true, pawns, customOrLoadedIdeo, delegate
			{
				CreateNewIdeo();
			}, forArchonexusRestart, (Pawn p) => pawnConvertToIdeo[p] ?? p.Ideo, delegate(Ideo ideo)
			{
				CheckRemoveNewIdeoAndMakePrimary(ideo);
				customOrLoadedIdeo = ideo;
			}, showLoadExistingIdeoBtn: false, allowLoad: true, delegate
			{
				CreateNewIdeo(fluid: true);
			});
			if (Widgets.ButtonText(new Rect(inRect.xMax - Window.CloseButSize.x - 18f, inRect.height - 55f, Window.CloseButSize.x, Window.CloseButSize.y), "Next".Translate()))
			{
				if (Faction.OfPlayer.ideos.PrimaryIdeo != CurrentPrimaryIdeo)
				{
					Faction.OfPlayer.ideos.SetPrimary(CurrentPrimaryIdeo);
				}
				foreach (Pawn pawn in pawns)
				{
					if (pawnConvertToIdeo[pawn] != null)
					{
						pawn.ideo.SetIdeo(pawnConvertToIdeo[pawn]);
					}
				}
				Close();
				nextAction();
			}
			IEnumerable<Pawn> source = pawns.Where((Pawn p) => p.IsColonist && p.Ideo != Faction.OfPlayer.ideos.PrimaryIdeo);
			if (source.Any() && Widgets.ButtonText(new Rect(inRect.center.x - AssignColonistsBtnSize.x / 2f, inRect.height - 55f, AssignColonistsBtnSize.x, AssignColonistsBtnSize.y), "AssignColonists".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ChooseColonistsForIdeo(CurrentPrimaryIdeo, source, (Pawn p) => p.Ideo != initialPrimaryIdeo && p.Ideo != CurrentPrimaryIdeo, (Pawn p) => p.Ideo, (Pawn p) => pawnConvertToIdeo[p] ?? p.Ideo, delegate(Pawn p, Ideo i)
				{
					if (p.Ideo == i)
					{
						pawnConvertToIdeo[p] = null;
					}
					else
					{
						pawnConvertToIdeo[p] = i;
					}
				}));
			}
			Rect rect = new Rect(inRect.x + 18f, inRect.height - 55f, ChangeOrDeleteIdeoBtnSize.x, ChangeOrDeleteIdeoBtnSize.y);
			if (customOrLoadedIdeo != null && IdeoUIUtility.selected == customOrLoadedIdeo && CurrentPrimaryIdeo == customOrLoadedIdeo)
			{
				if (Widgets.ButtonText(rect, "RemoveNewIdeoligion".Translate()))
				{
					CheckRemoveNewIdeoAndMakePrimary(initialPrimaryIdeo);
				}
			}
			else if (IdeoUIUtility.selected != CurrentPrimaryIdeo && Widgets.ButtonText(rect, "MakeIdeoligionPrimary".Translate()))
			{
				CheckRemoveNewIdeoAndMakePrimary(IdeoUIUtility.selected);
			}
		}

		private void CreateNewIdeo(bool fluid = false)
		{
			Ideo newIdeo = IdeoUtility.MakeEmptyIdeo();
			newIdeo.Fluid = fluid;
			Find.WindowStack.Add(new Dialog_ChooseMemes(newIdeo, MemeCategory.Structure, initialSelection: false, delegate
			{
				CheckRemoveNewIdeoAndMakePrimary(newIdeo);
				customOrLoadedIdeo = newIdeo;
			}));
		}

		private void RemoveNewIdeoIfCreated()
		{
			if (customOrLoadedIdeo != null)
			{
				Find.IdeoManager.Remove(customOrLoadedIdeo);
				customOrLoadedIdeo = null;
			}
		}

		private void CheckRemoveNewIdeoAndMakePrimary(Ideo primaryIdeo)
		{
			MakeIdeoPrimary(primaryIdeo);
			RemoveNewIdeoIfCreated();
			SoundDefOf.Click.PlayOneShotOnCamera();
		}

		private void MakeIdeoPrimary(Ideo primaryIdeo)
		{
			if (!Find.IdeoManager.IdeosListForReading.Contains(primaryIdeo))
			{
				Find.IdeoManager.Add(primaryIdeo);
			}
			foreach (Pawn pawn in pawns)
			{
				if (pawnConvertToIdeo[pawn] == CurrentPrimaryIdeo)
				{
					pawnConvertToIdeo[pawn] = primaryIdeo;
				}
				else if (pawnConvertToIdeo[pawn] == null && pawn.Ideo == initialPrimaryIdeo)
				{
					pawnConvertToIdeo[pawn] = primaryIdeo;
				}
			}
			newPrimaryIdeo = primaryIdeo;
			IdeoUIUtility.SetSelected(primaryIdeo);
		}
	}
}
