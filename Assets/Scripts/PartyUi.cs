using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyUi : MonoBehaviour
{
    [SerializeField] private List<Text> partyNumbersToSelect = new List<Text>();
    [SerializeField] private RectTransform canvasRect;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < partyNumbersToSelect.Count; i++)
        {
            partyNumbersToSelect[i].text = (i + 1).ToString();
        }
        while (true)
        {
            for (int i = 0; i < PartyInputManager.Instance.Party.Count; i++)
            {
                Vector2 viewportPosition = GameManager.Instance.mainCamera.WorldToViewportPoint(PartyInputManager.Instance.Party[i].transform.position);
                Vector2 worldObject_ScreenPosition = new Vector2(((viewportPosition.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)), ((viewportPosition.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)));

                if (partyNumbersToSelect.Count > i && partyNumbersToSelect[i] != null)
                {
                    partyNumbersToSelect[i].rectTransform.anchoredPosition = worldObject_ScreenPosition;
                }
            }

            for (int i = partyNumbersToSelect.Count - 1; i >= 0; i--)
            {
                if (partyNumbersToSelect.Count > PartyInputManager.Instance.Party.Count ||
                    PartyInputManager.Instance.Party[i].Health <= 0)
                {
                    Destroy(partyNumbersToSelect[i].gameObject);
                    partyNumbersToSelect.RemoveAt(i);
                }
            }
            
            yield return null;
        }
    }
}
