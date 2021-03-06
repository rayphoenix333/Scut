﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using ZyGames.DirCenter.CacheData;
using ZyGames.DirCenter.Model;

namespace ZyGames.DirCenter
{
    /// <summary>
    /// Service 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://dir.36you.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class DirService : System.Web.Services.WebService
    {
        [WebMethod(Description = "官方游戏列表")]
        public GameInfo[] GetOfficialGame()
        {
            CacheGame cacheGame = new CacheGame();
            return cacheGame.GetOfficialGame();
        }

        [WebMethod(Description = "游戏列表")]
        public GameInfo GetGameObj(int gameID)
        {
            CacheGame cacheGame = new CacheGame();
            return cacheGame.GetGame(gameID);
        }

        [WebMethod(Description = "游戏列表")]
        public GameInfo[] GetGame()
        {
            CacheGame cacheGame = new CacheGame();
            return cacheGame.GetGame();
        }
        [WebMethod(Description = "游戏信息")]
        public GameInfo GetGameInfo(int gameId)
        {
            CacheGame cacheGame = new CacheGame();
            var list = cacheGame.GetGame();
            var gameInfo = new GameInfo();
            foreach (var item in list)
            {
                if(item.ID==gameId)
                {
                    gameInfo = item;
                    break;
                }
            }
            return gameInfo;
        }
        [WebMethod(Description = "增加游戏")]
        public void AddGame(int gameID, string gameName)
        {
            CacheGame cacheGame = new CacheGame();
            cacheGame.AddToCache(new GameInfo() { ID = gameID, Name = gameName });
        }

        [WebMethod(Description = "增加游戏")]
        public void AddGameNew(int gameID, string gameName, string currency, decimal multiple, string gameWord, string agentsID, bool isRelease, DateTime releaseDate, string payStyle, string SocketServer, int SocketPort)
        {
            CacheGame cacheGame = new CacheGame();
            cacheGame.AddToCache(new GameInfo()
            {
                ID = gameID,
                Name = gameName,
                Currency = currency,
                Multiple = multiple,
                GameWord = gameWord,
                AgentsID = agentsID,
                IsRelease = isRelease,
                ReleaseDate = releaseDate,
                PayStyle = payStyle,
                SocketServer = SocketServer,
                SocketPort = SocketPort
            });
        }

        [WebMethod(Description = "增加游戏新服")]
        public void AddServer(int gameID, int serverId, string serverName, string serverUrl, string status, string intranetAddress)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.AddToCache(new ServerInfo()
            {
                ID = serverId,
                GameID = gameID,
                ServerName = serverName,
                ServerUrl = serverUrl,
                Status = status,
                IntranetAddress = intranetAddress
            });
        }

        [WebMethod(Description = "设置服务器")]
        public void SetServer(int serverID, int gameID, string serverName, string serverUrl, string status, int weight, string intranetAddress)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.SetServer(serverID, gameID, serverName, serverID, serverUrl, status, weight, intranetAddress);
        }

        [WebMethod(Description = "游戏服务器列表")]
        public ServerInfo[] GetServers(int gameID, bool isSort, bool isEnable)
        {
            CacheServer cacheServer = new CacheServer();
            return cacheServer.GetServers(gameID, isSort, isEnable);
        }

        [WebMethod(Description = "游戏服务器列表")]
        public ServerInfo[] GetServerList(int gameID)
        {
            CacheServer cacheServer = new CacheServer();
            return cacheServer.GetServerList(gameID, false);
        }

        [WebMethod(Description = "游戏服务器排序列表")]
        public ServerInfo[] GetServerSortList(int gameID)
        {
            CacheServer cacheServer = new CacheServer();
            return cacheServer.GetServerList(gameID, true);
        }

        [WebMethod(Description = "删除游戏服务器")]
        public void RemoveServer(int gameID, int serverID)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.RemoveServer(gameID, serverID);
        }

        [WebMethod(Description = "删除游戏")]
        public void RemoveGame(int gameID)
        {
            CacheGame cacheGame = new CacheGame();
            cacheGame.RemoveGame(gameID);
        }

        [WebMethod(Description = "设置服务器活跃值")]
        public void SetActiveNum(int serverID, int gameID, int activeNum)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.SetActiveNum(serverID, gameID, activeNum);
        }

        [WebMethod(Description = "设置服务器刷新")]
        public bool ReloadServer(int gameID)
        {
            CacheServer cacheServer = new CacheServer();
            return cacheServer.ReloadServer(gameID);
        }

        [WebMethod(Description = "获取游戏名称")]
        public string GetGameName(int gameID)
        {
            CacheGame cacheGame = new CacheGame();
            return cacheGame.GetGameName(gameID);
        }

        [WebMethod(Description = "获取服务器名称")]
        public string GetServerName(int gameID, int serverID)
        {
            CacheServer cacheServer = new CacheServer();
            return cacheServer.GetServerName(gameID, serverID);
        }

        [WebMethod(Description = "游戏服务器开启时间")]
        public void SetServerEnableDate(int gameID, int serverID, DateTime enableDate)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.SetServerEnableDate(gameID, serverID, enableDate);
        }


        [WebMethod(Description = "是否启用游戏服务器")]
        public void EnableServer(int gameID, int serverID, bool isEnable)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.EnableServer(gameID, serverID, isEnable);
        }

        [WebMethod(Description = "分服状态")]
        public void ServerStatus(int gameID, int serverID, int Status)
        {
            CacheServer cacheServer = new CacheServer();
            cacheServer.ServerStatus(gameID, serverID, Status);
        }

    }
}
