using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
  public class HealthBarBehavior : MonoBehaviour
  {
    // Start is called before the first frame update
    public Slider Slider;
    public Text text;
    public Color Low;
    public Color High;
    public Vector3 Offset;

    public void SetHealth(int health, int maxHealth)
    {
        Slider.gameObject.SetActive(health < maxHealth);
        text.gameObject.SetActive(health < maxHealth);
        Slider.value = health;
        Slider.maxValue = maxHealth;
        text.text = "HP: " + health;
        Slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(Low, High, Slider.normalizedValue);
    }

    // Update is called once per frame
    void Update()
    {
      Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
      text.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }
  }
}