﻿using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class ArrayExtensions
	{
		public static void Populate<T>(this T[] arr, T value)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = value;
			}
		}
	}
}
