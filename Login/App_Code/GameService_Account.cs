﻿// Author : dandanshih
// Desc : 帳號相關的程式都需要放在這裏處理
// [problem] 可以使用 exception 來簡化修改

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Web.Security;

/// <summary>
/// WebService 的摘要描述
/// </summary>
//[WebService(Namespace = "http://tempuri.org/")]
//[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下一行。
public partial class GameService : System.Web.Services.WebService
{
    #region 檢查帳號是否存在, 並轉換成 AccountID
    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Test_Account_Check()
    {
        string strResult = "";
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
        dictResult["Account"] = "dandan";
        dictResult["Password"] = "silveran";
        Account_Check(Json.Serialize(dictResult));
        return "Finish Test_Account_Check:" + strResult;
    }

    // 做帳號密碼的檢查
    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Account_Check(string strJson)
    {
        // 先做個記錄
        int LogID = ReportDBLog("Account_Check", strJson);
        // 開始準備做處理
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
        Dictionary<string, object> dictInfo = Json.Deserialize(strJson) as Dictionary<string, object>;
        string strCommand = "";
        List<List<object>> listDBResult = null;
        // 取得資料
        if (dictInfo.ContainsKey("Account") == false)
        {
            dictResult["Result"] = ErrorID.Check_Account_No_Account;
            ReportDBLog("Account_Check No Account", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        string strAccount = dictInfo["Account"].ToString();
        if (dictInfo.ContainsKey("Password") == false)
        {
            dictResult["Result"] = ErrorID.Check_Account_No_Password;
            ReportDBLog("Account_Check Not Password", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        string strPassword = dictInfo["Password"].ToString();

        // 檢查帳號的存在性
        strCommand = string.Format("select AccountID, Password, PlayerID from A_Account where Account = '{0}'", strAccount);
        listDBResult = UseDB.AccountDB.DoQueryCommand(strCommand);
        if (listDBResult.Count == 0)
        {
            dictResult["Result"] = ErrorID.Check_Account_No_Such_Account;
            ReportDBLog("Account_Check Not Such Account", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        int AccountID = System.Convert.ToInt32(listDBResult[0][0]);
        string Password = listDBResult[0][1].ToString();
        if (Password != strPassword)
        {
            dictResult["Result"] = ErrorID.Check_Account_No_Such_Password;
            ReportDBLog("Account_Check Not Such Password", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }

        // 更新 session key 和登入時間
        string strSessionKey = GetSessionKey(strAccount);
        strCommand = string.Format("update A_Account set SessionKey='{0}', UpdateDate='{1}' where AccountID={2}", strSessionKey, Utility.GetDBDateTime(), AccountID);
        UseDB.GameDB.DoQueryCommand(strCommand);

        // 回傳結果
        int PlayerID = System.Convert.ToInt32(listDBResult[0][2]);
        dictResult["Result"] = ErrorID.Success;
        dictResult["AccountID"] = AccountID;
        dictResult["PlayerID"] = PlayerID;
        dictResult["SessionKey"] = strSessionKey;
        // 做 Client 的行控制
        if (PlayerID == 0)
        {
            // 進入新手流程
            AddClientAction(dictResult, ClientAction.ToNewPlayer());
        }
        else
        {
            // 進入主流程
            AddClientAction(dictResult, ClientAction.ToLogin ());
        }
        ReportDBLog("Account_Check Success", Json.Serialize(dictResult), LogID);
        return Json.Serialize(dictResult);
    }

    // 取得 session Key
    string GetSessionKey(string Account)
    {
        // 以後再來想怎麼編碼
        return "SessionKey:" + Account;
    }

    // 利用 Session Key 取得帳號資料
    Dictionary<string, object> GetAccountInfoBySessionKey(string SessionKey)
    {
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
        string strCommand = "";
        List<List<object>> listDBResult = null;
        strCommand = string.Format("select AccountID, Account, PlayerID, UpdateDate from A_Account where SessionKey='{0}'", SessionKey);
        listDBResult = UseDB.GameDB.DoQueryCommand(strCommand);
        if (listDBResult.Count == 0)
        {
            ReportDBLog(Json.Serialize(ErrorID.SessionError), IDMap.GetEnumAttribute(ErrorID.SessionError));
            return null;
        }
        dictResult["AccountID"] = listDBResult[0][0];
        dictResult["Account"] = listDBResult[0][1];
        dictResult["PlayerID"] = listDBResult[0][2];
        dictResult["UpdateDate"] = listDBResult[0][3];
        return dictResult;
    }

    #endregion


    #region 創角動作

    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Test_Account_CreatePlayer()
    {
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
        dictResult["SessionKey"] = "SessionKey:dandan";
        Account_CreatePlayer(Json.Serialize(dictResult));
        return "Test_Account_CreatePlayer";
    }

    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Account_CreatePlayer(string strJson)
    {
        // 先寫一筆資料
        int LogID = ReportDBLog ("Account_CreatePlayer", strJson);
        string strCommand = "";
        List<List<object>> listDBResult = null;

        // 分析資料
        Dictionary<string, object> dictResult = new Dictionary<string, object>();

        // 取得資料
        Dictionary<string, object> dictInfo = Json.Deserialize(strJson) as Dictionary<string, object>;
        if (dictInfo == null)
        {
            dictResult["Result"] = ErrorID.Json_Format_Error;
            ReportDBLog("Account_CreatePlayer Json Format Error", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        if (dictInfo.ContainsKey ("SessionKey") == false)
        {
            dictResult["Result"] = ErrorID.No_SessionKey;
            ReportDBLog("No SessionKey", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        string SessionKey = dictInfo["SessionKey"].ToString();

        // 轉 SessionKey To AccountID
        Dictionary<string, object> dictAccount = GetAccountInfoBySessionKey(SessionKey);
        if (dictAccount == null)
        {
            dictResult["Result"] = ErrorID.SessionError;
            ReportDBLog("Session Error", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }
        int AccountID = System.Convert.ToInt32(dictAccount["AccountID"]);

        // 檢查角色
        int PlayerID = System.Convert.ToInt32(dictAccount["PlayerID"]);
        if (PlayerID != 0)
        {
            dictResult["Result"] = ErrorID.Account_CreatePlayer_Exist_Player;
            ReportDBLog("Account_CreatePlayer Exist Player", Json.Serialize(dictResult), LogID);
            return Json.Serialize(dictResult);
        }

        // 取得創角資料和建立角色
        string PlayerName = "";
        if (dictInfo.ContainsKey("PlayerName") == false)
        {
            PlayerName = GetRandomNameFromDB();
        }
        else
        {
            PlayerName = dictInfo["PlayerName"].ToString();
        }
        strCommand = string.Format("insert into a_member (PlayerName) values ('{0}');SELECT LAST_INSERT_ID();", PlayerName);
        listDBResult = UseDB.GameDB.DoQueryCommand(strCommand);

        // 進入主流程
        return Json.Serialize(dictResult);
    }

    // [Todo] 從 DB 取得亂數姓名
    string GetRandomNameFromDB()
    {
        return "測試用名字";
    }

    #endregion
}
