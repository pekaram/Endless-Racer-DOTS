using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdBillboard : UpperBillboard
{
    [SerializeField]
    private Material adMaterial;

    [SerializeField]
    private List<Texture> textures;

    protected override void OnSpawn(int remainingDistance)
    {
        this.adMaterial.SetTexture("_MainTex", textures[Random.Range(0, this.textures.Count - 1)]);
    }
}
