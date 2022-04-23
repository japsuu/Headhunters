using TMPro;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]
    private TMP_InputField usernameInputField;

    private void Start()
    {
        usernameInputField.onEndEdit.AddListener(UserDataManager.SaveUsername);

        usernameInputField.text = UserDataManager.LoadUsername();
    }
}
