using UnityEngine;

namespace Verse
{
	public interface ICellBoolGiver
	{
		Color Color
		{
			get;
		}

		bool GetCellBool(int index);

		Color GetCellExtraColor(int index);
	}
}
