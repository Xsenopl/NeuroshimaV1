using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public BoardManager boardManager;

    void Start()
    {
        // StartCoroutine(WaitAndExecute());
    }

    private IEnumerator WaitAndExecute()
    {
        yield return new WaitForSeconds(2);
    }

    void Update()
    {
        
    }

    public void PlaceToken(GameObject tokenPrefab, Vector3 worldPosition)
    {
        GameObject tokenObject = Instantiate(tokenPrefab, worldPosition, Quaternion.identity);
        Token token = tokenObject.GetComponent<Token>();
        if (token != null)
        {
            boardManager.RegisterToken(token);
        }
    }
}
