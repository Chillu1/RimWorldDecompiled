using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_ConfirmModUpload : Dialog_MessageBox
{
	private ModMetaData mod;

	public Dialog_ConfirmModUpload(ModMetaData mod, Action acceptAction)
		: base("ConfirmSteamWorkshopUpload".Translate(), "Confirm".Translate(), acceptAction, "GoBack".Translate(), null, null, buttonADestructive: true, acceptAction)
	{
		this.mod = mod;
	}

	public override void DoWindowContents(Rect inRect)
	{
		base.DoWindowContents(inRect);
		Vector2 topLeft = new Vector2(inRect.x + 10f, inRect.height - 35f - 24f - 10f);
		Widgets.Checkbox(topLeft, ref mod.translationMod);
		Widgets.Label(new Rect(topLeft.x + 24f + 10f, topLeft.y + (24f - Text.LineHeight) / 2f, inRect.width / 2f, 24f), "TagAsTranslation".Translate());
	}
}
