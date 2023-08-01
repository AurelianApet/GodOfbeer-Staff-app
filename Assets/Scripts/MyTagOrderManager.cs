using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using SocketIO;

public class MyTagOrderManager : MonoBehaviour
{
    public GameObject order_item;
    public GameObject order_parent;
    public Text total_priceTxt;
    public GameObject popup;
    public GameObject popup1;
    public Text tableName;
    public Text remain;
    public Text charge;

    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    public GameObject err_popup;
    public Text err_str;

    GameObject[] m_orderList;
    float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        StartCoroutine(LoadOrderList());
        tableName.text = Global.cur_tagInfo.tag_name;
        remain.text = Global.GetPriceFormat(Global.cur_tagInfo.remain);
        charge.text = Global.GetPriceFormat(Global.cur_tagInfo.charge);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    IEnumerator LoadOrderList()
    {
        while (order_parent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(order_parent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }

        Global.myorderlist.Clear();
        m_orderList = null;

        //주문정보 가져오기 api
        WWWForm form = new WWWForm();
        form.AddField("tag_id", Global.cur_tagInfo.tag_id);
        WWW www = new WWW(Global.api_url + Global.get_myorderlist_from_tag_api, form);
        StartCoroutine(GetMyorderlistFromApi(www));
    }

    IEnumerator GetMyorderlistFromApi(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            JSONNode molist = JSON.Parse(jsonNode["myorderlist"].ToString()/*.Replace("\"", "")*/);
            float total_price = 0f;

            int totalCnt = 0;
            for (int i = 0; i < molist.Count; i++)
            {
                MyOrderInfo minfo = new MyOrderInfo();
                minfo.total_price = molist[i]["total_price"].AsInt;
                total_price += molist[i]["total_price"].AsInt;
                JSONNode menulist = JSON.Parse(molist[i]["menulist"].ToString());
                List<OrderCartInfo> ocinfolist = new List<OrderCartInfo>();
                for (int j = 0; j < menulist.Count; j++)
                {
                    OrderCartInfo ocinfo = new OrderCartInfo();
                    ocinfo.amount = menulist[j]["quantity"].AsInt;
                    ocinfo.price = menulist[j]["price"].AsInt;
                    ocinfo.menu_id = menulist[j]["menu_id"];
                    ocinfo.trno = menulist[j]["trno"];
                    ocinfo.name = menulist[j]["name"];
                    ocinfo.tag_name = menulist[j]["tag_name"];
                    ocinfo.order_time = menulist[j]["reg_datetime"];
                    ocinfo.status = menulist[j]["status"].AsInt;
                    ocinfo.id = menulist[j]["order_id"];
                    ocinfolist.Add(ocinfo);
                    totalCnt++;
                }
                minfo.ordercartinfo = ocinfolist;
                Global.myorderlist.Add(minfo);
            }
            m_orderList = new GameObject[totalCnt];
            totalCnt = 0;
            for (int i = 0; i < Global.myorderlist.Count; i++)
            {
                for (int j = 0; j < Global.myorderlist[i].ordercartinfo.Count; j++)
                {
                    //UI
                    m_orderList[totalCnt] = Instantiate(order_item);
                    m_orderList[totalCnt].transform.SetParent(order_parent.transform);
                    m_orderList[totalCnt].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
                    m_orderList[totalCnt].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
                    float left = 0;
                    float right = 0;
                    m_orderList[totalCnt].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
                    m_orderList[totalCnt].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
                    m_orderList[totalCnt].transform.localScale = Vector3.one;

                    try
                    {
                        m_orderList[totalCnt].transform.Find("id").GetComponent<Text>().text = Global.myorderlist[i].ordercartinfo[j].id.ToString();
                        m_orderList[totalCnt].transform.Find("no").GetComponent<Text>().text = Global.myorderlist[i].ordercartinfo[j].menu_id.ToString();
                        m_orderList[totalCnt].transform.Find("name").GetComponent<Text>().text = Global.myorderlist[i].ordercartinfo[j].name;
                        m_orderList[totalCnt].transform.Find("amount").GetComponent<Text>().text = Global.myorderlist[i].ordercartinfo[j].amount.ToString();
                        m_orderList[totalCnt].transform.Find("price").GetComponent<Text>().text = Global.GetPriceFormat(Global.myorderlist[i].ordercartinfo[j].price);
                        m_orderList[totalCnt].transform.Find("time").GetComponent<Text>().text = Global.GetOrderTimeFormat(Global.myorderlist[i].ordercartinfo[j].order_time);
                        GameObject toggleObj = m_orderList[totalCnt].transform.Find("Toggle").gameObject;
                        m_orderList[totalCnt].transform.Find("name").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(toggleObj); });
                        m_orderList[totalCnt].transform.Find("amount").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(toggleObj); });
                        m_orderList[totalCnt].transform.Find("price").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(toggleObj); });
                        m_orderList[totalCnt].transform.Find("time").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(toggleObj); });
                        if (Global.myorderlist[i].ordercartinfo[j].status == 3)
                        {
                            m_orderList[totalCnt].transform.Find("notice3").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(m_orderList[totalCnt].transform.Find("Toggle").gameObject); });
                            m_orderList[totalCnt].transform.Find("notice1").gameObject.SetActive(false);
                            m_orderList[totalCnt].transform.Find("notice2").gameObject.SetActive(false);
                        }
                        else if(Global.myorderlist[i].ordercartinfo[j].status == 2)
                        {
                            m_orderList[totalCnt].transform.Find("notice2").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(m_orderList[totalCnt].transform.Find("Toggle").gameObject); });
                            m_orderList[totalCnt].transform.Find("notice1").gameObject.SetActive(false);
                            m_orderList[totalCnt].transform.Find("notice3").gameObject.SetActive(false);
                        }
                        else
                        {
                            m_orderList[totalCnt].transform.Find("notice1").GetComponent<Button>().onClick.AddListener(delegate () { onClickOrderItem(m_orderList[totalCnt].transform.Find("Toggle").gameObject); });
                            m_orderList[totalCnt].transform.Find("notice2").gameObject.SetActive(false);
                            m_orderList[totalCnt].transform.Find("notice3").gameObject.SetActive(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                    totalCnt++;
                }
            }
            total_priceTxt.text = Global.GetPriceFormat(total_price);
        }
    }

    void onClickOrderItem(GameObject toggleObj)
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

    public void onExit()
    {
        Global.order_type = 0;
        SceneManager.LoadScene("menu_tag");
    }

    List<string> selected_item_id;
    public void onCancelOrder()
    {
        //주문취소조작
        selected_item_id = new List<string>();
        bool is_cooking = false;
        for (int i = 0; i < order_parent.transform.childCount; i++)
        {
            try
            {
                if (order_parent.transform.GetChild(i).Find("Toggle").GetComponent<Toggle>().isOn)
                {
                    string item = order_parent.transform.GetChild(i).Find("id").GetComponent<Text>().text;
                    if(order_parent.transform.GetChild(i).Find("notice2").gameObject.activeSelf ||
                        order_parent.transform.GetChild(i).Find("notice3").gameObject.activeSelf)
                    {
                        is_cooking = true;
                    }
                    selected_item_id.Add(item);
                }
            }
            catch (Exception ex)
            {

            }
        }
        if(selected_item_id.Count == 0)
        {
            return;
        }
        if (is_cooking)
        {
            popup.SetActive(true);
        }
        else
        {
            popup1.SetActive(true);
        }
    }

    public void onConfirmPopup()
    {
        //취소조작
        WWWForm form = new WWWForm();
        form.AddField("table_name", Global.cur_tInfo.name);
        form.AddField("order_type", 0);//태그주문-0
        form.AddField("tag_id", Global.cur_tagInfo.tag_id);
        string oinfo = "[";
        Debug.Log(selected_item_id.Count + ", " + Global.myorderlist.Count);
        for (int i = 0; i < selected_item_id.Count; i++)
        {
            if (i == 0)
            {
                oinfo += "{";
            }
            else
            {
                oinfo += ",{";
            }
            for (int j = 0; j < Global.myorderlist.Count; j++)
            {
                Debug.Log(Global.myorderlist[j].ordercartinfo.Count);
                bool is_checked = false;
                for (int k = 0; k < Global.myorderlist[j].ordercartinfo.Count; k++)
                {
                    Debug.Log(selected_item_id[i] + ": " + Global.myorderlist[j].ordercartinfo[k].id);
                    if (selected_item_id[i] == Global.myorderlist[j].ordercartinfo[k].id)
                    {
                        oinfo += "\"order_id\":\"" + Global.myorderlist[j].ordercartinfo[k].id + "\","
                            + "\"tag_name\":\"" + Global.myorderlist[j].ordercartinfo[k].tag_name + "\","
                            + "\"menu_name\":\"" + Global.myorderlist[j].ordercartinfo[k].name + "\","
                            + "\"price\":\"" + Global.myorderlist[j].ordercartinfo[k].price + "\","
                            + "\"status\":\"" + Global.myorderlist[j].ordercartinfo[k].status + "\","
                            + "\"amount\":\"" + Global.myorderlist[j].ordercartinfo[k].amount + "\","
                            + "\"menu_id\":\"" + Global.myorderlist[j].ordercartinfo[k].menu_id + "\","
                            + "\"trno\":\"" + Global.myorderlist[j].ordercartinfo[k].trno + "\"}";
                        is_checked = true;
                        break;
                    }
                }
                if (is_checked)
                {
                    break;
                }
            }
        }
        oinfo += "]";
        Debug.Log(oinfo);
        form.AddField("order_info", oinfo);
        form.AddField("staff_no", Global.setInfo.staff_no);
        WWW www = new WWW(Global.api_url + Global.cancel_order_api, form);
        StartCoroutine(CancelOrder(www));
    }

    public void onCancelPopup()
    {
        if (popup.activeSelf)
        {
            popup.SetActive(false);
        }else if (popup1.activeSelf)
        {
            popup1.SetActive(false);
        }
    }

    IEnumerator CancelOrder(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if(jsonNode["suc"] == 1)
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
            err_str.text = "주문취소시에 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void onErrorPop()
    {
        err_popup.SetActive(false);
    }
}
