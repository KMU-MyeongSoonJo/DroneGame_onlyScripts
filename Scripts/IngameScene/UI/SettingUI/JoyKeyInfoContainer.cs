using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class JoyKeyInfoContainer : MonoBehaviour
{
    public static JoyKeyInfoContainer instance;

    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    List<string[]> rowData = new List<string[]>();
    
    public List<Dictionary<string, object>> csv; // csv에서 가져온데이터
    
    string path = "Data/JoyInfo/"; // + joystick name

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 조이스틱 키 정보 load
        //Read();
    }

    #region csv values get method

    /// <summary>
    /// 해당 조종 축의 이름으로 Axis 정보를 받아오는 함수
    /// </summary>
    /// <param name="name">ex. "Throttle", "Yaw" ... </param>
    /// <returns>ex. "X axis", "4th axis" ...</returns>
    public string GetAxisFromName(string name)
    {
        csv = Read("/CSV_test.csv");

        if (csv[1][name].ToString() == "null") return csv[0][name].ToString();
        else return csv[1][name].ToString();
    }
    
    #endregion

    #region read / write

    public void Read()
    {
        print("====== CSV Load Start ====== ");
        
        //if (File.Exists(Application.dataPath + "/CSV_test.csv"))
        if(!File.Exists(getPath()))
        {
            WriteDefault();
        }

        csv = Read("/CSV_test.csv");

        // save된 키 정보 없음
        if (csv[1]["Throttle"].ToString() == "null")
        {
            print("=== 로드할 키 정보가 존재하지 않습니다. ===");
            // default 키 정보 로드
            InputDeviceChecker.instance.throttleInputAxis = csv[0]["Throttle"].ToString();
            InputDeviceChecker.instance.yawInputAxis = csv[0]["Yaw"].ToString();
            InputDeviceChecker.instance.pitchInputAxis = csv[0]["Pitch"].ToString();
            InputDeviceChecker.instance.rollInputAxis = csv[0]["Roll"].ToString();
        }

        // save된 키 정보 존재
        else
        {
            print("=== 로드할 키 정보가 존재합니다. ===");
            // 저장된 키 정보 로드
            InputDeviceChecker.instance.throttleInputAxis = csv[1]["Throttle"].ToString();
            InputDeviceChecker.instance.yawInputAxis = csv[1]["Yaw"].ToString();
            InputDeviceChecker.instance.pitchInputAxis = csv[1]["Pitch"].ToString();
            InputDeviceChecker.instance.rollInputAxis = csv[1]["Roll"].ToString();
        }

        // 축 방향 로드
        InputDeviceChecker.instance.isThrottleAxisReverse = bool.Parse(csv[2]["Throttle"].ToString());
        InputDeviceChecker.instance.isYawAxisReverse = bool.Parse(csv[2]["Yaw"].ToString());
        InputDeviceChecker.instance.isPitchAxisReverse = bool.Parse(csv[2]["Pitch"].ToString());
        InputDeviceChecker.instance.isRollAxisReverse = bool.Parse(csv[2]["Roll"].ToString());
    }

    public void Write(string throttle, string yaw, string pitch, string roll,
        bool isThrottleRev, bool isYawRev, bool isPitchRev, bool isRollRev)
    {
        // Creating First row of titles manually..
        string[] rowDataTemp = new string[5];
        rowDataTemp[0] = "Type";
        rowDataTemp[1] = "Throttle";
        rowDataTemp[2] = "Yaw";
        rowDataTemp[3] = "Pitch";
        rowDataTemp[4] = "Roll";
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[5];
        rowDataTemp[0] = "default";
        rowDataTemp[1] = "X axis";
        rowDataTemp[2] = "Y axis";
        rowDataTemp[3] = "3rd axis";
        rowDataTemp[4] = "4th axis";
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[5];
        rowDataTemp[0] = "save";
        if (throttle == null)
        {
            if (csv[1]["Throttle"].ToString() == "null")
            {
                rowDataTemp[1] = csv[0]["Throttle"].ToString();
                print("디폴트 값으로 설정되었습니다.");
            }
            else
            {
                rowDataTemp[1] = csv[1]["Throttle"].ToString();
                print("이전에 설정됐던 값으로 설정되었습니다.");
            }
        }
        else rowDataTemp[1] = throttle;
        if (yaw == null)
        {
            if (csv[1]["Yaw"].ToString() == "null")
                rowDataTemp[2] = csv[0]["Yaw"].ToString();
            else
                rowDataTemp[2] = csv[1]["Yaw"].ToString();
        }
        else rowDataTemp[2] = yaw;
        if (pitch == null)
        {
            if (csv[1]["Pitch"].ToString() == "null")
                rowDataTemp[3] = csv[0]["Pitch"].ToString();
            else
                rowDataTemp[3] = csv[1]["Pitch"].ToString();
        }
        else rowDataTemp[3] = pitch;
        if (roll == null)
        {
            if (csv[1]["Roll"].ToString() == "null")
                rowDataTemp[4] = csv[0]["Roll"].ToString();
            else
                rowDataTemp[4] = csv[1]["Roll"].ToString();
        }
        else rowDataTemp[4] = roll;
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[5];
        rowDataTemp[0] = "isReverse";
        rowDataTemp[1] = isThrottleRev.ToString();
        rowDataTemp[2] = isYawRev.ToString();
        rowDataTemp[3] = isPitchRev.ToString();
        rowDataTemp[4] = isRollRev.ToString();
        rowData.Add(rowDataTemp);

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }

        string filePath = getPath();

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();

        Debug.Log(filePath);

        rowData.Clear();

    }
    
    public void WriteDefault()
    {
        // Creating First row of titles manually..
        string[] rowDataTemp = new string[5];
        rowDataTemp[0] = "Type";
        rowDataTemp[1] = "Throttle";
        rowDataTemp[2] = "Yaw";
        rowDataTemp[3] = "Pitch";
        rowDataTemp[4] = "Roll";
        rowData.Add(rowDataTemp);
        
        rowDataTemp = new string[5];
        rowDataTemp[0] = "default";
        rowDataTemp[1] = "X axis";
        rowDataTemp[2] = "Y axis";
        rowDataTemp[3] = "3rd axis";
        rowDataTemp[4] = "4th axis";
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[5];
        rowDataTemp[0] = "save";
        rowDataTemp[1] = "null";
        rowDataTemp[2] = "null";
        rowDataTemp[3] = "null";
        rowDataTemp[4] = "null";
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[5];
        rowDataTemp[0] = "isReverse";
        rowDataTemp[1] = "FALSE";
        rowDataTemp[2] = "FALSE";
        rowDataTemp[3] = "FALSE";
        rowDataTemp[4] = "FALSE";
        rowData.Add(rowDataTemp);

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }

        string filePath = getPath();

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();

        Debug.Log(filePath);

        rowData.Clear();
    }

    #endregion

    #region core

    public static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();
        //TextAsset data = Resources.Load (file) as TextAsset;

        string source;
        StreamReader sr = new StreamReader(Application.dataPath + file);
        source = sr.ReadToEnd();
        sr.Close();

        //var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        var lines = Regex.Split(source, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);

        // 0번 행은 X
        // 1번 행부터 값 추적
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }

    private string getPath()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/CSV_test.csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"TalkData.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"TalkData.csv";
#else
        return Application.dataPath +"/"+"TalkData.csv";
#endif
    }
    #endregion
}
