using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;

public struct Set_Info
{
    public string bus_id;
    public int staff_no;
    public string market_name;
}

public struct TableGroup
{
    public string name;
    public string id;
    public int is_pay_after;
    public List<TableInfo> tablelist;
}

public struct TableInfo
{
    public string name;
    public string id;
    public int is_pay_after;
    public int order_price;
    public int order_amount;
    public int is_blank;
    public List<TagInfo> taglist;
}

public struct TagInfo
{
    public string id;
    public string name;
    public int is_used;//1-»ç¿ëÁß
}

public struct TableSelectedInformation
{
    public string tgId;
    public string tId;
}

public struct CurTableInfo
{
    public int tgNo;
    public int tNo;
    public string tgid;
    public string tid;
    public string name;
    public int is_pay_after;//1-ÈÄºÒ, 0-¼±ºÒ
}

public struct CurTagInfo
{
    public string tag_id;
    public int remain;
    public int charge;
    public string tag_name;
    public DateTime reg_datetime;
    public int period;
    public string tag_data;
}

public struct CategoryInfo
{
    public string id;
    public string name;
    public List<MenuInfo> menulist;
}

public struct MenuInfo
{
    public string name;
    public int price;
    public string menu_id;
    public bool is_soldout;
    public int is_best;
}

public struct MyOrderInfo
{
    public List<OrderCartInfo> ordercartinfo;
    public int total_price;
    public TableSelectedInformation tsInfo;
}

public struct OrderCartInfo
{
    public string id;
    public string tag_name;
    public string menu_id;
    public string name;
    public int price;
    public int amount;
    public string trno;
    public int is_best;
    public int status;
    public string order_time;
}

public class Global
{
    //setting information
    public static Set_Info setInfo = new Set_Info();
    //scene information
    public static string last_scene;
    public static string selected_tableid = "";
    public static string selected_tablename = "";

    //price setting
    public static int ordercart_totalprice = 0;
    public static int ordercart_amount = 0;

    //mycart information
    public static List<TableGroup> tableGroupList = new List<TableGroup>();
    public static List<CategoryInfo> categorylist = new List<CategoryInfo>();
    public static List<OrderCartInfo> mycartlist = new List<OrderCartInfo>();
    public static List<MyOrderInfo> myorderlist = new List<MyOrderInfo>();

    public static CurTableInfo cur_tInfo = new CurTableInfo();
    public static List<TagInfo> cur_tagList = new List<TagInfo>();
    public static CurTagInfo cur_tagInfo = new CurTagInfo();
    public static int order_type = 1;//1-

    //api
    public static string server_address = "";
    public static string api_server_port = "3006";
    public static string api_url = "http://" + server_address + ":" + api_server_port + "/";
    static string api_prefix = "m-api/staff/";

    public static string set_info_api = api_prefix + "set-info";
    public static string get_table_group_api = api_prefix + "get-tablegrouplist";
    public static string get_categorylist_api = api_prefix + "get-categorylist";
    public static string reg_tag_api = api_prefix + "reg-tag";
    public static string cancel_tag_api = api_prefix + "cancel-tag";
    public static string move_table_api = api_prefix + "move-table";
    public static string get_myorderlist_api = api_prefix + "get-myorderlist";
    public static string get_myorderlist_from_tag_api = api_prefix + "get-myorderlist-tag";
    public static string cancel_order_api = api_prefix + "cancel-order";
    public static string order_api = api_prefix + "order";
    public static string find_table = api_prefix + "find-table";
    public static string find_tag = api_prefix + "find-tag";
    public static string select_table = api_prefix + "select-table";

    //socket server
    public static string socket_server = "ws://" + server_address + ":" + api_server_port;

    public static void removeOneCartItem(string menuNo)
    {
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == menuNo)
            {
                OrderCartInfo cinfo = mycartlist[i];
                if (cinfo.amount > 1)
                {
                    cinfo.amount--;
                    mycartlist[i] = cinfo;
                }
                else
                {
                    mycartlist.Remove(mycartlist[i]);
                }
                ordercart_totalprice -= cinfo.price;
                ordercart_amount --;
                Debug.Log("amount --");
                break;
            }
        }
    }

    public static void trashCartItem(string menuNo)
    {
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == menuNo)
            {
                ordercart_totalprice -= mycartlist[i].price * mycartlist[i].amount;
                ordercart_amount -= mycartlist[i].amount;
                Debug.Log("amount --" + mycartlist[i].amount);
                mycartlist.Remove(mycartlist[i]);
                break;
            }
        }
    }

    public static void addOneCartItem(OrderCartInfo cinfo)
    {
        bool is_existing = false;
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == cinfo.menu_id)
            {
                is_existing = true;
                cinfo.amount = mycartlist[i].amount + 1;
                mycartlist[i] = cinfo;
                break;
            }
        }
        if (!is_existing)
        {
            mycartlist.Add(cinfo);
        }
        ordercart_amount++;
        ordercart_totalprice += cinfo.price;
    }

    public static string GetPriceFormat(float price)
    {
        return string.Format("{0:N0}", price);
    }

    public static string GetOrderTimeFormat(string ordertime)
    {
        try
        {
            return string.Format("{0:D2}", Convert.ToDateTime(ordertime).Hour) + ":" + string.Format("{0:D2}", Convert.ToDateTime(ordertime).Minute);
        }
        catch (Exception ex)
        {
            return "";
        }
    }
}


