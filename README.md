# すこやかバディ（デモ版）

運動・睡眠・食事の記録に応じてキャラクター「すこやかバディ」が反応し、日を重ねるごとに成長・進化するUnity WebGL製のルールベース育成ゲームのデモです。仕様の正は [requirements.md](requirements.md) を参照してください。

## 遊び方

1. たまごの状態から、記録フォーム（運動・睡眠・食事）を入力するか、プリセット（元気な一日／だらけた一日／がんばりすぎ／ちょっと不足）を選んで一括入力します。
2. 「1日を終える」を押すと、バディが反応し、ステータス・経験値が増減します。
3. 仮想日（入力1回分）を繰り返すと、段階（たまご→こども→おとな→たつじん）が進み、育て方に応じた成長タイプ（アスリート型／のんびり型／グルメ型／バランス型）に分岐します。
4. がんばりすぎ・寝不足が続くとつかれが蓄積して体調を崩し、休むと回復します。
5. 「はじめから」でいつでも最初からやり直せます。

外部API・認証・DBは使用せず、記録はすべて端末のPlayerPrefs（ブラウザローカル）にのみ保存されます。個人情報は一切入力しません。

## ビルド・テスト

Unity 6000.4.10f1 が必要です。

```bash
# EditModeテストの実行
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults results.xml -quit

# WebGLビルド（Build/WebGL/ に出力）
Unity.exe -batchmode -nographics -projectPath . -executeMethod SukoyakaBuddy.EditorTools.Build.WebGL -quit
```

ビルド成果物（`.data.gz`・`.wasm.gz`）はgzip圧縮されているため、`python -m http.server` では正しく配信されません。`.claude/gzip_static_server.py`（ローカル確認専用、`.gitignore`で除外）で `Content-Encoding: gzip` を付与して配信してください。

```bash
python .claude/gzip_static_server.py 8743 Build/WebGL
```

## 技術構成

- Unity 6000.4.10f1 / C# / uGUI（Editor GUI操作なし、シーン・UIはすべてコードで動的生成）
- データ保存：PlayerPrefsのみ
- ロジックはすべてルールベース（AI機能なし）

## リポジトリ構成

```
Assets/Scripts/
├── Boot/          # GameBootstrap・GameController（起動・状態機械）
├── Core/          # InputNormalizer・DayScorer・GrowthEngine・MoodJudge・PresetCatalog
├── Model/         # Character・DailyLog・DayScore等のデータ型・定数
├── Persistence/   # SaveRepository・ResetPolicy
├── View/          # BuddyView・StatusPanel・HistoryChart・InputForm等
└── Editor/        # Build（CLIビルド用エントリ）
Assets/Tests/EditMode/  # Core・Persistenceの規則を検証するテスト
```
