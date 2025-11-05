using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinPicker : MonoBehaviour
{
    enum SkinIndex
    {
        NONE = 0,
        BLUE = 1,
        GREEN = 2,
        RED = 3
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] skins;

    int index = 0;
    
    private void Start()
    {
        // Load current skin dari DataContainer
        LoadCurrentSkin();
    }
    
    private void LoadCurrentSkin()
    {
        if (DataContainer.Instance != null)
        {
            // Determine current equipped skin
            if (DataContainer.Instance.redSkin == 1)
                index = (int)SkinIndex.RED;
            else if (DataContainer.Instance.greenSkin == 1)
                index = (int)SkinIndex.GREEN;
            else if (DataContainer.Instance.blueSkin == 1)
                index = (int)SkinIndex.BLUE;
            else
                index = (int)SkinIndex.NONE;
            
            // Update sprite
            if (index >= 0 && index < skins.Length)
                spriteRenderer.sprite = skins[index];
        }
    }

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

    public void UpdateSkin()
    {
        if (DataContainer.Instance != null)
        {
            int red, green, blue;

            red = index == (int)SkinIndex.RED ? 1 : 0;
            green = index == (int)SkinIndex.GREEN ? 1 : 0;
            blue = index == (int)SkinIndex.BLUE ? 1 : 0;
            
            // Update ke DataContainer (akan auto-sync ke backend)
            DataContainer.Instance.UpdateSkins(green, red, blue);
            
            Debug.Log($"Skin updated: Red={red}, Green={green}, Blue={blue}");
        }
        else
        {
            Debug.LogError("DataContainer.Instance is null! Cannot update skin.");
        }
    }
}
