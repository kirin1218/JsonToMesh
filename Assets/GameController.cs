#define WRITE_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
//using System.Collections.Generic;


[Serializable] //なくても一応変換はされるが、一部挙動が変になるので必ずつけておくべき
class Data
{
    public int publicField = 123;
    public float publicFloat = float.MaxValue;
    public bool publicBool = true;
    public string publicString = "ABC";

    //NonSerializedをつけるとシリアライズされない
    [NonSerialized]
    public int ignoreFiled = 999;

    //配列にも対応
    public int[] publicIntArray = new[] { 1, 2, 3, 4, 5 };

    //プロパティはシリアライズされない
    public string publicProperty { get { return "Property"; } }

    //privateフィールドはシリアライズされない
    private int privateField = 123;

    //privateも含めたい場合は SerializeFieldAttribute をつける
    [SerializeField]
    private int privateField2 = 345;
}

[Serializable]
class Data2 : ISerializationCallbackReceiver
{
    //Dictionaryはシリアライズしてくれない
    public Dictionary<int, string> playerList;

    [SerializeField]
    private List<int> _keyList;
    [SerializeField]
    private List<string> _valueList;

    public Data2()
    {
        playerList = new Dictionary<int, string>();
        playerList.Add(1, "A");
        playerList.Add(2, "B");
        playerList.Add(3, "C");
    }

    /// <summary>
    /// Json化時に実行してくれる
    /// </summary>
    public void OnBeforeSerialize()
    {
        //シリアライズする際にkeyとvalueをリストに展開
        _keyList = playerList.Keys.ToList();
        _valueList = playerList.Values.ToList();
    }

    /// <summary>
    /// Jsonからデシリアライズされたあとに実行される
    /// </summary>
    public void OnAfterDeserialize()
    {
        //デシリアライズ時に元のDictionaryに詰め直す
        playerList = _keyList.Select((id, index) =>
        {
            var value = _valueList[index];
            return new { id, value };
        }).ToDictionary(x => x.id, x => x.value);

        _keyList.Clear();
        _valueList.Clear();

    }
}

[Serializable]
class Data3
{
    public int intValue;
}

/// <summary>
/// meta情報を含んだJson
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
class JsonData<T>
{
    public Meta meta;
    public T data;

    public JsonData(string metaName, T data)
    {
        this.meta = new Meta(metaName);
        this.data = data;
    }
}

/// <summary>
/// Meta情報の部分
/// </summary>
[Serializable]
class Meta
{
    public string typeName;
    public Meta(string typeName) { this.typeName = typeName; }
}

/// <summary>
/// 実際に変換したいデータ
/// </summary>
[Serializable]
class PlayerParams
{
    public string name;
    public int hp;
    public int attackPower;

    public PlayerParams(string name, int hp, int attack)
    {
        this.name = name;
        this.hp = hp;
        this.attackPower = attack;
    }
}

[SerializeField]
public class SaveData
{

    private static string filePath = Application.dataPath + "/savedata.json";//セーブデータのファイルパス
    public static string FilePath
    {//ファイルパスのプロパティ
        get { return filePath; }
    }

    public long ld;
    public string name;
    public List<int> list;
    public int[] array;
}

/// <summary>
/// 頂点情報の管理
/// </summary>
[Serializable]
public class Vertex3
{
    [SerializeField]
    private float[] k;

    public float x { get { return k[0]; } }
    public float y { get { return k[1]; } }
    public float z { get { return k[2]; } }

    //  ポリゴンの構成情報
    public Vertex3( float x, float y, float z) {
        k = new float[3];
        k[0] = x;
        k[1] = y;
        k[2] = z;
    }
}

[Serializable]
public class Polygon3
{
    [SerializeField]
    private int[] i;

    public int i1 { get { return i[0]; } }
    public int i2 { get { return i[1]; } }
    public int i3 { get { return i[2]; } }

    //  ポリゴンの構成情報
    public Polygon3(int i1, int i2, int i3)
    {
        i = new int[3];
        i[0] = i1;
        i[1] = i2;
        i[2] = i3;
    }
}

/// <summary>
/// 建物情報の保持
/// </summary>
[Serializable]
public class BuildInfo
{
    //  建物の頂点情報
    [SerializeField]
    private List<Vertex3> v;
    //  ポリゴンを構成する頂点のリスト
    [SerializeField]
    private List<Polygon3> p;

    public List<Vertex3> getVertex3 { get { return v; } }
    public void AddVertex3( Vertex3 item )
    {
        v.Add(item);
    }
    public int CountVertex3()
    {
        return v.Count();
    }
    public List<Polygon3> getPolygon3 { get { return p; } }
    public void AddPolygon3(Polygon3 item)
    {
        p.Add(item);
    }
    public int CountPolygon3()
    {
        return p.Count();
    }
    public BuildInfo() {
        this.v = new List<Vertex3>();
        this.p = new List<Polygon3>();

    }
}

/// <summary>
/// 地図情報のJsonファイル情報を保持
/// </summary>
[SerializeField]
public class MapInfoJson
{

    private static string filePath = Application.dataPath + "/mapinfo.json";//セーブデータのファイルパス
    public static string FilePath
    {//ファイルパスのプロパティ
        get { return filePath; }
    }

    [SerializeField]
    private List<BuildInfo> map;

    public void Add(BuildInfo item)
    {
        map.Add(item);
    }

    public List<BuildInfo> List { get { return map; } }

    public int Count(){ return map.Count(); }

    public MapInfoJson()
    {
        this.map = new List<BuildInfo>();
    }
}

public class GameController : MonoBehaviour {
    [SerializeField]
    //private Mesh _mesh;
    private List<Mesh> listMesh;
    private List<int> listColor;
    private MapInfoJson mapinfo;

    public GameObject stage;
    public Material[] mateBuild;

    // Use this for initialization
    void Start () {
#if  WRITE_MODE
        SaveToJson(MapInfoJson.FilePath, mapinfo);
#endif
        //  Stargeを敷き詰める
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                GameObject stargeObject = (GameObject)Instantiate(
                    stage,
                    new Vector3(i * 100, 0, j * 100),
                    Quaternion.identity
                    );
            }
        }

#if false
        var data = new Data();
        var json = JsonUtility.ToJson(data, true); //整形する

        var data2 = new Data2();
        var json2 = JsonUtility.ToJson(data2, true);

        //json
        var json3 = "{\"intValue\":123}";

        //ジェネリック使わない版
        var data31 = JsonUtility.FromJson(json, typeof(Data3)) as Data3;
        //ジェネリックで型パラメータ渡す版
        var data32 = JsonUtility.FromJson<Data3>(json);


        var p1 = new PlayerParams("FighterA", 100, 300);
        var jsonData = new JsonData<PlayerParams>("player_param", p1);

        // 結果: {"meta":{"typeName":"player_param"},"data":{"name":"FighterA","hp":100,"attackPower":300}}
        Debug.Log(JsonUtility.ToJson(jsonData));
#endif
        }





    private void Awake()
    {
        listMesh = new List<Mesh>();
        listColor = new List<int>();
#if true
        mapinfo = LoadFromJson(MapInfoJson.FilePath);
#else
        mapinfo = new MapInfoJson();

        BuildInfo building = new BuildInfo();

        building.AddVertex3(new Vertex3(0, 1f, 0));
        building.AddVertex3(new Vertex3(1f, -1f, 0));
        building.AddVertex3(new Vertex3(-1f, -1f, 0));
        building.AddPolygon3(new Polygon3(0, 1, 2));

        mapinfo.Add(building);
#endif

        for (int i = 0; i < mapinfo.Count(); i++)
        {
            Mesh _mesh = new Mesh();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();


            BuildInfo build = mapinfo.List[i];

            for (int j = 0; j < build.CountVertex3(); j++) {
                vertices.Add(new Vector3(build.getVertex3[j].x, build.getVertex3[j].y, build.getVertex3[j].z));
            }

            for (int k = 0; k < build.CountPolygon3(); k++)
            {
                Polygon3 poly = build.getPolygon3[k];
                triangles.Add(poly.i1);
                triangles.Add(poly.i2);
                triangles.Add(poly.i3);
            }
            // (4) Meshに頂点情報を代入
            _mesh.vertices = vertices.ToArray();
            _mesh.triangles = triangles.ToArray();
            //        _mesh.normals = normals.ToArray();

            _mesh.RecalculateBounds();

            listMesh.Add(_mesh);

            int iColor = UnityEngine.Random.Range(0, 3);
            listColor.Add(iColor);

        }
    }

    // Update is called once per frame
    void Update () {
        for (int i = 0; i < listMesh.Count(); i++)
        {
            Graphics.DrawMesh(listMesh[i], Vector3.zero, Quaternion.identity, mateBuild[listColor[i]], 0);
        }
    }

    /// <summary>
    /// ファイル書き込み
    /// </summary>
    /// <param name="filePath">ファイルのある場所</param>
    public static void SaveToJson(string filePath, MapInfoJson data)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(JsonUtility.ToJson(data));
            }
        }
    }

    /// <summary>
    /// ファイル読み込みする
    /// </summary>
    /// <param name="filePath">ファイルのある場所</param>
    /// <returns></returns>
    public static MapInfoJson LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {//ファイルがない場合FALSE.
            Debug.Log("FileEmpty!");
            return new MapInfoJson();//ファイルが無いときは適当に処す.
        }

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                MapInfoJson sd = JsonUtility.FromJson<MapInfoJson>(sr.ReadToEnd());
                if (sd == null) return new MapInfoJson();
                return sd;
            }
        }
    }
}
#if false
{
    "map":[
            {
                "v":[
                        {"k":[0.0,1.0,0.0]},
                        {"k":[1.0,-1.0,0.0]},
                        {"k":[-1.0,-1.0,0.0]},
                        {"k":[0.0,1.0,1.0]},
                        {"k":[1.0,-1.0,1.0]},
                        {"k":[-1.0,-1.0,1.0]}

                    ],
                "p":[
                        {"i":[0,1,2]},
                        {"i":[3,4,5]}
                    ]
            }
    ]
}
#endif