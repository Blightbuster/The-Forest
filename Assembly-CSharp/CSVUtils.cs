using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CSVUtils
{
	public static List<List<string>> SubArea(List<List<string>> source, int startX, int startY, int countX = 2147483647, int countY = 2147483647)
	{
		int count = source.Count;
		if (startY >= count)
		{
			return new List<List<string>>();
		}
		int num = (countY != int.MaxValue) ? Mathf.Min(count, startY + countY) : count;
		List<List<string>> list = new List<List<string>>();
		for (int i = startY; i < num; i++)
		{
			List<string> list2 = source[i];
			int num2 = Mathf.Min(list2.Count - startX, countX);
			if (num2 > 0)
			{
				list.Add(list2.GetRange(startX, num2));
			}
		}
		return list;
	}

	public static List<List<string>> SplitCellsAndLines(string source, string newLineSequence, char deliminator, char encapsulate, char escape)
	{
		bool flag = false;
		bool flag2 = false;
		List<List<string>> result = new List<List<string>>();
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		if (source.Length > 0)
		{
			list = new List<string>();
		}
		int num = 0;
		foreach (char c in source)
		{
			bool flag3 = c == encapsulate;
			if (flag && escape == encapsulate && !flag3)
			{
				flag = false;
				flag2 = false;
			}
			if (c == newLineSequence[num] && !flag2)
			{
				if (num++ >= newLineSequence.Length - 1)
				{
					num = 0;
					CSVUtils.FinalizeCurrentCell(ref list, ref stringBuilder);
					CSVUtils.FinalizeCurrentLine(ref result, ref list);
				}
			}
			else
			{
				if (num > 0)
				{
					stringBuilder.Append(newLineSequence.Substring(0, num));
					num = 0;
				}
				bool flag4 = c == escape;
				if (flag2 && flag4 && !flag)
				{
					flag = true;
				}
				else if (flag3 && !flag)
				{
					flag2 = !flag2;
				}
				else
				{
					bool flag5 = c == deliminator;
					if (flag5 && !flag2)
					{
						CSVUtils.FinalizeCurrentCell(ref list, ref stringBuilder);
					}
					else
					{
						flag = false;
						stringBuilder.Append(c);
					}
				}
			}
		}
		CSVUtils.FinalizeCurrentCell(ref list, ref stringBuilder);
		CSVUtils.FinalizeCurrentLine(ref result, ref list);
		return result;
	}

	private static void FinalizeCurrentLine(ref List<List<string>> result, ref List<string> currentLine)
	{
		result.Add(currentLine);
		currentLine = new List<string>();
	}

	private static void FinalizeCurrentCell(ref List<string> currentLine, ref StringBuilder currentCell)
	{
		currentLine.Add(currentCell.ToString());
		currentCell.Length = 0;
	}
}
