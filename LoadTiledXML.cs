using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Xml;
using System.IO;


public class LoadTiledXML : MonoBehaviour
{
    struct mapInfo
    {       
        public int width;
        public int height;
        public int tilewidth;
        public int tileheight;
        //public tileset tt;
        //public tiledLayer tl;
    }
    struct tileset {
        public int index;
        public struct imageInfo {
            public string pngsource;
            public int pngwidth;
            public int pngheight;
            public struct CollisionInfo {
                public string CollisionType;// box or polygon
                public int tileId;
                public Rect boxRect;
                public Vector2 poloffset;
                public List<Vector2> polygonPoints;
            }
            public List<CollisionInfo> CollisionInfoArr;
        }
        public imageInfo image;
        public int firstgid;
        public int tilewidth;
        public int tileheight;
        public int tilecount;
    }
    struct tiledLayer{
        public string layerName;
        public string data_encoding;
        public List<int> mapData;
    }

    [MenuItem("myselfExtend/LoadTiled-----Tiled文件")]
    static void LoadTiled()
    {
        string[] filters = { "tmx", "tmx", "xml", "xml" };//文件过滤
        string filepath = EditorUtility.OpenFilePanelWithFilters("select xml", "D:/", filters);

        if(!File.Exists(filepath)) return;//如果文件不存在就返回

        mapInfo map = new mapInfo();//
        List<tileset> tilesetArr = new List<tileset>();
        List<tiledLayer> tiledLayerArr = new List<tiledLayer>();

        XmlDocument xml = new XmlDocument();
        XmlReaderSettings set = new XmlReaderSettings();
        set.IgnoreComments = true;//这个设置是忽略xml注释文档的影响。有时候注释会影响到xml的读取
        xml.Load(XmlReader.Create((filepath), set));

        //取得map节点的各属性值
        XmlElement xmlroot = (XmlElement)xml.SelectSingleNode("map");//得到map节点
        XmlNodeList xmlNodeList = xmlroot.ChildNodes;//得到map节点下的所有子节点
        map.width = int.Parse(xmlroot.GetAttribute("width"));//得到map width属性值
        map.height = int.Parse(xmlroot.GetAttribute("height"));
        map.tilewidth = int.Parse(xmlroot.GetAttribute("tilewidth"));
        map.tileheight = int.Parse(xmlroot.GetAttribute("tileheight"));
        //Debug.Log(map.width + " " + map.height + " " + map.tilewidth + " " + map.tileheight);
        int index = 0; //tempTileset.index = index++;
        //遍历map节点下的所有子节点 把信息存储到自定义的结构中
        foreach(XmlElement xmlNode in xmlNodeList) {
            if(xmlNode.Name == "tileset") {
                tileset tempTileset = new tileset();
                tempTileset.firstgid = int.Parse(xmlNode.GetAttribute("firstgid"));
                tempTileset.tilewidth = int.Parse(xmlNode.GetAttribute("tilewidth"));
                tempTileset.tileheight = int.Parse(xmlNode.GetAttribute("tileheight"));
                tempTileset.tilecount = int.Parse(xmlNode.GetAttribute("tilecount"));
                tempTileset.image.pngsource = xmlNode.GetAttribute("name");
                XmlElement image = (XmlElement)xmlNode.ChildNodes[0];//得到tileset下第一个子节点image               
                //tempTileset.image.pngsource = image.GetAttribute("source").
                //    Remove(image.GetAttribute("source").Length - 4);//减掉4为字符串最后4位 .png
                //                                                    //Split('.')[0];弃用
                tempTileset.image.pngwidth = int.Parse(image.GetAttribute("width"));
                tempTileset.image.pngheight = int.Parse(image.GetAttribute("height"));
                tempTileset.image.CollisionInfoArr = new List<tileset.imageInfo.CollisionInfo>();
                foreach(XmlElement tile in xmlNode) {
                    if(tile.Name == "tile") {
                        tileset.imageInfo.CollisionInfo tempcol = new tileset.imageInfo.CollisionInfo();
                        tempcol.boxRect = new Rect();
                        tempcol.tileId = int.Parse(tile.GetAttribute("id"));
                        XmlElement objectGroup = (XmlElement)tile.ChildNodes[0];//objectgroup
                        foreach(XmlElement obj in objectGroup) {
                            float x = float.Parse(obj.GetAttribute("x"));
                            float y = float.Parse(obj.GetAttribute("y"));
                            //Debug.Log(obj.ChildNodes.Count);
                            if(obj.ChildNodes.Count == 0) {
                                tempcol.CollisionType = "rect";
                                tempcol.boxRect = new Rect(x, y,
                                    float.Parse(obj.GetAttribute("width")),
                                    float.Parse(obj.GetAttribute("height")));
                            } else {
                                if(obj.ChildNodes[0].Name == "ellipse") {
                                    tempcol.CollisionType = "ellipse";
                                    tempcol.boxRect = new Rect(x, y,
                                    float.Parse(obj.GetAttribute("width")),
                                    float.Parse(obj.GetAttribute("height")));
                                } else{
                                    tempcol.CollisionType = obj.ChildNodes[0].Name;
                                    XmlElement polyline = (XmlElement)obj.ChildNodes[0];
                                    tempcol.polygonPoints = new List<Vector2>();
                                    tempcol.poloffset = new Vector2(x, y);//object id x y line开始的两个点
                                    string[] tempp = polyline.GetAttribute("points").Split(' ');
                                    //if(tempp.Length == 2) { tempcol.CollisionType = "edge"; }
                                    foreach(string pos in tempp) {
                                        string[] tp = pos.Split(',');
                                        tempcol.polygonPoints.Add(new Vector2(float.Parse(tp[0]), float.Parse(tp[1])));
                                    }
                                }
                            }
                        }
                        tempTileset.image.CollisionInfoArr.Add(tempcol);
                    }
                }
                tempTileset.index = index++;
                tilesetArr.Add(tempTileset);//添加到LIST
            }//if (xmlNode.Name == "tileset")
            if(xmlNode.Name == "layer") {
                tiledLayer tempLayer = new tiledLayer();
                tempLayer.layerName = xmlNode.GetAttribute("name");
                XmlElement data = (XmlElement)xmlNode.ChildNodes[0];//得到layer下第一个子节点data
                tempLayer.data_encoding = data.GetAttribute("encoding");//得到data的加密方式

                if(tempLayer.data_encoding == "csv")//如果是CSV没有加密的情况
                {
                    tempLayer.mapData = new List<int>();
                    string[] strArr = data.InnerText.Split(',');
                    foreach(string s in strArr) {
                        tempLayer.mapData.Add(int.Parse(s));
                    }
                    tiledLayerArr.Add(tempLayer);
                } else if(tempLayer.data_encoding == "base64")//如果是CSV没有加密的情况
                {
                    if(data.GetAttribute("compression") != "") { throw new System.Exception("tmx文件请使用base64非压缩格式"); }//加密返回 
                    tempLayer.mapData = new List<int>();
                    byte[] bytes = System.Convert.FromBase64String(data.InnerText);
                    for(int bytesindex = 0; bytesindex < bytes.Length;) {
                        tempLayer.mapData.Add(bytes[bytesindex] + bytes[bytesindex + 1] * 256 +
                            bytes[bytesindex + 2] * 256 * 256 + bytes[bytesindex + 3] * 256 * 256 * 256);//1Byte=8bit 2^8=256
                        bytesindex += 4;
                    }
                    tiledLayerArr.Add(tempLayer);
                } else { throw new System.Exception("tmx文件请使用csv格式或者base64非压缩格式"); }//加密返回                
            }//if (xmlNode.Name == "layer")            
        }//foreach (XmlElement xmlNode in xmlNodeList)

        //根据数据绘图-------------
        List<Texture2D> Texture2dArr = new List<Texture2D>();


        foreach(tileset temp in tilesetArr)//建立所有图片Texture2D
        {
            Texture2D tempTexture = (Texture2D)Resources.Load(temp.image.pngsource) as Texture2D;
            Texture2dArr.Add(tempTexture);
            //if (tempTexture == null) Debug.Log("tempTexture is null");
        }

        GameObject myTiled = new GameObject();
        myTiled.name = "myTiled";
        int tempX = 0;
        int tempY = 0;
        int layerMub = 0;
        //int linex=tilesetArr[0].image.pngwidth / tilesetArr[0].tilewidth;// 每一行的图块个数png图的宽度/图块的宽度
        //得到map数据里面的对应的图块并建立sprite
        foreach(tiledLayer templayer in tiledLayerArr) { //遍历所有layer
            GameObject tempTiled = new GameObject();
            foreach(int data in templayer.mapData) { //遍历所有layer的mapData  
                if(data == 0)//空补位
                {
                    if(tempX > map.width - 1) { tempX = 0; tempY++; }
                    tempX++;
                }
                foreach(tileset tile in tilesetArr) {
                    if(data >= tile.firstgid && data < (tile.firstgid + tile.tilecount)) {
                        int linex = tilesetArr[tile.index].image.pngwidth / tilesetArr[tile.index].tilewidth;// 每一行的图块个数png图的宽度/图块的宽度
                        //int liney = tilesetArr[tile.index].image.pngheight / tilesetArr[tile.index].tileheight;// 每一列的图块个数png图的高度/图块的宽度
                        int tempData = data - tile.firstgid + 1;
                        int picx = tempData / linex; //得到图块所在的png图行位置               
                        int picy = tempData % linex; //得到图块所在的png图列位置
                        Sprite tempSprite = null;
                        if(picy == 0) { picx -= 1; picy = linex; }//每行最后一个的

                        tempSprite = Sprite.Create(Texture2dArr[tile.index],
                            new Rect((picy - 1) * tile.tilewidth,//x左上0向右+
                            tile.image.pngheight - (picx * tile.tileheight) - tile.tileheight,//y左下0向上+
                            tile.tilewidth,//width 16
                            tile.tileheight),//height 16
                            new Vector2(0.5f, 0.5f));
                        GameObject pic = new GameObject();
                        pic.AddComponent<SpriteRenderer>();
                        pic.GetComponent<SpriteRenderer>().sortingOrder = layerMub;
                        pic.GetComponent<SpriteRenderer>().sprite = tempSprite;
                        pic.transform.localScale += new Vector3(1.0f, 1.0f, 0);//缩放xy10.0
                        if(tempX > map.width - 1) { tempX = 0; tempY++; }
                        if(tempX++ <= map.width - 1) {
                            pic.name = tempY.ToString() + "_" + tempX.ToString();
                            pic.transform.position = new Vector2(tempX * map.tilewidth * 0.02f, -map.tileheight * tempY * 0.02f);
                            //pic.transform.position = new Vector2(tempX * map.tilewidth, -map.tileheight * tempY);
                        }
                        // tiled坐标 左上----> +X   unity 坐标           +Y
                        //             |                                 ^
                        //             |                                 |
                        //             |                                中点------> +X
                        //             |                                 |
                        //             v                                 v 
                        //             +Y                                -Y
                        int coll = data - tile.firstgid;//当前图片是否存在Collision  有就添加
                        foreach(tileset.imageInfo.CollisionInfo colInfo in tile.image.CollisionInfoArr) {
                            if(colInfo.tileId == coll) {
                                float posx = -(float)tilesetArr[tile.index].tilewidth / 200 + //把X点设置在图片块tilewidth的1半位置
                                      colInfo.boxRect.x / 100 + colInfo.boxRect.width / 200; //加上x点跟图片块tilewidth的1半
                                float posy = (float)tilesetArr[tile.index].tileheight / 200 - //把y点设置在图片块tileheight的1半位置
                                    colInfo.boxRect.y / 100 - colInfo.boxRect.height / 200;//取反方向y点跟图片块tileheight的1半                        

                                float polyx = -(float)tilesetArr[tile.index].tilewidth / 200 + colInfo.poloffset.x / 100;
                                float polyy = (float)tilesetArr[tile.index].tileheight / 200 - colInfo.poloffset.y / 100;
                                if(colInfo.CollisionType == "rect") {
                                            pic.AddComponent<BoxCollider2D>();
                                    pic.GetComponent<BoxCollider2D>().offset = new Vector2(posx, posy);
                                    pic.GetComponent<BoxCollider2D>().size = new Vector2(
                                        colInfo.boxRect.width / 100,
                                        colInfo.boxRect.height / 100);
                                }
                                if(colInfo.CollisionType == "polygon") {
                                    pic.AddComponent<PolygonCollider2D>();
                                    pic.GetComponent<PolygonCollider2D>().offset = new Vector2(polyx, polyy);
                                    pic.GetComponent<PolygonCollider2D>().pathCount = colInfo.polygonPoints.Count;
                                    Vector2[] tempv2 = new Vector2[colInfo.polygonPoints.Count];
                                    int v2index = 0;
                                    foreach(Vector2 tv2 in colInfo.polygonPoints) { tempv2[v2index++] = new Vector2(tv2.x / 100.0f, tv2.y / -100.0f); }
                                    pic.GetComponent<PolygonCollider2D>().points = tempv2;
                                }
                                if(colInfo.CollisionType == "polyline") {
                                    pic.AddComponent<EdgeCollider2D>();
                                    pic.GetComponent<EdgeCollider2D>().offset = new Vector2(polyx, polyy);
                                    //pic.GetComponent<EdgeCollider2D>().pathCount = colInfo.polygonPoints.Count;
                                    Vector2[] tempv2 = new Vector2[colInfo.polygonPoints.Count];
                                    int v2index = 0;

                                    foreach(Vector2 tv2 in colInfo.polygonPoints) { tempv2[v2index++] = new Vector2(tv2.x / 100.0f, tv2.y / -100.0f); }

                                    pic.GetComponent<EdgeCollider2D>().points = tempv2;
                                }
                                if(colInfo.CollisionType == "ellipse") {
                                    pic.AddComponent<CapsuleCollider2D>();
                                    pic.GetComponent<CapsuleCollider2D>().offset = new Vector2(posx, posy);
                                    pic.GetComponent<CapsuleCollider2D>().size = new Vector2(
                                       colInfo.boxRect.width / 100,
                                       colInfo.boxRect.height / 100);
                                }

                            }
                        }                        
                        pic.transform.parent = tempTiled.transform;//设置tempTiled为父对象
                    }
                }// foreach (tileset t in tilesetArr)                                         
            }//foreach (int data in templayer.mapData)
            tempX = 0;
            tempY = 0;
            layerMub++;
            tempTiled.name = templayer.layerName;
            tempTiled.transform.parent = myTiled.transform;//设置myTiled为父对象            
        }// foreach (tiledLayer templayer in tiledLayerArr) 
         //float CameraSize = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize;
         //假如屏幕宽Screen.width,屏幕高Screen.heigth,Camera的Size为camera.size，
         //那么矩形的高度总为2* camera.size，
         //设矩形的宽度为rectWidth,
         //则Screen.width / Screen.heigth = rectWidth /（2 * camera.size）,
         //矩形的宽度rectWidth为（2 * camera.size）*Screen.width / Screen.heigth.
         //也就是说不管屏幕分辨率怎么变化，矩形的高度始终为摄像机size的两倍  
         //myTiled.transform.position = new Vector3(Screen.width / 200, Screen.height / 200);
         //myTiled.transform.position = new Vector3(-2 * CameraSize + map.tilewidth / 100.0f, CameraSize - map.tileheight / 100.0f, 0.0f);
        myTiled.name = filepath;
    }
    //[MenuItem("myselfExtend/LoadTiled-----Tiled文件")]
    public void delpng(string fileName) {
        fileName.Remove(fileName.Length - 4);//减掉4为字符串最后4位 .png
    }
}
