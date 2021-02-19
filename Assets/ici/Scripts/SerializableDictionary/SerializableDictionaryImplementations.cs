using System;

using UnityEngine;

// ---------------
//  String => Int
// ---------------
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> {}

// ---------------
//  GameObject => Float
// ---------------
[Serializable]
public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> {}

// ---------------
//  Material => Int
// ---------------
[Serializable]
public class MaterialIntDictionary : SerializableDictionary<Material, int> {}
