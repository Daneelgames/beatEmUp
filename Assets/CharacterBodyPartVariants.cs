using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterBodyPartVariants : MonoBehaviour
{
    [SerializeField] MeshRenderer nakedVariantToDisable;
    [SerializeField] private float nakedChance = 0.5f;
    
    [Header("Choose One")]
    [SerializeField] List<MeshRenderer> variants;
    [SerializeField] List<GameObject> variantsGO;

    void Start()
    {
        if (nakedVariantToDisable != null && Random.value < nakedChance)
        {
            // naked
            if (variants.Count > 0)
            {
                for (int i = variants.Count - 1; i >= 0; i--)
                {
                    if (i >= variants.Count)
                        continue;

                    if (variants[i] != null)
                    {
                        Destroy(variants[i].gameObject);
                    }

                    variants.RemoveAt(i);
                }
            }
            else if (variantsGO.Count > 0)
            {
                for (int i = variantsGO.Count - 1; i >= 0; i--)
                {
                    if (i >= variantsGO.Count)
                        continue;

                    if (variantsGO[i] != null)
                    {
                        Destroy(variantsGO[i]);
                    }

                    variantsGO.RemoveAt(i);
                }
            }
        }
        else
        {
            // choose
            if (nakedVariantToDisable)
                nakedVariantToDisable.enabled = false;

            if (variants.Count > 0)
            {
                int r = Random.Range(0, variants.Count);
                for (int i = variants.Count - 1; i >= 0; i--)
                {
                    if (i >= variants.Count)
                        continue;

                    if (r == i)
                    {
                        variants[i].gameObject.SetActive(true);
                        continue;
                    }

                    if (variants[i] != null)
                    {
                        Destroy(variants[i].gameObject);
                    }

                    variants.RemoveAt(i);
                }
            }
            else if (variantsGO.Count > 0)
            {
                int r = Random.Range(0, variantsGO.Count);
                for (int i = variantsGO.Count - 1; i >= 0; i--)
                {
                    if (i >= variantsGO.Count)
                        continue;

                    if (r == i)
                    {
                        variantsGO[i].gameObject.SetActive(true);
                        continue;
                    }

                    if (variantsGO[i] != null)
                    {
                        Destroy(variantsGO[i].gameObject);
                    }

                    variantsGO.RemoveAt(i);
                }
            }
        }
    }
}