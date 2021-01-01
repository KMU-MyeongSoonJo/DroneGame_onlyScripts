using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomWall : MonoBehaviour
{
    public static RoomWall instance;

    public Material Front, Right, Back, Left, Floor, Celling;

    private void Awake()
    {
        instance = this;

        UpdateWallTexture();
    }

    public void SetWallTheme(string _theme) // 마이룸 벽 테마 설정
    {
        if (_theme.Length >= 1)
        {
            PlayerPrefs.SetString("myRoomWall", _theme);
        }
        else // 매개 변수를 비워둘경우 PlayerPrefs의 벽 Key를 제거 (기본값)
        {
            PlayerPrefs.DeleteKey("myRoomWall");
        }

        UpdateWallTexture(); // 테마 설정 후 벽 Texture 자동 업데이트
    }

    public void SetFloorTheme(string _theme) // 마이룸 바닥 테마 설정
    {
        if (_theme.Length >= 1)
        {
            PlayerPrefs.SetString("myRoomFloor", _theme);
        }
        else // 매개 변수를 비워둘경우 PlayerPrefs의 벽 Key를 제거 (기본값)
        {
            PlayerPrefs.DeleteKey("myRoomFloor");
        }

        UpdateWallTexture();
    }

    public void SetCellingTheme(string _theme) // 마이룸 천장 테마 설정
    {
        if (_theme.Length >= 1)
        {
            PlayerPrefs.SetString("myRoomCelling", _theme);
        }
        else // 매개 변수를 비워둘경우 PlayerPrefs의 벽 Key를 제거 (기본값)
        {
            PlayerPrefs.DeleteKey("myRoomCelling");
        }

        UpdateWallTexture();
    }

    public void UpdateWallTexture()
    {
        if (PlayerPrefs.HasKey("myRoomWall")) // PlayerPrefs에 벽 Key가 존재하면 마이룸 벽 Texture 변경
        {
            Front.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomWall") + "/front") as Texture;
            Right.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomWall") + "/right") as Texture;
            Back.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomWall") + "/back") as Texture;
            Left.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomWall") + "/left") as Texture;
        }
        else // 존재하지 않을 때 마이룸 벽 기본값으로 변경
        {
            Front.mainTexture = null;
            Right.mainTexture = null;
            Back.mainTexture = null;
            Left.mainTexture = null;
        }

        if (PlayerPrefs.HasKey("myRoomFloor")) // PlayerPrefs에 바닥 Key가 존재하면 마이룸 바닥 Texture 변경
        {
            Floor.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomFloor") + "/floor") as Texture;
        }
        else // 존재하지 않을 때 마이룸 바닥 기본값으로 변경
        {
            Floor.mainTexture = null;
        }

        if (PlayerPrefs.HasKey("myRoomCelling")) // PlayerPrefs에 바닥 Key가 존재하면 마이룸 바닥 Texture 변경
        {
            Celling.mainTexture = Resources.Load("Sprites/RoomWall/" + PlayerPrefs.GetString("myRoomCelling") + "/celling") as Texture;
        }
        else // 존재하지 않을 때 마이룸 바닥 기본값으로 변경
        {
            Celling.mainTexture = null;
        }
    }
}
