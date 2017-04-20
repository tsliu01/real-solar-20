using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PanelController : MonoBehaviour
{

    // private GameObject panelObj;
    private RectTransform parentTF;
    private GameObject gameObjCam;
    //private Transform 
    //private float distanceCam;
    //private float disCamPanel;

    //ivate Vector3 offess = new Vector3(1, 1, 1);
    // private string panelName;
    // Use this for initialization
    void Start()
    {
        // panelObj = GameObject.Find("Panel_L");

        //uiPanel = GameObject.Find("Panel_L");
        gameObjCam = GameObject.Find("Main Camera");
        parentTF = this .transform.parent as RectTransform;
      // uiPanel.transform.parent = this.transform;
      
       
    }
  




    /// <summary>/// 显示pamel/// </summary>
    public void ShowUI()
    {
        //if (panelName == null)
        //{

        this .transform.DOScale(new Vector3(3.5f,1.3f,1.3f), 1f);

        // }
    }
    public void HideUI()
    {
        
       this .transform.DOScale(new Vector3(0, 0, 0), 1f);
    }
}