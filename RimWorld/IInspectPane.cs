using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public interface IInspectPane
	{
		float RecentHeight
		{
			get;
			set;
		}

		Type OpenTabType
		{
			get;
			set;
		}

		bool AnythingSelected
		{
			get;
		}

		IEnumerable<InspectTabBase> CurTabs
		{
			get;
		}

		bool ShouldShowSelectNextInCellButton
		{
			get;
		}

		bool ShouldShowPaneContents
		{
			get;
		}

		float PaneTopY
		{
			get;
		}

		void DrawInspectGizmos();

		string GetLabel(Rect rect);

		void DoInspectPaneButtons(Rect rect, ref float lineEndWidth);

		void SelectNextInCell();

		void DoPaneContents(Rect rect);

		void CloseOpenTab();

		void Reset();
	}
}
