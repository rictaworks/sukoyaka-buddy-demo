using NUnit.Framework;
using SukoyakaBuddy.Model;
using SukoyakaBuddy.Persistence;
using UnityEngine;

namespace SukoyakaBuddy.Tests.EditMode
{
    public class SaveRepositoryTests
    {
        // PlayerPrefsはEditorのテスト実行環境ごとに共有される領域のため、他のテストと干渉しないよう
        // 各テストの前後でこのプロジェクトが使うキーのみを消す（requirements.md 12.3節の検証対象そのもの）。
        [SetUp]
        [TearDown]
        public void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void SaveThenLoad_RoundTripsAllFields()
        {
            var repo = new SaveRepository();
            var character = Character.NewEgg();
            character.DayNo = 3;
            character.Exp = 150;
            character.Power = 40;
            character.Energy = 55;
            character.Body = 60;
            character.Fatigue = 20;
            character.Streak = 2;
            character.History.Add(new DayRecord(2, 70, 80, 65));

            bool saved = repo.Save(character, "2026-09-06");
            Assert.IsTrue(saved);

            var loaded = repo.Load("2026-09-06", out bool isNewGame);
            Assert.IsFalse(isNewGame);
            Assert.AreEqual(character.DayNo, loaded.DayNo);
            Assert.AreEqual(character.Exp, loaded.Exp);
            Assert.AreEqual(character.Power, loaded.Power);
            Assert.AreEqual(character.Energy, loaded.Energy);
            Assert.AreEqual(character.Body, loaded.Body);
            Assert.AreEqual(character.Fatigue, loaded.Fatigue);
            Assert.AreEqual(character.Streak, loaded.Streak);
            Assert.AreEqual(1, loaded.History.Count);
            Assert.AreEqual(2, loaded.History[0].DayNo);
        }

        [Test]
        public void Load_WithNoSchemaVersion_ReturnsNewEgg()
        {
            var repo = new SaveRepository();
            var loaded = repo.Load("2026-09-06", out bool isNewGame);
            Assert.IsTrue(isNewGame);
            Assert.AreEqual(1, loaded.DayNo);
            Assert.AreEqual(0, loaded.Exp);
            Assert.AreEqual(GameConstants.InitialStat, loaded.Power);
        }

        [Test]
        public void Load_WithSchemaVersionMismatch_ReturnsNewEgg()
        {
            PlayerPrefs.SetInt("schema_version", GameConstants.SchemaVersion + 1);
            var repo = new SaveRepository();
            var loaded = repo.Load("2026-09-06", out bool isNewGame);
            Assert.IsTrue(isNewGame);
        }

        [Test]
        public void Load_WithOutOfRangeStat_ReturnsNewEgg_NoPartialRestore()
        {
            var repo = new SaveRepository();
            var character = Character.NewEgg();
            repo.Save(character, "2026-09-06");
            // 保存後にpowerだけ不正な値へ書き換える（部分破損の再現）。
            PlayerPrefs.SetInt("power", 999);

            var loaded = repo.Load("2026-09-06", out bool isNewGame);
            Assert.IsTrue(isNewGame, "1項目でも不正なら部分復元せず新規開始とする");
        }

        [Test]
        public void Load_SickFatigueInconsistency_IsCorrectedByFatigue()
        {
            var repo = new SaveRepository();
            var character = Character.NewEgg();
            character.IsSick = true;
            character.Fatigue = 30; // 体調不良なのにつかれ50未満 -> 矛盾
            repo.Save(character, "2026-09-06");

            var loaded = repo.Load("2026-09-06", out _);
            Assert.IsFalse(loaded.IsSick, "つかれの値を正として体調不良の有無を直す");
        }
    }
}
