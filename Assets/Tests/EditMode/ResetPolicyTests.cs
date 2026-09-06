using System;
using NUnit.Framework;
using SukoyakaBuddy.Persistence;
using UnityEngine;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class ResetPolicyTests
    {
        [SetUp]
        [TearDown]
        public void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void BusinessDate_JstThreeAM_IsBoundary()
        {
            // JST 03:00:00 ちょうど -> 営業日は当日。
            var atBoundary = new DateTime(2026, 9, 5, 18, 0, 0, DateTimeKind.Utc); // JST 2026-09-06 03:00
            Assert.AreEqual("2026-09-06", ResetPolicy.BusinessDate(atBoundary));

            // JST 02:59:59 -> まだ前日の営業日（深夜のプレイは前日扱い）。
            var beforeBoundary = new DateTime(2026, 9, 5, 17, 59, 59, DateTimeKind.Utc); // JST 2026-09-06 02:59:59
            Assert.AreEqual("2026-09-05", ResetPolicy.BusinessDate(beforeBoundary));
        }

        [Test]
        public void CheckAndReset_FirstRun_ReturnsTrueAndStoresDate()
        {
            var policy = new ResetPolicy();
            bool isNewGame = policy.CheckAndReset(DateTime.UtcNow);
            Assert.IsTrue(isNewGame);
        }

        [Test]
        public void CheckAndReset_SameBusinessDate_ReturnsFalse()
        {
            var policy = new ResetPolicy();
            var now = new DateTime(2026, 9, 6, 1, 0, 0, DateTimeKind.Utc);
            policy.CheckAndReset(now);

            bool secondCall = policy.CheckAndReset(now);
            Assert.IsFalse(secondCall, "同じ営業日内は既存データを維持する");
        }

        [Test]
        public void CheckAndReset_DifferentBusinessDate_ReturnsTrue_AndClearsOldData()
        {
            var policy = new ResetPolicy();
            var day1 = new DateTime(2026, 9, 6, 1, 0, 0, DateTimeKind.Utc);
            policy.CheckAndReset(day1);
            PlayerPrefs.SetInt("exp", 500); // 前日の何らかの保存値を模す。

            var day2 = new DateTime(2026, 9, 7, 1, 0, 0, DateTimeKind.Utc);
            bool isNewGame = policy.CheckAndReset(day2);

            Assert.IsTrue(isNewGame);
            Assert.IsFalse(PlayerPrefs.HasKey("exp"), "営業日が変わったら全削除される");
        }
    }
}
