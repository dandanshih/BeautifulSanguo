﻿// Author : dandanshih
// Desc : 使用定義

using System;

[AttributeUsage(AttributeTargets.Field)]
class ErrorIDMapAttribute : Attribute
{
    public string Description { get; set; }

    public ErrorIDMapAttribute(string Description)
    {
        this.Description = Description;
    }

    public override string ToString()
    {
        return this.Description.ToString();
    }
}

/// <summary>
/// ErrorIDMap 的摘要描述
/// </summary>
public enum ErrorID : int
{

    #region 共用的區域

    [ErrorIDMapAttribute("成功")]
    Success = 0,
    [ErrorIDMapAttribute("參數 Json 解不開")]
    Json_Format_Error = 1,
    [ErrorIDMapAttribute("沒有帶 SessionKey")]
    No_SessionKey = 2,
    [ErrorIDMapAttribute("Session Key 錯誤, 根本不存在")]
    SessionError = 3,
    [ErrorIDMapAttribute("Session Key 逾時")]
    SessionTimeout = 4,

    #endregion

    #region Agent_Account_Create 的錯誤

    [ErrorIDMapAttribute("帳號重覆")]
    Agent_Account_Create_Duplicate_Account = 101,

    #endregion

    #region Check_Account 的錯誤

    [ErrorIDMapAttribute("沒有帳號資料")]
    Check_Account_No_Account = 10001,
    [ErrorIDMapAttribute("沒有密碼資料")]
    Check_Account_No_Password = 10002,
    [ErrorIDMapAttribute("帳號錯誤")]
    Check_Account_No_Such_Account = 10003,
    [ErrorIDMapAttribute("密碼錯誤")]
    Check_Account_No_Such_Password = 10004,

    #endregion

    #region 創角

    [ErrorIDMapAttribute("己有角色存在")]
    Account_CreatePlayer_Exist_Player = 10101,

    #endregion

}

