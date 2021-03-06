﻿/****************************************************************************
Copyright (c) 2013-2015 scutgame.com

http://www.scutgame.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using ZyGames.Framework.Common;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Common.Security;
using ZyGames.Framework.Data.Sql;
using ZyGames.Framework.Game.Runtime;

namespace ZyGames.Framework.Game.Sns
{
	/// <summary>
	/// Reg type.
	/// </summary>
    public enum RegType
    {
        /// <summary>
        /// 正常形式
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 游客通过设备ID登录
        /// </summary>
        Guest,
		/// <summary>
		/// The other.
		/// </summary>
        Other
    }
	/// <summary>
	/// Pwd type.
	/// </summary>
    public enum PwdType
    {
		/// <summary>
		/// The DE.
		/// </summary>
        DES = 0,
		/// <summary>
		/// The M d5.
		/// </summary>
        MD5
    }

    /// <summary>
    /// SnsCenterUser 的摘要说明
    /// </summary>
    public class SnsCenterUser
    {
		/// <summary>
		/// Passwords the encrypt md5.
		/// </summary>
		/// <returns>The encrypt md5.</returns>
		/// <param name="str">String.</param>
        public static string PasswordEncryptMd5(string str)
        {
            return CryptoHelper.RegUser_MD5_Pwd(str);
        }

        /// <summary>
        /// 官网渠道ID
        /// </summary>
        private const string SysetmRetailID = "0000";
        private int _userid;
        /// <summary>
        /// 获得用户ID
        /// </summary>
        /// 
        public int UserId { get { return _userid; } }
        private string _PassportId = String.Empty;
        private string _PassportPwd = String.Empty;
        private string _sNickName = String.Empty;
        private string _deviceID = String.Empty;
        private BaseLog _Logger = new BaseLog();

		/// <summary>
		/// Gets the passport identifier.
		/// </summary>
		/// <value>The passport identifier.</value>
        public string PassportId
        {
            get { return _PassportId; }
        }
		/// <summary>
		/// Gets the password.
		/// </summary>
		/// <value>The password.</value>
        public string Password
        {
            get { return _PassportPwd; }
        }
		/// <summary>
		/// Gets or sets the retail I.
		/// </summary>
		/// <value>The retail I.</value>
        public string RetailID
        {
            get;
            set;
        }
		/// <summary>
		/// Gets or sets the weixin code.
		/// </summary>
		/// <value>The weixin code.</value>
        public string WeixinCode
        {
            get;
            set;
        }
		/// <summary>
		/// Gets or sets the retail user.
		/// </summary>
		/// <value>The retail user.</value>
        public string RetailUser
        {
            get;
            set;
        }
		/// <summary>
		/// Gets or sets the type of the reg.
		/// </summary>
		/// <value>The type of the reg.</value>
        public RegType RegType
        {
            get;
            set;
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="ZyGames.Framework.Game.Sns.SnsCenterUser"/> class.
		/// </summary>
        public SnsCenterUser()
        {
            RegType = RegType.Other;
        }
        /// <summary>
        /// 修改：伍张发
        /// </summary>
        /// <param name="sPassportId"></param>
        /// <param name="passportPwd"></param>
        /// <param name="deviceID"></param>
        public SnsCenterUser(string sPassportId, string passportPwd, string deviceID)
        {
            _PassportId = sPassportId.ToUpper();
            _PassportPwd = passportPwd;
            _deviceID = deviceID;
            RegType = string.IsNullOrEmpty(deviceID) ? RegType.Normal : RegType.Guest;
            RetailID = SysetmRetailID;
        }

        /// <summary>
        /// 增加空密码处理
        /// 修改：伍张发
        /// </summary>
        /// <returns></returns>
        public int GetUserId()
        {
            RegType regType = RegType;
            PwdType pwdType = PwdType.DES;
            SetLoginType(ref regType, ref pwdType, PassportId);
            List<SqlParameter> listTmp = new List<SqlParameter>();
            string sGetSql = "select top 1 userid,PassportId,DeviceID,RegType,RetailID,RetailUser,WeixinCode from SnsUserInfo ";
            string password = _PassportPwd;

            if (regType == RegType.Normal)
            {
                if (pwdType == PwdType.MD5)
                {
                    password = CryptoHelper.DES_Decrypt(password, GameEnvironment.ProductDesEnKey);
                    password = PasswordEncryptMd5(password);
                }
                sGetSql += "where PassportId=@aPassportId and PassportPwd=@PassportPwd";
                listTmp.Add(SqlParamHelper.MakeInParam("@aPassportId", SqlDbType.VarChar, 0, _PassportId));
                listTmp.Add(SqlParamHelper.MakeInParam("@PassportPwd", SqlDbType.VarChar, 0, password));
            }
            else if (regType == RegType.Guest)
            {
                if (pwdType == PwdType.MD5)
                {
                    password = CryptoHelper.DES_Decrypt(password, GameEnvironment.ProductDesEnKey);
                    if (password.Length != 32)
                    {
                        //判断是否已经MD5加密
                        password = PasswordEncryptMd5(password);
                    }
                }
                sGetSql += "where (DeviceID=@DeviceID and PassportPwd=@PassportPwd ) and PassportId=@aPassportId and RegType=@RegType";
                listTmp.Add(SqlParamHelper.MakeInParam("@aPassportId", SqlDbType.VarChar, 0, _PassportId));
                listTmp.Add(SqlParamHelper.MakeInParam("@DeviceID", SqlDbType.VarChar, 0, _deviceID));
                listTmp.Add(SqlParamHelper.MakeInParam("@PassportPwd", SqlDbType.VarChar, 0, password));
                listTmp.Add(SqlParamHelper.MakeInParam("@RegType", SqlDbType.Int, 0, (int)regType));
            }
            else
            {
                sGetSql += "where RetailID=@RetailID and RetailUser=@RetailUser";
                listTmp.Add(SqlParamHelper.MakeInParam("@RetailID", SqlDbType.VarChar, 0, RetailID));
                listTmp.Add(SqlParamHelper.MakeInParam("@RetailUser", SqlDbType.VarChar, 0, RetailUser));
            }
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                SnsCenterUser user = new SnsCenterUser();
                if (aReader.Read())
                {
                    try
                    {
                        _userid = Convert.ToInt32(aReader["userid"]);
                        _PassportId = aReader["PassportId"].ToString();
                        _deviceID = aReader["DeviceID"].ToNotNullString();
                        RegType = aReader["RegType"].ToEnum<RegType>();
                        RetailID = aReader["RetailID"].ToNotNullString();
                        RetailUser = aReader["RetailUser"].ToNotNullString();
                        WeixinCode = aReader["WeixinCode"].ToNotNullString();
                    }
                    catch (Exception ex)
                    {
                        TraceLog.WriteError("GetUserId method error:{0}, sql:{0}", ex, sGetSql);
                    }
                    return _userid;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获取用户类型
        /// </summary>
        /// <returns></returns>
        public RegType GetUserType()
        {
            string sGetSql = "select top 1 PassportId, PassportPwd, RegType from SnsCenter.dbo.SnsUserInfo where PassportId=@PassportId";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@PassportId", SqlDbType.VarChar, 0, PassportId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    return (RegType)Enum.ToObject(typeof(RegType), Convert.ToInt32(aReader["RegType"]));
                }
            }
            return 0;
        }

        /// <summary>
        /// 是否存在账号
        /// </summary>
        /// <returns></returns>
        public bool IsExist()
        {
            string sGetSql = "select top 1 userid from SnsCenter.dbo.SnsUserInfo where PassportId=@aPassportId";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@aPassportId", SqlDbType.VarChar, 0, PassportId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    return true;
                }
            }
            return false;
        }
		/// <summary>
		/// Determines whether this instance is exist retail.
		/// </summary>
		/// <returns><c>true</c> if this instance is exist retail; otherwise, <c>false</c>.</returns>
        public bool IsExistRetail()
        {
            string sGetSql = "select top 1 userid from SnsUserInfo where RetailID=@RetailID and RetailUser=@RetailUser";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@RetailID", SqlDbType.VarChar, 0, RetailID));
            listTmp.Add(SqlParamHelper.MakeInParam("@RetailUser", SqlDbType.VarChar, 0, RetailUser));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    return true;
                }
            }

            //自动获取通行证
            SnsPassport passport = new SnsPassport();
            this._PassportId = passport.GetRegPassport();
            this._PassportPwd = CryptoHelper.DES_Encrypt(passport.GetRandomPwd(), GameEnvironment.ProductDesEnKey);
            return false;
        }

		/// <summary>
		/// Sets the type of the login.
		/// </summary>
		/// <param name="regType">Reg type.</param>
		/// <param name="pwdType">Pwd type.</param>
		/// <param name="passportId">Passport identifier.</param>
        public static void SetLoginType(ref RegType regType, ref PwdType pwdType, string passportId)
        {
            string sGetSql = "select top 1 RegType,DeviceID,PwdType from SnsUserInfo where PassportId=@aPassportId";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@aPassportId", SqlDbType.VarChar, 0, passportId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    string deviceID = Convert.ToString(aReader["DeviceID"]);
                    RegType rt = (RegType)Enum.ToObject(typeof(RegType), Convert.ToInt32(aReader["RegType"]));
                    pwdType = (PwdType)Enum.ToObject(typeof(PwdType), Convert.ToInt32(aReader["PwdType"]));
                    if (rt == RegType.Other && regType != RegType.Other)
                    {
                        //渠道登陆的用户允许更换包登陆
                        regType = string.IsNullOrEmpty(deviceID) ? RegType.Normal : rt;
                    }
                    else
                    {
                        regType = rt;
                    }
                }
            }
        }

        /// <summary>
        /// 是否有绑定DeviceID
        /// </summary>
        /// <returns></returns>
        public static SnsCenterUser GetUserByDeviceID(string deviceID)
        {
            if (deviceID.Length == 0 || deviceID.StartsWith("00:00:00:00:00:"))
            {
                deviceID = Guid.NewGuid().ToString();
            }
            string sGetSql = "select top 1 PassportId, PassportPwd,PwdType, RegType from SnsUserInfo where DeviceID=@DeviceID";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@DeviceID", SqlDbType.VarChar, 0, deviceID));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    PwdType pwdType = (PwdType)Enum.ToObject(typeof(PwdType), Convert.ToInt32(aReader["PwdType"]));
                    string password = Convert.ToString(aReader["PassportPwd"]);
                    if (pwdType == PwdType.MD5)
                    {
                        password = CryptoHelper.DES_Encrypt(password, GameEnvironment.ProductDesEnKey);
                    }

                    SnsCenterUser user = new SnsCenterUser(Convert.ToString(aReader["PassportId"]), password, deviceID);
                    user.RegType = (RegType)Enum.ToObject(typeof(RegType), Convert.ToInt32(aReader["RegType"]));
                    return user;
                }
            }
            return null;
        }
		/// <summary>
		/// Inserts the sns user.
		/// </summary>
		/// <returns>The sns user.</returns>
		/// <param name="paramNames">Parameter names.</param>
		/// <param name="paramValues">Parameter values.</param>
        public int InsertSnsUser(string[] paramNames, string[] paramValues)
        {
            SnsPassport oSnsPassportLog = new SnsPassport();
            if (!oSnsPassportLog.VerifyRegPassportId(_PassportId))
            {
                return 0;
            }
            //md5加密
            string password = CryptoHelper.DES_Decrypt(_PassportPwd, GameEnvironment.ProductDesEnKey);
            password = PasswordEncryptMd5(password);
            string sInsertSql = string.Empty;

            string extColumns = string.Join(",", paramNames);
            extColumns = extColumns.TrimEnd().Length > 0 ? "," + extColumns : string.Empty;
            string paramColumns = string.Join(",@", paramNames);
            paramColumns = paramColumns.TrimEnd().Length > 0 ? ",@" + paramColumns : string.Empty;

            List<SqlParameter> paramsInsert = new List<SqlParameter>();
            sInsertSql = string.Format("insert into SnsUserInfo(passportid, passportpwd, DeviceID, RegType, RegTime,RetailID,RetailUser,PwdType{0})", extColumns);
            sInsertSql += string.Format("values(@aPassportId, @aPassportPwd, @DeviceID, @RegType, @RegTime, @RetailID, @RetailUser,@PwdType{0}) select @@IDENTITY", paramColumns);
            paramsInsert.Add(SqlParamHelper.MakeInParam("@aPassportId", SqlDbType.VarChar, 0, _PassportId));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@aPassportPwd", SqlDbType.VarChar, 0, password));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@deviceID", SqlDbType.VarChar, 0, _deviceID));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@RegType", SqlDbType.Int, 0, (int)RegType));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@RegTime", SqlDbType.DateTime, 0, DateTime.Now));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@RetailID", SqlDbType.VarChar, 0, RetailID));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@RetailUser", SqlDbType.VarChar, 0, RetailUser));
            paramsInsert.Add(SqlParamHelper.MakeInParam("@PwdType", SqlDbType.Int, 0, (int)PwdType.MD5));
            for (int i = 0; i < paramNames.Length; i++)
            {
                paramsInsert.Add(SqlParamHelper.MakeInParam("@" + paramNames[i], SqlDbType.VarChar, 0, paramValues[i]));
            }
            try
            {

                if (!oSnsPassportLog.SetPassportReg(_PassportId))
                {
                    throw new Exception("SetPassportReg Error");
                }
                using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sInsertSql, paramsInsert.ToArray()))
                {
                    if (aReader.Read())
                    {
                        _userid = Convert.ToInt32(aReader[0]);
                    }
                }
                return _userid;
            }
            catch (Exception ex)
            {
                _Logger.SaveLog(ex);
                return 0;
            }
        }
        /// <summary>
        /// 向社区中心添加用户
        /// </summary>
        /// <returns></returns>
        public int InsertSnsUser()
        {
            return InsertSnsUser(new string[0], new string[0]);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public int ChangePass(string userId)
        {
            try
            {
                //md5加密
                string password = CryptoHelper.DES_Decrypt(_PassportPwd, GameEnvironment.ProductDesEnKey);
                password = PasswordEncryptMd5(password);
                string sInsertSql = string.Empty;
                SqlParameter[] paramsUpdate = new SqlParameter[5];
                string condition = " where 1=1";
                if (userId.ToUpper().StartsWith("Z"))
                {
                    condition += " and PassportID=@UserId";
                }
                else
                {
                    condition += " and UserID=@UserId";
                }
                sInsertSql = "update SnsUserInfo set passportpwd=@aPassportPwd,RegType=@RegType,DeviceID=@DeviceID,PwdType=@PwdType" + condition;

                paramsUpdate[0] = SqlParamHelper.MakeInParam("@UserId", SqlDbType.VarChar, 0, userId);
                paramsUpdate[1] = SqlParamHelper.MakeInParam("@aPassportPwd", SqlDbType.VarChar, 0, password);
                paramsUpdate[2] = SqlParamHelper.MakeInParam("@RegType", SqlDbType.Int, 0, (int)RegType.Normal);
                paramsUpdate[3] = SqlParamHelper.MakeInParam("@DeviceID", SqlDbType.VarChar, 0, string.Empty);
                paramsUpdate[4] = SqlParamHelper.MakeInParam("@PwdType", SqlDbType.Int, 0, (int)PwdType.MD5);//MD5

                return SqlHelper.ExecuteNonQuery(config.connectionString, CommandType.Text, sInsertSql, paramsUpdate);
            }
            catch (Exception ex)
            {
                _Logger.SaveLog(ex);
                return 0;
            }
        }
		/// <summary>
		/// Gets the password.
		/// </summary>
		/// <returns>The password.</returns>
		/// <param name="passportId">Passport identifier.</param>
        public string GetPassword(string passportId)
        {
            string sGetSql = "select top 1 PassportPwd from SnsUserInfo where PassportId=@PassportId";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@PassportId", SqlDbType.VarChar, 0, passportId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    return Convert.ToString(aReader["PassportPwd"]);
                }
            }
            return string.Empty;
        }


        internal int ChangeUserInfo(string pid, SnsUser snsuser)
        {
            try
            {
                string sInsertSql = string.Empty;
                List<SqlParameter> paramsUpdate = new List<SqlParameter>();
                paramsUpdate.Add(SqlParamHelper.MakeInParam("@PassportId", SqlDbType.VarChar, 0, pid));

                sInsertSql = "update SnsUserInfo set PassportId=@PassportId";
                if (!string.IsNullOrEmpty(snsuser.Mobile))
                {
                    sInsertSql += ", Mobile=@Mobile";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@Mobile", SqlDbType.VarChar, 0, snsuser.Mobile));
                }
                if (!string.IsNullOrEmpty(snsuser.Mail))
                {
                    sInsertSql += ", Mail=@Mail";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@Mail", SqlDbType.VarChar, 0, snsuser.Mail));
                }
                if (!string.IsNullOrEmpty(snsuser.RealName))
                {
                    sInsertSql += ", RealName=@RealName";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@RealName", SqlDbType.VarChar, 0, snsuser.RealName));
                }
                if (!string.IsNullOrEmpty(snsuser.IDCards))
                {
                    sInsertSql += ", IDCards=@IDCards";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@IDCards", SqlDbType.VarChar, 0, snsuser.IDCards));
                }
                if (!string.IsNullOrEmpty(snsuser.ActiveCode))
                {
                    sInsertSql += ", ActiveCode=@ActiveCode";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@ActiveCode", SqlDbType.VarChar, 0, snsuser.ActiveCode));
                }
                if (snsuser.SendActiveDate > DateTime.MinValue)
                {
                    sInsertSql += ", SendActiveDate=@SendActiveDate";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@SendActiveDate", SqlDbType.DateTime, 0, snsuser.SendActiveDate));
                }
                if (snsuser.ActiveDate > DateTime.MinValue)
                {
                    sInsertSql += ", ActiveDate=@ActiveDate";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@ActiveDate", SqlDbType.DateTime, 0, snsuser.ActiveDate));
                }
                if (!string.IsNullOrEmpty(snsuser.WeixinCode))
                {
                    sInsertSql += ", WeixinCode=@WeixinCode";
                    paramsUpdate.Add(SqlParamHelper.MakeInParam("@WeixinCode", SqlDbType.VarChar, 0, snsuser.WeixinCode));
                }
                sInsertSql += " where PassportId=@PassportId";

                return SqlHelper.ExecuteNonQuery(config.connectionString, CommandType.Text, sInsertSql, paramsUpdate.ToArray());
            }
            catch (Exception ex)
            {
                _Logger.SaveLog(ex);
                return 0;
            }
        }

        internal static bool CheckDevice(string device)
        {
            if (device == string.Empty)
                return true;
            string sGetSql = string.Format("select count([DeviceID]) from LimitDevice where DeviceID = @deviceID ", device);
            SqlParameter[] para = new SqlParameter[] 
            {
                SqlParamHelper.MakeInParam("@deviceID",SqlDbType.VarChar,0,device),
            };
            int count = Convert.ToInt32(SqlHelper.ExecuteScalar(config.connectionString, CommandType.Text, sGetSql, para));
            return count <= 0;
        }

        internal SnsUser GetUserInfo(string passportId)
        {
            SnsUser snsUser = new SnsUser();
            string sGetSql = "select top 1 [UserId],[PassportID],[PassportPwd],[DeviceID],[RegType],[RegTime],[RetailID],[RetailUser],[Mobile],[Mail],[PwdType],[RealName],[IDCards],[ActiveCode],[SendActiveDate],[ActiveDate],WeixinCode from SnsUserInfo where PassportId=@PassportId";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@PassportId", SqlDbType.VarChar, 0, passportId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            SetUserInfo(sGetSql, paramsGet, snsUser);
            return snsUser;
        }

        /// <summary>
        /// 通过微信号
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        internal SnsUser GetUserByWeixin(string openId)
        {
            SnsUser snsUser = new SnsUser();
            string sGetSql = "select top 1 [UserId],[PassportID],[PassportPwd],[DeviceID],[RegType],[RegTime],[RetailID],[RetailUser],[Mobile],[Mail],[PwdType],[RealName],[IDCards],[ActiveCode],[SendActiveDate],[ActiveDate],WeixinCode from SnsUserInfo where WeixinCode=@WeixinCode";
            List<SqlParameter> listTmp = new List<SqlParameter>();
            listTmp.Add(SqlParamHelper.MakeInParam("@WeixinCode", SqlDbType.VarChar, 0, openId));
            SqlParameter[] paramsGet = listTmp.ToArray();
            SetUserInfo(sGetSql, paramsGet, snsUser);
            return snsUser;
        }

        private void SetUserInfo(string sGetSql, SqlParameter[] paramsGet, SnsUser snsUser)
        {
            using (SqlDataReader aReader = SqlHelper.ExecuteReader(config.connectionString, CommandType.Text, sGetSql, paramsGet))
            {
                if (aReader.Read())
                {
                    snsUser.UserId = Convert.ToInt32(aReader["UserId"]);
                    snsUser.PassportId = Convert.ToString(aReader["PassportID"]);
                    snsUser.RegTime = Convert.ToDateTime(aReader["RegTime"]);
                    snsUser.RetailID = Convert.ToString(aReader["RetailID"]);
                    snsUser.RetailUser = Convert.ToString(aReader["RetailUser"]);
                    snsUser.Mobile = Convert.ToString(aReader["Mobile"]);
                    snsUser.Mail = Convert.ToString(aReader["Mail"]);
                    snsUser.RealName = Convert.ToString(aReader["RealName"]);
                    snsUser.IDCards = Convert.ToString(aReader["IDCards"]);
                    snsUser.ActiveCode = Convert.ToString(aReader["ActiveCode"]);
                    snsUser.SendActiveDate = ToDate(Convert.ToString(aReader["SendActiveDate"]));
                    snsUser.ActiveDate = ToDate(Convert.ToString(aReader["ActiveDate"]));
                    snsUser.WeixinCode = Convert.ToString(aReader["WeixinCode"]);
                }
            }
        }

        private DateTime ToDate(string str)
        {
            DateTime result = new DateTime();
            DateTime.TryParse(str, out result);
            return result;
        }

		/// <summary>
		/// Adds the login log.
		/// </summary>
		/// <param name="deviceID">Device I.</param>
		/// <param name="PassportID">Passport I.</param>
        public static void AddLoginLog(string deviceID, string PassportID)
        {
            if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(PassportID))
            {
                return;
            }
            string sql = "insert into PassportLoginLog values(@deviceID,@passportid,@logintime)";
            SqlParameter[] para = new SqlParameter[] 
            {
                SqlParamHelper.MakeInParam("@deviceID",SqlDbType.VarChar,0,deviceID),
               SqlParamHelper.MakeInParam("@passportid",SqlDbType.VarChar,0,PassportID),
               SqlParamHelper.MakeInParam("@logintime",SqlDbType.DateTime,0,DateTime.Now),
            };
            SqlHelper.ExecuteNonQuery(config.connectionString, CommandType.Text, sql, para);
        }
    }
}