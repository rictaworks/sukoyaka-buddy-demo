using System;
using System.Globalization;
using SukoyakaBuddy.Model;
using UnityEngine;

namespace SukoyakaBuddy.Persistence
{
    /// <summary>関数H：日付確認とリセット（requirements.md 4.8節）。ページロード時のみ呼ばれる。</summary>
    public class ResetPolicy
    {
        private const string KeyPlayDate = "play_date";
        private const string DateFormat = "yyyy-MM-dd";

        /// <summary>UTCに9時間を加えてJSTとし、そこから3時間引いた日付を営業日とする（ブラウザのタイムゾーン設定に依存しない）。</summary>
        public static string BusinessDate(DateTime utcNow)
        {
            var jst = utcNow.AddHours(GameConstants.JstOffsetHours);
            var businessDate = jst.AddHours(-GameConstants.BusinessDayOffsetHours).Date;
            return businessDate.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>保存されている営業日と異なれば（または未保存なら）全削除して新規開始とする。</summary>
        public bool CheckAndReset(DateTime utcNow)
        {
            string today = BusinessDate(utcNow);
            string storedPlayDate = PlayerPrefs.HasKey(KeyPlayDate) ? PlayerPrefs.GetString(KeyPlayDate) : null;

            if (storedPlayDate == today)
            {
                return false; // 同じ営業日：続きから
            }

            // PlayerPrefsは端末ローカルのゲームデータのみを保持する保存領域であり、
            // requirements.md F1・F12・DFDで明示された仕様どおりの全削除（本人のリポジトリ・ファイルシステムではない）。
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString(KeyPlayDate, today);
            PlayerPrefs.Save();
            return true; // 新規開始
        }
    }
}
