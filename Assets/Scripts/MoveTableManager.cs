using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIO;


public class MoveTableManager : MonoBehaviour
{
    public GameObject table_group_Item;
    public GameObject table_group_parent;
    public GameObject table_item;
    public GameObject table_parent;
    public GameObject popup;
    public Text popup_str;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    public GameObject err_popup;
    public Text err_str;

    GameObject[] m_tableGroupItem;
    GameObject[] m_tableItem;
    int total_table_group_cnt = 0;
    string first_table_group = "";
    string old_tg_no = "";
    bool loading = false;
    List<TableGroup> mTableGroupList;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Debug.Log("loading table group list in table move scene.");
        mTableGroupList = new List<TableGroup>();
        for(int i = 0; i < Global.tableGroupList.Count; i++)
        {
            Debug.Log(Global.tableGroupList[i].is_pay_after);
            if (Global.tableGroupList[i].is_pay_after == Global.cur_tInfo.is_pay_after)
            {
                mTableGroupList.Add(Global.tableGroupList[i]);
            }
        }
        total_table_group_cnt = mTableGroupList.Count;
        LoadTableGroup();
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

    void LoadTableGroup()
    {
        //UI에 추가
        m_tableGroupItem = new GameObject[total_table_group_cnt];
        if (total_table_group_cnt > 0)
        {
            try
            {
                first_table_group = mTableGroupList[0].id;
            }
            catch (Exception ex)
            {

            }
        }
        for (int i = 0; i < total_table_group_cnt; i++)
        {
            m_tableGroupItem[i] = Instantiate(table_group_Item);
            m_tableGroupItem[i].transform.SetParent(table_group_parent.transform);
            m_tableGroupItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            m_tableGroupItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            m_tableGroupItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            m_tableGroupItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            m_tableGroupItem[i].transform.localScale = Vector3.one;
            try
            {
                m_tableGroupItem[i].transform.Find("name").GetComponent<Text>().text = mTableGroupList[i].name;
                m_tableGroupItem[i].transform.Find("id").GetComponent<Text>().text = mTableGroupList[i].id.ToString();
                string tg_id = mTableGroupList[i].id;
                m_tableGroupItem[i].GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine(LoadTableList(tg_id)); });
            }
            catch (Exception ex)
            {

            }
        }

        if (!loading && total_table_group_cnt > 0 && first_table_group != "")
            StartCoroutine(LoadTableList(first_table_group));
    }

    IEnumerator Destroy_Object(GameObject obj)
    {
        DestroyImmediate(obj);
        yield return null;
    }

    IEnumerator LoadTableList(string id)
    {
        //UI 내역 초기화
        while (table_parent.transform.childCount > 0)
        {
            StartCoroutine(Destroy_Object(table_parent.transform.GetChild(0).gameObject));
            //DestroyImmediate(table_parent.transform.GetChild(0).gameObject);
        }
        while (table_parent.transform.childCount > 0)
        {
            yield return new WaitForSeconds(0.01f);
        }

        //선택된 테이블그룹 노란색으로.
        try
        {
            for (int i = 0; i < table_group_parent.transform.childCount; i++)
            {
                if (table_group_parent.transform.GetChild(i).Find("id").GetComponent<Text>().text == old_tg_no.ToString())
                {
                    table_group_parent.transform.GetChild(i).transform.Find("Name").GetComponent<Text>().color = Color.white;
                }
                if (table_group_parent.transform.GetChild(i).Find("id").GetComponent<Text>().text == id.ToString())
                {
                    table_group_parent.transform.GetChild(i).transform.Find("Name").GetComponent<Text>().color = Color.yellow;
                }
            }
        }
        catch (Exception ex)
        {

        }

        old_tg_no = id;
        List<TableInfo> tbList = new List<TableInfo>();
        for (int i = 0; i < mTableGroupList.Count; i++)
        {
            if (mTableGroupList[i].id == id)
            {
                tbList = mTableGroupList[i].tablelist;break;
            }
        }
        //UI에 로딩
        int tbCnt = tbList.Count;
        m_tableItem = new GameObject[tbCnt];
        loading = true;
        for (int i = 0; i < tbCnt; i++)
        {
            m_tableItem[i] = Instantiate(table_item);
            m_tableItem[i].transform.SetParent(table_parent.transform);
            m_tableItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            m_tableItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            m_tableItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            m_tableItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            m_tableItem[i].transform.localScale = Vector3.one;
            try
            {
                m_tableItem[i].transform.Find("id").GetComponent<Text>().text = tbList[i].id.ToString();
                m_tableItem[i].transform.Find("name").GetComponent<Text>().text = tbList[i].name;
                if (tbList[i].is_blank == 0)
                {
                    m_tableItem[i].transform.Find("name").GetComponent<Text>().color = Color.yellow;
                    if (tbList[i].is_pay_after == 1 && tbList[i].order_price > 0f)
                    {
                        m_tableItem[i].transform.Find("price").GetComponent<Text>().text = Global.GetPriceFormat(tbList[i].order_price);
                        m_tableItem[i].transform.Find("price").GetComponent<Text>().color = Color.yellow;
                    }
                }
                TableInfo tinfo = tbList[i];
                m_tableItem[i].GetComponent<Button>().onClick.AddListener(delegate () { onMoveTable(tinfo); });
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        loading = false;
    }

    void onMoveTable(TableInfo tinfo)
    {
        Global.selected_tableid = tinfo.id;
        Global.selected_tablename = tinfo.name;
        if(Global.selected_tableid == Global.cur_tInfo.tid)
        {
            return;
        }
        if(Global.cur_tInfo.is_pay_after != tinfo.is_pay_after)
        {
            err_str.text = "선불/후불 테이블 간에는 이동이 불가합니다.";
            err_popup.SetActive(true);
        }
        else if(tinfo.order_price > 0 || (tinfo.taglist != null && tinfo.taglist.Count > 0))
        {
            popup_str.text = Global.cur_tInfo.name + "와 " + tinfo.name + "을 합석하시겠습니까?\n합석 후에는 취소가 불가합니다.";
            popup.SetActive(true);
        }
        else
        {
            //즉시 합석 가능
            MixTable(Global.cur_tInfo.tid, tinfo.id, Global.cur_tInfo.name, tinfo.name);
        }
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

    public void onYes()
    {
        MixTable(Global.cur_tInfo.tid, Global.selected_tableid, Global.cur_tInfo.name, Global.selected_tablename);
    }

    public void onNo()
    {
        popup.SetActive(false);
    }

    public void onQrCode()
    {
        Global.last_scene = "table_move";
        SceneManager.LoadScene("QRScanScene");
    }

    public void onCancel()
    {
        SceneManager.LoadScene("menu");
    }

    public void onErrorPop()
    {
        err_popup.SetActive(false);
    }
}
