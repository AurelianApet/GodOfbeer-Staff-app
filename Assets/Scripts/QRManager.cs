using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TBEasyWebCam;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class QRManager : MonoBehaviour
{
    public QRCodeDecodeController e_qrController;
    public GameObject resetBtn;
    public GameObject scanLineObj;
    public GameObject err_popup;
    public Text err_str;
    public GameObject popup;
    public Text popup_str;
    public Text title;

    /// <summary>
    /// when you set the var is true,if the result of the decode is web url,it will open with browser.
    /// </summary>
    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if (this.e_qrController != null)
        {
            this.e_qrController.onQRScanFinished += new QRCodeDecodeController.QRScanFinished(this.qrScanFinished);
        }
        if (Global.last_scene == "main")
        {
            title.text = "선불태그 선택";
        }
        else if (Global.last_scene == "table_move")
        {
            title.text = "테이블이동";
        }
        else if (Global.last_scene == "add_tag")
        {
            title.text = "태그등록";
        }
    }

    private void Update()
    {
    }

    public void onGoBack()
    {
        GotoNextScene(Global.last_scene);
    }

    private void qrScanFinished(string dataText)
    {
        if (this.resetBtn != null)
        {
            this.resetBtn.SetActive(true);
        }
        if (this.scanLineObj != null)
        {
            this.scanLineObj.SetActive(false);
        }

        if(Global.last_scene == "main")
        {
            //선불태그 선택
            Debug.Log(dataText);
            WWWForm form = new WWWForm();
            form.AddField("qrcode", dataText);
            WWW www = new WWW(Global.api_url + Global.find_tag, form);
            StartCoroutine(ProcessFindTag(www));
        }
        else if(Global.last_scene == "table_move")
        {
            //테이블 이동
            Debug.Log(dataText);
            WWWForm form = new WWWForm();
            form.AddField("table_name", dataText);
            WWW www = new WWW(Global.api_url + Global.find_table, form);
            StartCoroutine(ProcessSelectTable(www, dataText));
        }
        else if(Global.last_scene == "add_tag")
        {
            //태그등록
            Debug.Log(dataText);
            WWWForm form = new WWWForm();
            form.AddField("qrcode", dataText);
            form.AddField("table_id", Global.cur_tInfo.tid);
            WWW www = new WWW(Global.api_url + Global.reg_tag_api, form);
            StartCoroutine(RegTag(www));
        }
    }

    IEnumerator RegTag(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if (jsonNode["suc"] == 1)
            {
                List<TagInfo> tagList = Global.tableGroupList[Global.cur_tInfo.tgNo].tablelist[Global.cur_tInfo.tNo].taglist;
                string tag_id = jsonNode["tag_id"];
                string name = jsonNode["name"];
                TagInfo tg = new TagInfo();
                tg.id = tag_id;
                tg.name = name;
                tg.is_used = jsonNode["is_used"].AsInt;
                tagList.Add(tg);
                Global.cur_tagList = tagList;
                SceneManager.LoadScene("add_tag");
            }
            else
            {
                err_str.text = "태그등록에 실패하였습니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "태그등록시에 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    IEnumerator ProcessFindTable(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                Global.cur_tInfo.tgid = jsonNode["table_group_id"];
                Global.cur_tInfo.tid = jsonNode["table_id"];
                for (int i = 0; i < Global.tableGroupList.Count; i++)
                {
                    if (Global.tableGroupList[i].id == jsonNode["table_group_id"])
                    {
                        Global.cur_tInfo.tgNo = i;
                        for (int j = 0; j < Global.tableGroupList[i].tablelist.Count; j++)
                        {
                            if (Global.tableGroupList[i].tablelist[j].id == jsonNode["table_id"])
                            {
                                Global.cur_tInfo.tNo = j;
                                Global.cur_tInfo.name = Global.tableGroupList[i].tablelist[j].name;
                                Global.cur_tInfo.is_pay_after = Global.tableGroupList[i].tablelist[j].is_pay_after;
                                break;
                            }
                        }
                        break;
                    }
                }
                Debug.Log(Global.cur_tInfo.name + " clicked.");
                if (Global.tableGroupList[Global.cur_tInfo.tgNo].tablelist[Global.cur_tInfo.tNo].is_pay_after == 1)
                {
                    //후불
                    Debug.Log("후불테이블");
                    SceneManager.LoadScene("menu");
                }
                else
                {
                    //선불
                    Debug.Log("선불테이블");
                    SceneManager.LoadScene("tag");
                }
            }
            else
            {
                err_str.text = "등록되지 않은 테이블입니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "테이블 비교시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    IEnumerator ProcessFindTag(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                Global.cur_tInfo.tgid = jsonNode["tg_id"];
                Global.cur_tInfo.tid = jsonNode["table_id"];
                Global.cur_tInfo.name = jsonNode["table_name"];
                Global.cur_tagInfo.tag_id = jsonNode["tag_id"];
                Global.cur_tagInfo.tag_name = jsonNode["name"];
                Global.cur_tagInfo.remain = jsonNode["remain"].AsInt;
                Global.cur_tagInfo.charge = jsonNode["charge"].AsInt;
                Global.cur_tagInfo.period = jsonNode["period"].AsInt;
                Global.cur_tagInfo.tag_data = jsonNode["tag_data"];
                string reg_date = jsonNode["reg_datetime"];
                Global.cur_tagInfo.reg_datetime = Convert.ToDateTime(reg_date);
                for (int i = 0; i < Global.tableGroupList.Count; i++)
                {
                    if (Global.tableGroupList[i].id == jsonNode["tg_id"])
                    {
                        Global.cur_tInfo.tgNo = i;
                        for (int j = 0; j < Global.tableGroupList[i].tablelist.Count; j++)
                        {
                            if (Global.tableGroupList[i].tablelist[j].id == jsonNode["table_id"])
                            {
                                Global.cur_tInfo.tNo = j;
                                Global.cur_tInfo.name = Global.tableGroupList[i].tablelist[j].name;
                                Global.cur_tInfo.is_pay_after = Global.tableGroupList[i].tablelist[j].is_pay_after;
                                break;
                            }
                        }
                        break;
                    }
                }
                Debug.Log(Global.cur_tInfo.name + " clicked.");
                //선불
                SceneManager.LoadScene("menu_tag");
            }
            else
            {
                err_str.text = jsonNode["msg"];
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "태그 스캔시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    IEnumerator ProcessSelectTable(WWW www, string resultStr)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                Global.selected_tableid = jsonNode["table_id"];
                Global.selected_tablename = resultStr;
                if (Global.selected_tableid == Global.cur_tInfo.tid)
                {
                    GotoNextScene(Global.last_scene);
                }
                if (Global.cur_tInfo.is_pay_after != jsonNode["is_pay_after"])
                {
                    err_str.text = "선불/후불 테이블 간에는 이동이 불가합니다.";
                    err_popup.SetActive(true);
                }
                else if (jsonNode["is_blank"].AsInt == 0)
                {
                    popup_str.text = Global.cur_tInfo.name + "와 " + resultStr + "을 합석하시겠습니까?\n합석 후에는 취소가 불가합니다.";
                    popup.SetActive(true);
                }
                else
                {
                    //즉시 합석 가능
                    MixTable(Global.cur_tInfo.tid, jsonNode["table_id"], Global.cur_tInfo.name, resultStr);
                }
            }
            else
            {
                err_str.text = "등록되지 않은 테이블입니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "테이블 비교시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void onYes()
    {
        MixTable(Global.cur_tInfo.tid, Global.selected_tableid, Global.cur_tInfo.name, Global.selected_tablename);
    }

    public void onNo()
    {
        popup.SetActive(false);
        GotoNextScene(Global.last_scene);
    }

    void MixTable(string origin_tableid, string destination_tableid, string origin_tablename, string destination_tablename)
    {
        WWWForm form = new WWWForm();
        form.AddField("origin_tableid", origin_tableid);
        form.AddField("origin_tablename", origin_tablename);
        form.AddField("destination_tableid", destination_tableid);
        form.AddField("destination_tablename", destination_tablename);
        WWW www = new WWW(Global.api_url + Global.move_table_api, form);
        StartCoroutine(onMixTable(www));
    }

    IEnumerator onMixTable(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if (jsonNode["suc"] == 1)
            {
                SceneManager.LoadScene("main");
            }
            else
            {
                err_str.text = jsonNode["msg"];
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "합석 조작중 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void Reset()
    {
        if (this.e_qrController != null)
        {
            this.e_qrController.Reset();
        }
        if (this.resetBtn != null)
        {
            this.resetBtn.SetActive(false);
        }
        if (this.scanLineObj != null)
        {
            this.scanLineObj.SetActive(true);
        }
    }

    public void Play()
    {
        Reset();
        if (this.e_qrController != null)
        {
            this.e_qrController.StartWork();
        }
    }

    public void Stop()
    {
        if (this.e_qrController != null)
        {
            this.e_qrController.StopWork();
        }

        if (this.resetBtn != null)
        {
            this.resetBtn.SetActive(false);
        }
        if (this.scanLineObj != null)
        {
            this.scanLineObj.SetActive(false);
        }
    }

    public void GotoNextScene(string scenename)
    {
        if (this.e_qrController != null)
        {
            this.e_qrController.StopWork();
        }
        SceneManager.LoadScene(scenename);
    }

    public void onConfirmErrPopup()
    {
        err_popup.SetActive(false);
        GotoNextScene(Global.last_scene);
    }
}
