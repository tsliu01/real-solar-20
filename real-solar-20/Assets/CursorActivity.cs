using UnityEngine;
using System.Collections;

public class CursorActivity : MonoBehaviour
{
    public bool isSelecting;
    public GameObject selectedIcon;
    private GameObject panelGameobject;
    public  GameObject arrowObj;
    public string a;
    // Use this for initialization
    void Start()
    {
       panelGameobject = GameObject.Find("Panel_L");
        arrowObj = GameObject.Find("Arrow0");
    }
    private string curName;
    // Update is called once per frame
    void Update()
    {
        
        Ray ray = new Ray(transform.position, transform.forward);
        //Ray ray = Camera.main.ScreenPointToRay(transform.forward);//从摄像机发出到点击坐标的射线
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            Debug.DrawLine(ray.origin, hitInfo.point);//划出射线，只有在scene视图中才能看到
            selectedIcon = hitInfo.collider.gameObject;
            Debug.Log("click object name is " + selectedIcon.name);
            isSelecting = true;
            //if (icon.tag == "icon")//当射线碰撞目标为icon类型的物品 ，执行操作
            //{
            //if (isSelecting == true)
            //{
                a = selectedIcon.transform.parent.gameObject .name;


                Transform gameObjTF = selectedIcon.transform.parent;
                Transform arrowObjTF = arrowObj.transform.parent;
                arrowObjTF.parent = gameObjTF;
                arrowObjTF.localPosition = new Vector3(0, 0, 0);
            // arrowObj.transform.localScale *= gameObjTF.lossyScale.x;
            panelGameobject.GetComponent<PanelController>().ShowUI();
            if (  this.transform.parent.name != arrowObj.transform.parent.transform.parent.name)
            {
                arrowObj.SetActive(true);
            }
            else
            {
                arrowObj.SetActive(false);
            }
          
                
                this.gameObject.GetComponent<AdiuoCtor>().ShallowAudio();
          




           
                
                //}
            //}
        }
        else
        {
            isSelecting = false;
            panelGameobject.GetComponent<PanelController>().HideUI();
            arrowObj.SetActive(false);
        }
    }
}