// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;

namespace CLARTE.Backport.Collections
{
	public interface IStructuralComparable
	{
		Int32 CompareTo(Object other, IComparer comparer);
	}
}
