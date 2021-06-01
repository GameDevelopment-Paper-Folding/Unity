using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FoldPercent : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider;
    private Text text;
    void Start()
    {
        text = GameObject.Find("FoldPercent").GetComponent<Text>();
        int FoldPercent = (int)slider.value;
        PlayerPrefs.SetInt("FoldPercent", FoldPercent);
        text.text = "Fold Percent:" + FoldPercent + "%";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangePercent()
    {
        int FoldPercent = (int)slider.value;
        PlayerPrefs.SetInt("FoldPercent", FoldPercent);
        text.text = "Fold Percent:" + FoldPercent +"%";
    }
}
