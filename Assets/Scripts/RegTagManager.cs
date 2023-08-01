using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIO;

public class RegTagManager : MonoBehaviour
{
    public GameObject tagItem;
    public GameObject tagItemParent;
    public GameObject popup;

    public GameObject err_popup;
    public Text err_str;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    GameObject[] m_tagItem;
    List<TagInfo> tagList;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        //tagList = Global.tableGroupList[Global.cur_tInfo.tgNo].tablelist[Global.cur_tInfo.tNo].taglist;
        tagList = Global.cur_tagList;
        StartCoroutine(LoadTaglist());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator LoadTaglist()
    {
        while (tagItemParent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(tagItemParent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        //UI
        m_tagItem = new GameObject[tagList.Count];
        for (int i = 0; i < m_tagItem.Length; i++)
        {
            m_tagItem[i] = Instantiate(tagItem);
            m_tagItem[i].transform.SetParent(tagItemParent.transform);
            m_tagItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            m_tagItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            m_tagItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            m_tagItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            m_tagItem[i].transform.localScale = Vector3.one;
            try
            {
                m_tagItem[i].transform.Find("name").GetComponent<Text>().text = tagList[i].name;
                m_tagItem[i].transform.Find("no").GetComponent<Text>().text = tagList[i].id.ToString();
                GameObject toggleObj = m_tagItem[i].transform.Find("Toggle").gameObject;
                m_tagItem[i].transform.Find("name").GetComponent<Button>().onClick.AddListener(delegate () { onClickTagItem(toggleObj); });
            }
            catch (Exception ex)
            {

            }
        }
    }

    void onClickTagItem(GameObject toggleObj)
    {
        if (toggleObj.GetComponent<Toggle>().isOn)
        {
            toggleObj.GetComponent<Toggle>().isOn = false;
        }
        else
        {
            toggleObj.GetComponent<Toggle>().isOn = true;
        }
    }

    public void onRegTag()
    {
        //카메라 실행하여 QR코드 스캔
        Global.last_scene = "add_tag";
        SceneManager.LoadScene("QRScanScene");
    }

    void cancelTagFunc(List<string> selected_tag_id)
    {
        //취소조작
        WWWForm form = new WWWForm();
        string tag_ids = "[";
        for (int i = 0; i < selected_tag_id.Count; i++)
        {
            if (i == 0)
            {
                tag_ids += "{";
            }
            else
            {
                tag_ids += ",{";
            }
            tag_ids += "\"id\":\"" + selected_tag_id[i] + "\"}";
        }
        tag_ids += "]";
        Debug.Log(tag_ids);
        form.AddField("tag_id", tag_ids);
        WWW www = new WWW(Global.api_url + Global.cancel_tag_api, form);
        StartCoroutine(CancelTags(www, selected_tag_id));
    }

    public void onCancelTag()
    {
        List<string> selected_tag_id = new List<string>();
        for (int i = 0; i < tagItemParent.transform.childCount; i ++)
        {
            try
            {
                if(tagItemParent.transform.GetChild(i).Find("Toggle").GetComponent<Toggle>().isOn)
                {
                    string item = tagItemParent.transform.GetChild(i).Find("no").GetComponent<Text>().text;
                    selected_tag_id.Add(item);
                }
            }
            catch (Exception ex)
            {

            }
        }
        if(Global.cur_tInfo.is_pay_after == 0)
        {
            cancelTagFunc(selected_tag_id);
        }
        else
        {
            bool is_used = false;
            for (int i = 0; i < selected_tag_id.Count; i++)
            {
                for (int j = 0; j < tagList.Count; j++)
                {
                    if (selected_tag_id[i] == tagList[j].id)
                    {
                        if (tagList[j].is_used == 1)
                        {
                            is_used = true; break;
                        }
                    }
                }
                if (is_used)
                {
                    break;
                }
            }
            if (is_used)
            {
                popup.SetActive(true);
            }
            else
            {
                cancelTagFunc(selected_tag_id);
            }
        }
    }

    IEnumerator CancelTags(WWW www, List<string> tags)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if(jsonNode["suc"] == 1)
            {
                //성공
                for (int j = 0; j < tags.Count; j++)
                {
                    for (int i = 0; i < tagList.Count; i++)
                    {
                        if (tagList[i].id == tags[j])
                        {
                            tagList.RemoveAt(i); break;
                        }
                    }
                    for (int i = 0; i < tagItemParent.transform.childCount; i++)
                    {
                        try
                        {
                            if (tagItemParent.transform.GetChild(i).Find("no").GetComponent<Text>().text == tags[j].ToString())
                            {
                                DestroyImmediate(tagItemParent.transform.GetChild(i).gameObject);break;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            else
            {
                err_str.text = "태그회수에 실패하였습니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "태그회수시에 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void onConfirm()
    {
        popup.SetActive(false);
    }

    public void onExit()
    {
        SceneManager.LoadScene("main");
    }

    public void onErrorPop()
    {
        err_popup.SetActive(false);
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
}
