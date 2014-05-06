﻿// Author : dandanshih
// Desc: 所使用到的DB

// [History]
// 2014/5/5 加入 MSSQL 的連線版本

using System;
using System.Collections.Generic;

public class UseDB
{
    #region 帳號 DB
    private static ISQL _AccountDB = null;
	public static ISQL AccountDB
    {
        get
        {
            if (_AccountDB == null)
            {
				try
				{
					_AccountDB = new CMySQL("127.0.0.1", "Sanguo", "sa", "koske1984");
				}
				catch
				{
					_AccountDB = new CMSSQL("db.08online.rd1.sgt", "51098", "Demo", "sa", "sqlgosmio2749");
				}
            }
            return _AccountDB;
        }
    }
    #endregion

    #region 遊戲 DB
	private static ISQL _GameDB = null;
	public static ISQL GameDB
    {
        get
        {
			//if (_GameDB == null)
			//{
			//	_GameDB = new CMySQL("127.0.0.1", "Sanguo", "sa", "koske1984");
			//}
			try
			{
				_GameDB = new CMySQL("127.0.0.1", "Sanguo", "sa", "koske1984");
			}
			catch
			{
				_GameDB = new CMSSQL("db.08online.rd1.sgt", "51098", "Demo", "sa", "sqlgosmio2749");
			}
			return _GameDB;
        }
    }
    #endregion

    #region 遊戲 Log DB
	private static ISQL _GameLogDB = null;
	public static ISQL GameLogDB
    {
        get
        {
			//if (_GameLogDB == null)
			//{
			//	_GameLogDB = new CMySQL("127.0.0.1", "Sanguo", "sa", "koske1984");
			//}
			try
			{
				_GameLogDB = new CMySQL("127.0.0.1", "Sanguo", "sa", "koske1984");
			}
			catch
			{
				_GameLogDB = new CMSSQL("db.08online.rd1.sgt", "51098", "Demo", "sa", "sqlgosmio2749");
			}
			return _GameLogDB;
        }
    }
    #endregion

}

