﻿// Author : dandanshih
// Desc : 帳號相關的程式都需要放在這裏處理


using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Web.Security;
using Newtonsoft.Json;

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
        dictResult["Password"] = "silver";
        strResult = Account_Check(JsonConvert.SerializeObject(dictResult));
        return "[Finish Test_Account_Check] " + strResult;
    }

    // 做帳號密碼的檢查 
    // (has client)
    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Account_Check(string strJson)
    {
        // 先做個記錄
        int LogID = ReportDBLog("Account_Check", strJson);
        // 開始準備做處理
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
		Dictionary<string, object> dictInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(strJson);
        string strCommand = "";
        List<List<object>> listDBResult = null;
        // 取得資料
        if (dictInfo.ContainsKey("Account") == false)
        {
            return ReportTheResult(dictResult, ErrorID.Check_Account_No_Account, LogID);
        }
        string strAccount = dictInfo["Account"].ToString();
        if (dictInfo.ContainsKey("Password") == false)
        {
            return ReportTheResult(dictResult, ErrorID.Check_Account_No_Password, LogID);
        }
        string strPassword = dictInfo["Password"].ToString();

        // 檢查帳號的存在性
        strCommand = string.Format("select AccountID, Password, PlayerID from a_account where Account = '{0}'", strAccount);
        listDBResult = UseDB.AccountDB.DoQueryCommand(strCommand);
        if (listDBResult.Count == 0)
        {
            return ReportTheResult(dictResult, ErrorID.Check_Account_No_Such_Account, LogID);
        }
        int AccountID = System.Convert.ToInt32(listDBResult[0][0]);
        string Password = listDBResult[0][1].ToString();
        if (Password != strPassword)
        {
            return ReportTheResult(dictResult, ErrorID.Check_Account_No_Such_Password, LogID);
        }

        // 更新 session key 和登入時間
        string strSessionKey = GetSessionKey(strAccount);
        strCommand = string.Format("update a_account set SessionKey='{0}', UpdateDate='{1}' where AccountID={2}", strSessionKey, Utility.GetDBDateTime(), AccountID);
        UseDB.GameDB.DoQueryCommand(strCommand);

        // 回傳結果
        int PlayerID = System.Convert.ToInt32(listDBResult[0][2]);
        dictResult["Result"] = ErrorID.Success;
        dictResult["Account"] = strAccount;
        dictResult["Password"] = strPassword;
        dictResult["AccountID"] = AccountID;
        dictResult["PlayerID"] = PlayerID;
        dictResult["SessionKey"] = strSessionKey;
        // 做 Client 的行控制
        if (PlayerID == 0)
        {
            // 進入新手流程
            ClientAction.AddClientAction(dictResult, ClientActionID.ToNewPlayer);
        }
        else
        {
            // 進入主流程
            ClientAction.AddClientAction(dictResult, ClientActionID.ToLogin);
		}

        return ReportTheResult(dictResult, ErrorID.Success, LogID);
    }

    #endregion

    #region 創角動作

    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Test_Account_CreatePlayer()
    {
        Dictionary<string, object> dictResult = new Dictionary<string, object>();
        dictResult["SessionKey"] = "SessionKey:dandan";
        dictResult["PlayerName"] = "dandan";
        Account_CreatePlayer(JsonConvert.SerializeObject(dictResult));
        return "Test_Account_CreatePlayer";
    }

    [WebMethod]
    [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public string Account_CreatePlayer(string strJson)
    {
        log4net.LogManager.GetLogger("Account_CreatePlayer").DebugFormat("Args:{0}", strJson);
        // 先寫一筆資料
        int LogID = ReportDBLog ("Account_CreatePlayer", strJson);
        string strCommand = "";
        List<List<object>> listDBResult = null;

        // 分析資料
        Dictionary<string, object> dictResult = new Dictionary<string, object>();

        // 取得資料
		Dictionary<string, object> dictInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(strJson);
        if (dictInfo == null)
        {
            return ReportTheResult(dictResult, ErrorID.Json_Format_Error, LogID);
        }
        if (dictInfo.ContainsKey ("SessionKey") == false)
        {
            return ReportTheResult(dictResult, ErrorID.No_SessionKey, LogID);
        }
        string SessionKey = dictInfo["SessionKey"].ToString();

        // 轉 SessionKey To AccountID
        Dictionary<string, object> dictAccount = GetAccountInfoBySessionKey(SessionKey);
        if (dictAccount == null)
        {
            return ReportTheResult(dictResult, ErrorID.SessionError, LogID);
        }
        int AccountID = System.Convert.ToInt32(dictAccount["AccountID"]);

        // 檢查角色
        int PlayerID = System.Convert.ToInt32(dictAccount["PlayerID"]);
        if (PlayerID != 0)
        {
            return ReportTheResult(dictResult, ErrorID.Account_CreatePlayer_Exist_Player, LogID);
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
        dictResult["PlayerName"] = PlayerName;

        strCommand = string.Format("insert into a_member (PlayerName) values ('{0}')"+UseDB.GETID, PlayerName);
        listDBResult = UseDB.GameDB.DoQueryCommand(strCommand);
        PlayerID = System.Convert.ToInt32(listDBResult[0][0]);
        dictResult["PlayerID"] = PlayerID;

        // 塞回去 Account 中
        strCommand = string.Format("update a_account set PlayerID={0} where AccountID={1}", PlayerID, AccountID);
        UseDB.GameDB.DoCommand(strCommand);

        // 產生一隻新的 NPC 給玩家使用
        string strCreateNPC = ConfigTable.instance().Get("CreatePlayer_Start_Partner");
        List<string> listCreateNPC = Utility.Split(strCreateNPC, ",");
        string strNPC = Utility.GetRange(listCreateNPC);
        // 送給玩家

        // 進入主流程
        return ReportTheResult(dictResult, ErrorID.Success, LogID);
    }

    // [Todo] 從 DB 取得亂數姓名
    string GetRandomNameFromDB()
    {
        return "測試用名字";
    }

    #endregion
}
