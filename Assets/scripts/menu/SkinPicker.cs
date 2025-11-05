using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinPicker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] skins;

    int index = 0;

    public void NextSkin()
    {
        index = (index + 1) % skins.Length;
        spriteRenderer.sprite = skins[index];
    }

    public void SetSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < skins.Length)
        {
            index = skinIndex;
            spriteRenderer.sprite = skins[index];
        }
    }
}
