using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InteriorSaver : MonoBehaviour
{
    public static InteriorSaver instance;

    private List<Dictionary<string, object>> csv; // CSV파일에서 가져온 데이터
    private int index = 0; // csv 행 숫자

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Load();
    }

    public void Save()
    {
        using (var writer = new CsvFileWriter(Application.persistentDataPath + "/myInterior.csv"))
        {
            List<string> columns = new List<string>();// making Index Row
            columns.Add("name");
            columns.Add("posX");
            columns.Add("posY");
            columns.Add("posZ");
            columns.Add("angY");
            writer.WriteRow(columns);
            columns.Clear(); // 다음 행을 위해 초기화

            writer.WriteRow(columns); // 2줄 띄어 놓기

            foreach (Interior interior in InteriorArrayManager.instance.placedInteriors)
            {
                columns.Add(interior.name); // 가구 이름
                columns.Add(interior.transform.position.x.ToString()); // 가구 x 위치
                columns.Add(interior.transform.position.y.ToString()); // 가구 y 위치
                columns.Add(interior.transform.position.z.ToString()); // 가구 z 위치
                columns.Add(((int)interior.transform.eulerAngles.y).ToString()); // 가구의 회전 상태
                writer.WriteRow(columns); // 현재 행 입력
                columns.Clear(); // 다음 행을 위해 초기화
            }

            columns.Add("END"); // 가구 불러오기의 끝을 알리는 문자
            writer.WriteRow(columns); // 현재 행 입력
            columns.Clear(); // 다음 행을 위해 초기화
        }
    }

    public void Load()
    {
        csv = CSVReader.Read(new WWW("file:///" + Application.persistentDataPath + "/myInterior.csv")); // CSV파일 불러오기
        index = 0; // CSV의 현재 열을 초기화

        CreateInterior(); // CSV 파일에 있는 정보대로 가구 생성하기
    }

    private void CreateInterior()
    {
        if (csv[index]["name"].ToString() == "END") // 모든 가구를 불러왔다면 함수 정지
            return;

        Vector3 pos = new Vector3(float.Parse(csv[index]["posX"].ToString()), float.Parse(csv[index]["posY"].ToString()), float.Parse(csv[index]["posZ"].ToString())); // 가구 생성할 위치 받아오기
        Quaternion ang = Quaternion.Euler(0, int.Parse(csv[index]["angY"].ToString()), 0); // 가구의 각도 받아오기

        GameObject obj = new GameObject();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("My Room"));

        switch (csv[index]["name"].ToString()) // 가구 이름을 받아와서 생성할 가구 Prefab을 결정
        {
            // 1. Bed
            // blue
            case "bed_1_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_1_blue"), pos, ang);
                break;
            case "bed_2_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_2_blue"), pos, ang);
                break;
            // purple
            case "bed_1_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_1_purple"), pos, ang);
                break;
            case "bed_2_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_2_purple"), pos, ang);
                break;
            // stripes
            case "bed_1_stripes(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_1_stripes"), pos, ang);
                break;
            case "bed_2_stripes(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Bed/bed_2_stripes"), pos, ang);
                break;

            // 2. Chair
            case "chair_office(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_office"), pos, ang);
                break;
            case "chair_1_white(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_1_white"), pos, ang);
                break;
            case "chair_1_wood(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_1_wood"), pos, ang);
                break;
            case "chair_2(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_2"), pos, ang);
                break;
            case "chair_2_no_padding(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_2_no_padding"), pos, ang);
                break;
            // red
            case "chair_3_red(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_3_red"), pos, ang);
                break;
            case "chair_4_red(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_4_red"), pos, ang);
                break;
            case "chair_playroom_1_red(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_1_red"), pos, ang);
                break;
            case "chair_playroom_2_red(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_2_red"), pos, ang);
                break;
            // orange
            case "chair_3_orange(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_3_orange"), pos, ang);
                break;
            case "chair_4_orange(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_4_orange"), pos, ang);
                break;
            case "chair_playroom_1_orange(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_1_orange"), pos, ang);
                break;
            case "chair_playroom_2_orange(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_2_orange"), pos, ang);
                break;
            // blue
            case "chair_3_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_3_blue"), pos, ang);
                break;
            case "chair_4_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_4_blue"), pos, ang);
                break;
            case "chair_playroom_1_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_1_blue"), pos, ang);
                break;
            case "chair_playroom_2_blue(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_2_blue"), pos, ang);
                break;
            // purple
            case "chair_3_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_3_purple"), pos, ang);
                break;
            case "chair_4_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_4_purple"), pos, ang);
                break;
            case "chair_playroom_1_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_1_purple"), pos, ang);
                break;
            case "chair_playroom_2_purple(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Chair/chair_playroom_2_purple"), pos, ang);
                break;

            // 3. Table
            case "table_1_white(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_1_white"), pos, ang);
                break;
            case "table_1_white_with_tablecloth(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_1_white_with_tablecloth"), pos, ang);
                break;
            case "table_1_wood(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_1_wood"), pos, ang);
                break;
            case "table_1_wood_with_tablecloth(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_1_wood_with_tablecloth"), pos, ang);
                break;
            case "table_2_white(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_2_white"), pos, ang);
                break;
            case "table_2_white_with_tablecloth(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_2_white_with_tablecloth"), pos, ang);
                break;
            case "table_3(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_3"), pos, ang);
                break;
            case "table_televisionstand(Clone)":
                obj = Instantiate(Resources.Load<GameObject>("Prefabs/Furniture/Table/table_televisionstand"), pos, ang);
                break;
        }

        Scene loadingScene = SceneManager.GetSceneByName("LoadingScene");
        if (loadingScene.isLoaded) SceneManager.SetActiveScene(loadingScene);

        InteriorArrayManager.instance.PlaceAtArray(obj.GetComponent<Interior>()); // 배열에 인테리어 넣기

        index++;
        CreateInterior(); // 재귀 함수
    }
}