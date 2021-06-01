using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Toggled : MonoBehaviour
{
    private Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changeValue()
    {
        ColorBlock cb = toggle.colors;
        if (toggle.isOn)
        {
            cb.normalColor =Color.gray;
            cb.selectedColor = Color.gray;
        }
        else
        {
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
        }
        toggle.colors = cb;
    }
}
