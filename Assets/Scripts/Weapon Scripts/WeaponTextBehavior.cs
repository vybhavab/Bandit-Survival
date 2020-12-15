using UnityEngine;
using UnityEngine.UI;

public class WeaponTextBehavior : MonoBehaviour
{
    public Color negativeColor;
    public Color positiveColor;
    public Text text;
    public Vector3 Offset;

    public void SetText(string txt, bool showDam, bool damIncrease)
    {
        text.color = negativeColor;
        if(damIncrease){
            text.color = positiveColor;
        }
        text.gameObject.SetActive(showDam);
        text.text = txt;
    }

    // Update is called once per frame
    void Update()
    {
        text.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }
}
