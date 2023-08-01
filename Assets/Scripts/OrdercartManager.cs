using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
using SocketIO;

public class OrdercartManager : MonoBehaviour
{
    public GameObject cart_item;
    public GameObject cart_parent;
    public Text total_price;
    public Text total_amount;
    public Text tableName;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    public GameObject err_popup;
    public Text err_msg;

    GameObject[] m_CartList;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        StartCoroutine(LoadCartList());
        if(Global.order_type == 1)
        {
            tableName.text = Global.cur_tInfo.name;
        }
        else
        {
            tableName.text = Global.cur_tagInfo.tag_name;
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
        if(Global.order_type == 1)
        {
            SceneManager.LoadScene("menu");
        }
        else
        {
            SceneManager.LoadScene("menu_tag");
        }
    }

    public void allDelete()
    {
        Global.mycartlist.Clear();
        Global.ordercart_totalprice = 0;
        Global.ordercart_amount = 0;
        total_amount.text = "0";
        total_price.text = "0";
        StartCoroutine(ClearList());
    }

    IEnumerator ClearList()
    {
        Debug.Log("clear order cart list--");
        while (cart_parent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(cart_parent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator LoadCartList()
    {
        while (cart_parent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(cart_parent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        Debug.Log("load order cart list.");
        m_CartList = new GameObject[Global.mycartlist.Count];
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            m_CartList[i] = Instantiate(cart_item);
            m_CartList[i].transform.SetParent(cart_parent.transform);
            m_CartList[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            m_CartList[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            m_CartList[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            m_CartList[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            m_CartList[i].transform.localScale = Vector3.one;
            try
            {
                m_CartList[i].transform.Find("no").GetComponent<Text>().text = Global.mycartlist[i].menu_id.ToString();
                Text amount = m_CartList[i].transform.Find("amount").GetComponent<Text>();
                amount.text = Global.mycartlist[i].amount.ToString();
                m_CartList[i].transform.Find("name").GetComponent<Text>().text = Global.mycartlist[i].name;
                Text price = m_CartList[i].transform.Find("price").GetComponent<Text>();
                price.text = Global.GetPriceFormat(Global.mycartlist[i].price * Global.mycartlist[i].amount);
                float unit_price = Global.mycartlist[i].price;
                OrderCartInfo cinfo = Global.mycartlist[i];
                m_CartList[i].transform.Find("minus").GetComponent<Button>().onClick.AddListener(delegate () { onMinusBtn(unit_price, price, amount, cinfo); });
                m_CartList[i].transform.Find("plus").GetComponent<Button>().onClick.AddListener(delegate () { onPlusBtn(unit_price, price, amount, cinfo); });
                m_CartList[i].transform.Find("trash").GetComponent<Button>().onClick.AddListener(delegate () { onTrash(cinfo.menu_id); });
            }
            catch (Exception ex)
            {

            }
        }
        total_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
        total_amount.text = Global.ordercart_amount.ToString();
    }

    public void onMinusBtn(float unit_price, Text price, Text amount, OrderCartInfo cinfo)
    {
        Debug.Log("minus count = " + cinfo.menu_id);
        int cnt = 1;
        try
        {
            cnt = int.Parse(amount.text);
        }
        catch (Exception ex)
        {
        }
        if (cnt > 1)
        {
            cnt--;
            Global.removeOneCartItem(cinfo.menu_id);
        }
        amount.text = cnt.ToString();
        price.text = Global.GetPriceFormat(unit_price * cnt);
        total_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
        total_amount.text = Global.ordercart_amount.ToString();
    }

    public void onPlusBtn(float unit_price, Text price, Text amount, OrderCartInfo cinfo)
    {
        Debug.Log("plus count = " + cinfo.menu_id);
        int cnt = 1;
        try
        {
            cnt = int.Parse(amount.text);
        }
        catch (Exception ex)
        {
        }
        cnt++;
        amount.text = cnt.ToString();
        Global.addOneCartItem(cinfo);
        price.text = Global.GetPriceFormat(unit_price * cnt);
        total_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
        total_amount.text = Global.ordercart_amount.ToString();
    }

    public void onTrash(string menuNo)
    {
        Debug.Log("trash = " + menuNo);
        //UI상에서 제거
        for (int i = 0; i < m_CartList.Length; i++)
        {
            try
            {
                if (m_CartList[i].transform.Find("no").GetComponent<Text>().text == menuNo.ToString())
                {
                    DestroyImmediate(m_CartList[i].gameObject);
                    break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //Global에서 제거
        Global.trashCartItem(menuNo);
        total_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
        total_amount.text = Global.ordercart_amount.ToString();
    }

    public void onPack()
    {
        bool takeout = false;
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            if (Global.mycartlist[i].is_best == 999)
            {
                err_msg.text = "포장이 불가한 메뉴입니다.";
                err_popup.SetActive(true);
                takeout = true;
                break;
            }
        }
        if (!takeout)
        {
            if (Global.order_type == 0 && (Global.ordercart_totalprice > Global.cur_tagInfo.remain))
            {
                err_msg.text = "잔액이 부족하여 주문이 불가합니다.";
                err_popup.SetActive(true);
            }
            else
            {
                bool is_expired = false;
                if (Global.order_type == 0)
                {
                    DateTime expiredDate = Global.cur_tagInfo.reg_datetime.AddDays(Global.cur_tagInfo.period);
                    if (DateTime.Now > expiredDate)
                    {
                        is_expired = true;
                    }
                }
                if (is_expired)
                {
                    err_msg.text = "태그의 사용기간이 만료되였습니다.";
                    err_popup.SetActive(true);
                }
                else
                {
                    //포장처리
                    WWWForm form = new WWWForm();
                    form.AddField("type", 1);
                    form.AddField("staff_no", Global.setInfo.staff_no);
                    form.AddField("order_type", Global.order_type);//태그주문-0
                    if (Global.order_type == 0)
                    {
                        form.AddField("tag_id", Global.cur_tagInfo.tag_id);
                    }
                    form.AddField("table_id", Global.cur_tInfo.tid);
                    form.AddField("table_name", Global.cur_tInfo.name);
                    string oinfo = "[";
                    for (int i = 0; i < Global.mycartlist.Count; i++)
                    {
                        if (i == 0)
                        {
                            oinfo += "{";
                        }
                        else
                        {
                            oinfo += ",{";
                        }
                        float new_price = 0f;
                        if(Global.mycartlist[i].is_best >= 100)
                        {
                            new_price = Global.mycartlist[i].price + Global.mycartlist[i].is_best;
                        }
                        else
                        {
                            new_price = Global.mycartlist[i].price * (100 + Global.mycartlist[i].is_best) / 100;
                        }
                        oinfo += "\"menu_id\":\"" + Global.mycartlist[i].menu_id + "\","
                            + "\"menu_name\":\"" + Global.mycartlist[i].name + "\","
                            + "\"price\":" + new_price.ToString() + ","
                            + "\"quantity\":" + Global.mycartlist[i].amount.ToString() + "}";
                    }
                    oinfo += "]";
                    Debug.Log(oinfo);
                    form.AddField("order_info", oinfo);
                    WWW www = new WWW(Global.api_url + Global.order_api, form);
                    StartCoroutine(ProcessOrder(www));
                }
            }
        }
    }

    public void onOrder()
    {
        if(Global.order_type == 0 && (Global.ordercart_totalprice > Global.cur_tagInfo.remain))
        {
            err_msg.text = "잔액이 부족하여 주문이 불가합니다.";
            err_popup.SetActive(true);
        }
        else
        {
            bool is_expired = false;
            if(Global.order_type == 0)
            {
                DateTime expiredDate = Global.cur_tagInfo.reg_datetime.AddDays(Global.cur_tagInfo.period);
                if (DateTime.Now > expiredDate)
                {
                    is_expired = true;
                }
            }
            if (is_expired)
            {
                err_msg.text = "태그의 사용기간이 만료되였습니다.";
                err_popup.SetActive(true);
            }
            else
            {
                //주문 api
                WWWForm form = new WWWForm();
                form.AddField("type", 0);
                form.AddField("staff_no", Global.setInfo.staff_no);
                form.AddField("order_type", Global.order_type);//태그주문-0
                if(Global.order_type == 0)
                {
                    form.AddField("tag_id", Global.cur_tagInfo.tag_id);
                }
                form.AddField("table_id", Global.cur_tInfo.tid);
                form.AddField("table_name", Global.cur_tInfo.name);
                string oinfo = "[";
                for (int i = 0; i < Global.mycartlist.Count; i++)
                {
                    if (i == 0)
                    {
                        oinfo += "{";
                    }
                    else
                    {
                        oinfo += ",{";
                    }
                    oinfo += "\"menu_id\":\"" + Global.mycartlist[i].menu_id + "\","
                        + "\"menu_name\":\"" + Global.mycartlist[i].name + "\","
                        + "\"price\":" + Global.mycartlist[i].price.ToString() + ","
                        + "\"quantity\":" + Global.mycartlist[i].amount.ToString() + "}";
                }
                oinfo += "]";
                Debug.Log(oinfo);
                form.AddField("order_info", oinfo);
                WWW www = new WWW(Global.api_url + Global.order_api, form);
                StartCoroutine(ProcessOrder(www));
            }
        }
    }

    IEnumerator ProcessOrder(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                //성공이면 Global에 추가
                MyOrderInfo myorderinfo = new MyOrderInfo();
                myorderinfo.ordercartinfo = Global.mycartlist;
                myorderinfo.total_price = Global.ordercart_totalprice;
                Global.myorderlist.Add(myorderinfo);
                Global.mycartlist.Clear();
                SceneManager.LoadScene("main");
            }
            else
            {
                err_msg.text = jsonNode["msg"];
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_msg.text = "주문조작시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void onConfirmPopup()
    {
        err_popup.SetActive(false);
    }
}
