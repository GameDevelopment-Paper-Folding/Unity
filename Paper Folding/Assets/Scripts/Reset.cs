using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Reset : MonoBehaviour
{
    private GameObject FoldPercentSlider;
    // Start is called before the first frame update
    void Start()
    {
        FoldPercentSlider = GameObject.Find("FoldPercentSlider");
        FoldPercentSlider.GetComponent<Slider>().value = 50;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void reset()
    {
        FoldPercentSlider.GetComponent<Slider>().value = 50;
    }
}
