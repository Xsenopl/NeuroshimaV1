using UnityEngine;
using UnityEngine.UI;
using System;

public class PanelConfirmationController : MonoBehaviour
{
    public GameObject panelConfirmation;
    public Button confirmButton;
    public Button denyButton;

    private Action onConfirmAction;
    private Action onDenyAction;

    private void Start()
    {
        panelConfirmation.SetActive(false);

        confirmButton.onClick.AddListener(() => Confirm());
        denyButton.onClick.AddListener(() => Deny());
    }

    public void ShowPanel(Action confirmAction, Action denyAction)
    {
        panelConfirmation.SetActive(true);
        onConfirmAction = confirmAction;
        onDenyAction = denyAction;
    }

    private void Confirm()
    {
        onConfirmAction?.Invoke();
        panelConfirmation.SetActive(false);
    }

    private void Deny()
    {
        onDenyAction?.Invoke();
        panelConfirmation.SetActive(false);
    }
}
