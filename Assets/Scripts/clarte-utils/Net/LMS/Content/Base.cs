using System;
using UnityEngine;

namespace CLARTE.Net.LMS.Content
{
    public abstract class Base : ScriptableObject
    {
		#region Getters / Setters
		public Guid Guid => GetType().GUID;
		#endregion
	}
}
