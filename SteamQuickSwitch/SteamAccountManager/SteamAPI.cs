using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Models;

namespace SteamQuickSwitch
{
    public static class SteamAPI
    {
        static readonly string APIKey = PrivateInfoLibrary.PrivateData.SteamAPIKey;

        public static string GetNicknameFromSteamID(string steamID3)
        {
            return GetPlayerSummary(steamID3).Result.Data.Nickname;
        }

        public static string GetGameNameFromID(string appID)
        {
            return GetSteamAppModel(appID).Result.Name;
        }

        private static async Task<SteamWebAPI2.Utilities.ISteamWebResponse<Steam.Models.SteamCommunity.PlayerSummaryModel>> GetPlayerSummary(string steamID3)
        {
            uint uintAccountID = (uint)Convert.ToUInt64(Int32.Parse(steamID3));

            SteamUser steamUser = new SteamUser(APIKey);
            SteamId sid = new SteamId(uintAccountID);

            var playerSummary = await steamUser.GetPlayerSummaryAsync(sid.To64Bit());

            return playerSummary;
        }

        private static async Task<Steam.Models.SteamStore.StoreAppDetailsDataModel> GetSteamAppModel(string appID)
        {
            uint uintAppID = (uint)Int32.Parse(appID);

            var steamStore = new SteamStore();
            var appDetais = await steamStore.GetStoreAppDetailsAsync(uintAppID);
            
            return appDetais;
        }
        
    }
}
