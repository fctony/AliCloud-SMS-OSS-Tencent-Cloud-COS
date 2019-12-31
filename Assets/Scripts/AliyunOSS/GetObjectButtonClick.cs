using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetObjectButtonClick : MonoBehaviour
{
    private void Awake()
    {
        GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            string filePath = transform.GetChild(0).GetComponent<Text>().text;
            GetObject.Instance.GetObjectProcess(Test.Instance.PutWithProcessCallBack, filePath, @"C:\Users\山商艾欧特\Desktop\" + filePath);
        });
    }
}
