using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Daftar : MonoBehaviour
{
    [SerializeField] private BackendManager backendManager;
    [SerializeField] private TMP_InputField usernameInput, passwordInput, confirmPasswordInput;

    public void RegisterUser()
    {
        if (passwordInput.text != confirmPasswordInput.text)
        {
            Debug.LogError("Passwords do not match!");
            return;
        }

        backendManager.CreateUser(usernameInput.text, passwordInput.text);
    }
}
