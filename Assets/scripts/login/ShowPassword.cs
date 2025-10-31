using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowPassword : MonoBehaviour
{
    // public Button toggleButton;
    // public Sprite showIcon;
    // public Sprite hideIcon;

    private bool isPasswordVisible = false;

    public void TogglePasswordVisibility(TMP_InputField passwordInputField)
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            // toggleButton.image.sprite = hideIcon;
        }
        else
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            // toggleButton.image.sprite = showIcon;
        }

        // Force the input field to update its display
        passwordInputField.ForceLabelUpdate();
    }
}