using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UICtrl : MonoSingleton<UICtrl>
{

    public Text text;
    /// <summary>
    /// 被检测到的物体名称
    /// </summary>
    public string starName;
    /// <summary>
    /// 被检测到的物体名称
    /// </summary>
    public  string selectedIconName;
    //GameObject text = GameObject.Find("Text");
    public string configPath = "/config.txt" ;
    //private void Awake()
    //{
    //    //configPath = "/config.txt";
    //    Resources.Load(configPath);
    //   // StartCoroutine(Start());
    //}
    // Update is called once per frame
  
    void Update()
    {
       
        //text.text= GameObject.Find("Arrow").GetComponent<Select>().planetName;
       // starName = GameObject.Find("Arrow").GetComponent<Select>().planetName;
       selectedIconName = GameObject.Find("Main Camera").GetComponent<CursorActivity>().a ;
      //  print(starName + 111111);
        if (selectedIconName!=null&&selectedIconName !=starName  )
        {
            //GetParticularInfo(starName);
            // GetParticularInfo(selectedIconName );
            //StartCoroutine(StartGet());
            GetInfo(selectedIconName);

        }
       //if (starName !=null &&starName !=selectedIconName )
       // {
       //     GetInfo(starName);
       // }



    }
    //IEnumerator StartGet()
    //{
    //    //bool a = GetComponent<FocusOn>().closeRange;
    //    Configuration.LoadConfig(configPath);
    //    while (!Configuration.IsDone)
    //        yield return null;
    //  //  text.text = Configuration.GetContent("Star", starName);
    //    GetParticularInfo(selectedIconName);
    //}



    //public void GetParticularInfo(string starName)
    //{
    //    text.text = Configuration.GetContent("Star", starName);


    //}



    private void GetInfo(string starName)
    {
        switch (starName)
        {
            case "Sun": text.text = "太阳：太阳系中心的恒星，由热等离子体与磁场交织着的近理想球体。直径约为1.392×10⁶千米（地球直径的109倍）；体积约为地球的130万倍；质量约为2×10³⁰千克（地球的33万倍）。从化学组成来看，现在太阳质量的大约四分之三是氢，剩下的几乎都是氦，而氧、碳、氖、铁和其他的重元素质量少于2%，以核聚变的方式向太空释放光和热。"; break;
            case "Mercury": text.text = "水星：太阳系中最内侧也是最小的一颗行星。有着八大行星中最大的轨道偏心率。它每87.968个地球日绕行太阳一周，而每公转2.01周同时也自转3圈。"; break;
            case "Venus": text.text = "金星：类地行星，因为其质量与地球类似，有时也被人们叫做地球的“姐妹星”。也是太阳系中唯一一颗没有磁场的行星。在八大行星中金星的轨道最接近圆形，偏心率最小，仅为0.006811。"; break;
            case "Earth": text.text = "地球：自西向东非匀速地自转，同时围绕太阳公转。由于日、月、行星的引力作用以及大气、海洋和地球内部物质的各种作用，地球自转轴在空间和地球本体内的方向都会产生变化。地球自转与公转运动的结合产生了昼夜交替和四季变化。其自转产生的惯性离心力使得地球形状由两极向赤道逐渐膨胀，成为略扁的旋转椭球体，极半径比赤道半径约短21千米。"; break;
            case "Mars": text.text = "火星：太阳系的第四颗行星，属于类地行星，直径约为地球的0.53倍，自转轴倾角、自转周期均与地球相近，公转一周约为地球公转时间的两倍。其天文符号是♂，在西方称为“战神玛尔斯”。火星基本上是沙漠行星，沙丘、砾石遍布且没有稳定的液态水体，地表的赤铁矿（氧化铁）使其呈现出橘红色。二氧化碳为主的大气既稀薄又寒冷，沙尘悬浮其中，常有尘暴发生。由冰与干冰组成的两极极冠会随着季节消长。与地球相比，地质活动较不活跃，地表地貌大部份于远古较活跃的时期形成，陨石坑、火山与峡谷密布，包括太阳系最高的山：奥林帕斯山和最大的峡谷：水手号峡谷。"; break;
            case "Jupiter": text.text = "木星：太阳系中体积和质量最大的行星，质量超过其它七大行星总和的2.5倍，是地球的318倍，体积是地球的1,321倍。木星也是太阳系中自转最快的行星，所以两极稍扁，赤道略鼓。木星是天空中第四亮的星星，仅次于太阳、月球和金星（木星有时会比火星稍暗，但有时却要比金星还亮）。木星主要由氢和氦组成，中心温度估计高达30,500℃。"; break;
            case "Saturn": text.text = "土星：古称镇星或填星。直径约119300公里（为地球的9.5倍），是太阳系第二大行星。它与邻居木星十分相像，表面也是液态氢和氦的海洋，上方同样覆盖着厚厚的云层。土星上狂风肆虐，沿东西方向的风速可超过每小时1600公里。土星上空的云层就是这些狂风造成的，云层中含有大量的结晶氨。"; break;
            case "Uranus": text.text = "天王星：太阳系的第七颗行星，距太阳约29亿公里。其公转周期为84.01个地球年。赤道半径约25,900公里。质量大约是地球的14倍，体积约为地球的65倍，在九大行星中仅次于木星和土星。"; break;
            case "Neptune": text.text = "海王星：距离太阳最远的行星，体积是太阳系第四大，但质量排名是第三，大约是地球的17倍。海王星大气层85%是氢气，13%是氦气，2%是甲烷，此外还有少量氨气。其中甲烷是使行星呈现蓝色的一部分原因，但因为海王星的蓝色比有同样份量的天王星更为鲜艳，因此应该还有其他的成分对海王星明显的颜色有所贡献。"; break;
            case "Halley": text.text = "哈雷彗星：彗核的成分以水冰为主，占70%，其他成分是 一氧化碳（10～15%）、二氧化碳、碳氧化合物、氢氰酸等。整个彗核的密度是水冰的10～40%，就像一个松散的大雪堆。彗核深层是原始物质和较易挥发的冰块，周围是含有硅酸盐和碳氢化合物的水冰包层，最外层则是呈蜂窝状的难熔的碳质层。彗尾的主要成分为水、氨、氮、甲烷、一氧化碳、二氧化碳，和不完备分子的自由基。"; break;
            case "Moon": text.text = "月球：俗称月亮，古时又称太阴、玄兔，是地球唯一的天然卫星，也是太阳系中第五大的卫星和密度第二高的卫星（仅次于木卫一）。直径是地球的四分之一，质量是地球的1/81，是相对于所环绕行星的质量最大的卫星。一般认为月亮形成于约45亿年前，地球出现后的不长时间。"; break;



        }

    }
}