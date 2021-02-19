using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour {

	public Renderer[] touchableObjects;

	public Renderer[] visualObjects;

	public Material[] materials;

	protected int[] currentMaterialId;

	void Start()
	{
		if (touchableObjects == null || materials == null)
		{
			return;
		}
		currentMaterialId = new int[touchableObjects.Length];

		for (int j = 0; j < touchableObjects.Length; j++)
		{
			// default value
			currentMaterialId[j] = 0;

			for (int i = 0; i < materials.Length; i++)
			{
				Material initial_material = touchableObjects[j].sharedMaterial;

				if (initial_material == materials [i])
				{
					currentMaterialId [j] = i;

					break;
				}
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (currentMaterialId == null)
		{
			return;
		}

		bool update_needed = false;

		int selected_object = 0;
		int[] forbidden_materials_ids = { 1 };

		if (Input.GetKeyDown (KeyCode.F1))
		{
			selected_object = 0;
			forbidden_materials_ids = new int[] { 0, 3, 4, 5 };

			update_needed = true;
		}

		if (Input.GetKeyDown (KeyCode.F2))
		{
			selected_object = 2;
			forbidden_materials_ids = new int[] { 0, 4, 5 };

			update_needed = true;
		}

		if (update_needed)
		{
			CycleMaterials (selected_object, forbidden_materials_ids);

			Debug.Log ("Current material for " + touchableObjects [selected_object].name + ": " + touchableObjects [selected_object].sharedMaterial.name);
		}
	}

	void CycleMaterials(int selected_object, int[] forbidden_materials_ids)
	{
		int new_material_id = currentMaterialId [selected_object];

		new_material_id++;

		if (new_material_id > materials.Length - 1)
		{
			new_material_id = 0;
		}

		while(IsMaterialForbidden(new_material_id, forbidden_materials_ids))
		{
			new_material_id++;

			if (new_material_id > materials.Length - 1)
			{
				new_material_id = 0;
			}
		}



		currentMaterialId [selected_object] = new_material_id;

		touchableObjects [selected_object].sharedMaterial = materials [currentMaterialId [selected_object]];

		visualObjects [selected_object].sharedMaterial = materials [currentMaterialId [selected_object]];
	}

	bool IsMaterialForbidden(int candidate_material_id, int[] forbidden_materials_ids)
	{
		for (int i = 0; i < forbidden_materials_ids.Length; i++)
		{
			if (candidate_material_id == forbidden_materials_ids [i])
			{
				return true;
			}
		}

		return false;
	}
}
