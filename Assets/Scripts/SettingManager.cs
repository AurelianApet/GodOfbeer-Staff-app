using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LitJson;
using SimpleJSON;
using System;
using SocketIO;

public class SettingManager : MonoBehaviour
{
    public InputField bus_id;
    public InputField pip;
    public InputField staffno;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    public GameObject error_popup;
    public Text error_string;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if (Global.server_address != "")
        {
            //이미 보관된 정보가 있다면
            bus_id.text = Global.setInfo.bus_id;
            pip.text = Global.server_address;
            staffno.text = Global.setInfo.staff_no.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float time = 0f;

    void FixedUpdate()
    {
        if (!Input.anyKey)
        {
            time += Time.deltaTime;
        }
        else
        {
            if (time != 0f)
            {
                GameObject.Find("touch").GetComponent<AudioSource>().Play();
                time = 0f;
            }
        }
    }


    public void onExit()
    {
        Application.Quit();
    }

    public void onBack()
    {
        SceneManager.LoadScene("main");
    }

    public void Save()
    {
        if (bus_id.text == "")
        {
            error_string.text = @"사업자등록번호를 입력하세요.";
            error_popup.SetActive(true);
            return;
        }
        if (pip.text == "")
        {
            error_string.text = @"IP를 입력하세요.";
            error_popup.SetActive(true);
            return;
        }
        if (staffno.text == "")
        {
            error_string.text = @"Staff no을 입력하세요.";
            error_popup.SetActive(true);
            return;
        }
        //set info api
        Global.server_address = pip.text;
        PlayerPrefs.SetString("ip", Global.server_address);
        Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
        WWWForm form = new WWWForm();
        form.AddField("bus_id", bus_id.text);
        WWW www = new WWW(Global.api_url + Global.set_info_api, form);
        StartCoroutine(ProcessSetInfo(www));
    }

    IEnumerator ProcessSetInfo(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                try
                {
                    PlayerPrefs.SetString("bus_id", bus_id.text);
                    PlayerPrefs.SetString("staff_no", staffno.text);
                    PlayerPrefs.SetString("mark_name", jsonNode["market_name"]);
                    Global.setInfo.bus_id = bus_id.text;
                    Global.setInfo.staff_no = int.Parse(staffno.text);
                    Global.setInfo.market_name = jsonNode["market_name"];
                    SceneManager.LoadScene("main");
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            else
            {
                error_string.text = jsonNode["msg"]/*.Replace("\"", "")*/;
                error_popup.SetActive(true);
            }
        }
        else
        {
            error_string.text = @"저장에 실패하였습니다.";
            error_popup.SetActive(true);
        }
    }

    public void onClosePopup()
    {
        error_popup.SetActive(false);
    }
}
