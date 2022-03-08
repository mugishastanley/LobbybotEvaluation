// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;

namespace CLARTE.Backport.Collections
{
	public interface IStructuralEquatable
	{
		Boolean Equals(Object other, IEqualityComparer comparer);
		int GetHashCode(IEqualityComparer comparer);
	}
}
