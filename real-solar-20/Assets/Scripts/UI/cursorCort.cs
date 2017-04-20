using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorCort : MonoBehaviour
{
    /// <summary> /// 相机 /// </summary>
    private GameObject targetCam;
    /// <summary> /// 星体 /// </summary>
    private GameObject targetObj;
    ///// <summary> /// 星体坐标 /// </summary>
    //private Transform planetTrans;
    /// <summary> /// 光标指示坐标/// </summary>
    private GameObject OBJ;
   // private float distance;
    // Use this for initialization
    void Start()
    {
        targetCam = GameObject.Find("Main Camera");
        //planetTrans = GameObject.Find("Sun").GetComponent<Transform>();
       OBJ = GameObject.Find("Arrow0");
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(targetCam.transform);
      
       // targetObj  =GameObject .Find ( GameObject.Find("Panel_L").GetComponent<UICtrl>().starName);
        targetObj = GameObject.Find(GameObject.Find("Panel_L").GetComponent<UICtrl>().selectedIconName );
       // distance = Vector3.Distance(targetObj.transform.position, targetCam.transform.position);
      
        if (targetObj != null)
        {
            Fun(targetObj);
          
            OBJ.SetActive(true);
          
        }
     


    }
   public  void Fun(GameObject targetObj)
    {

       


        if (targetObj != null)
        {

            //if (distance <=100)
            //{

            //}
            if (targetObj.transform.lossyScale.x < 100)
            {
                this.transform.localScale *= 100;
            }
            else
            {
                this.transform.localScale *= targetObj.transform.lossyScale.x;
            }
        }
    }
}
