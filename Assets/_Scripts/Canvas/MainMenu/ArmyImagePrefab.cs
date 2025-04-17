using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyImagePrefab : MonoBehaviour
{
    public string armyName;
    private Image armyImage;
    private ArmySelectionManager selectionManager;

    private void Start()
    {
        armyImage = GetComponent<Image>();
        selectionManager = FindObjectOfType<ArmySelectionManager>();

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (selectionManager != null)
        {
            selectionManager.SelectArmy(armyImage.sprite, armyName);
        }
    }
}
