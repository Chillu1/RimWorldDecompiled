using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugTables
	{
		public static void MakeTablesDialog<T>(IEnumerable<T> dataSources, params TableDataGetter<T>[] getters)
		{
			List<TableDataGetter<T>> list = getters.ToList();
			int num = dataSources.Count() + 1;
			int count = list.Count;
			string[,] array = new string[count, num];
			int num2 = 0;
			foreach (TableDataGetter<T> tableDataGetter in getters)
			{
				array[num2, 0] = tableDataGetter.label;
				num2++;
			}
			int num3 = 1;
			foreach (T dataSource in dataSources)
			{
				for (int j = 0; j < count; j++)
				{
					array[j, num3] = list[j].getter(dataSource);
				}
				num3++;
			}
			Find.WindowStack.Add(new Window_DebugTable(array));
		}

		public static void MakeTablesDialog<TColumn, TRow>(IEnumerable<TColumn> colValues, Func<TColumn, string> colLabelFormatter, IEnumerable<TRow> rowValues, Func<TRow, string> rowLabelFormatter, Func<TColumn, TRow, string> func, string tlLabel = "")
		{
			int num = colValues.Count() + 1;
			int num2 = rowValues.Count() + 1;
			string[,] array = new string[num, num2];
			array[0, 0] = tlLabel;
			int num3 = 1;
			foreach (TColumn colValue in colValues)
			{
				array[num3, 0] = colLabelFormatter(colValue);
				num3++;
			}
			int num4 = 1;
			foreach (TRow rowValue in rowValues)
			{
				array[0, num4] = rowLabelFormatter(rowValue);
				num4++;
			}
			int num5 = 1;
			foreach (TRow rowValue2 in rowValues)
			{
				int num6 = 1;
				foreach (TColumn colValue2 in colValues)
				{
					array[num6, num5] = func(colValue2, rowValue2);
					num6++;
				}
				num5++;
			}
			Find.WindowStack.Add(new Window_DebugTable(array));
		}
	}
}
