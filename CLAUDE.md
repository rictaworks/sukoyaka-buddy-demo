# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Claude Safety Rules（最優先）

### 削除系コマンドの禁止（重要）

以下のルールはこのワークスペース内のすべての会話で絶対に守られる：

- Claude はファイルまたはディレクトリを削除するコマンドを一切生成してはならない。
  例：rm, rm -rf, rm *, rmdir, unlink, cache --delete,
      lftp mirror --delete, rsync --delete, git clean -df, find -delete 等。

- 削除が必要な場合でも、Claude は削除コマンドを提案せず、
  「手動で削除してください」といった説明に留めること。

- 削除の推奨・削除操作の自動判断も禁止。

- ssh / lftp / デプロイ系スクリプトを生成する場合でも、
  削除コマンドの生成は禁止。

これらはすべての会話・コード生成に適用される。

### シークレット管理（重要）

- `config/master.key` など機密ファイルを `git add` するコードを生成してはならない
- デプロイスクリプト・セットアップ手順でも同様
- シークレットは必ず環境変数（RAILS_MASTER_KEY 等）で渡すこと
- `.gitignore` への追加を確認する手順を必ずコードに含めること
- 初回コミット前に `git status` でステージング確認を促すこと

## プロジェクト概要

運動・睡眠・食事の記録に応じてキャラクター「すこやかバディ」が反応し、日を重ねるごとに成長・進化するUnity WebGL製のルールベース育成ゲームのデモ版。展示会場で来場者が「たまごから始める→仮想日（入力1回分）の運動・睡眠・食事を記録する→バディが反応しステータス・経験値が増減する→数日分繰り返すと段階（たまご→こども→おとな→たつじん）が進み成長タイプが分岐する→がんばりすぎ・寝不足が続くと体調を崩し休むと回復する」という一連の体験を、ブラウザ1つ・数分で完結して行える展示物とすることが目的。

仕様の正は [requirements.md](requirements.md)（設計書。機能一覧・ロジック仕様・ER図・DFD・シーケンス図・クラス図・状態遷移図・ユースケース図まで含む）。表記揺れでの書き換えは行わず、設計変更があった場合のみ改訂する。

デモ版としての制約（requirements.md 1.4節）：外部API・認証・セッション・DBを一切持たず、すべてのロジックはルールベース、データ保存はPlayerPrefs（端末ローカル）のみ。個人情報（氏名・生年月日等）は一切入力させない。実装は1 issueのワンショットを前提とする。

## コマンド

現時点でAssets/配下の実装は未着手（リポジトリにはrequirements.mdとテンプレート一式のみ存在）。requirements.md 12.6節で定められているビルド・テスト方針：

- ビルド：シーン・UI・キャラクターはすべてコードで動的生成し、Unity EditorのGUI操作を不要とする。CLIビルドは `Unity.exe -batchmode -nographics -executeMethod <ビルドエントリ>` で実行する（エントリは `Assets/Scripts/Editor/` に実装する）。
- テスト：`Assets/Tests/` にUnity Test Framework の EditModeテストを置き、Core（InputNormalizer・DayScorer・GrowthEngine・MoodJudge）とPersistence（SaveRepository・ResetPolicy）の規則を検証する。具体的なCLI実行コマンド（テストランナーの呼び出し方）は実装時にビルドエントリと合わせて定め、ここに追記すること。
- Unity Playへのアップロードはビルド成果物をZIPで手動ドラッグ＆ドロップする（人間が行う。CLI化しない）。

## アーキテクチャ

すべてコード生成（プレハブ・シーンファイル・画像アセットに依存しない）。全体はrequirements.md 9章のクラス図の通り、以下の層に分かれる。

- **Boot**：`GameBootstrap` がシーン・UIを動的生成し `GameController` を作る。
- **Core（ロジック本体）**：`GameController` が入力→成長→表示の1サイクルを統括する。パイプラインは `InputNormalizer`（関数A・正規化）→`DayScorer`（関数B・日次スコア算出）→`GrowthEngine`（関数C・成長適用）→`MoodJudge`（関数D・きぶん判定）の順（詳細ロジックはrequirements.md 4章）。`PresetCatalog` が4種のプリセット入力を提供する。
- **Model**：`Character`（段階・成長タイプは経験値等から求める派生値であり保存しない）、`DailyLog`、`DayScore`、`GrowthResult` 等。
- **Persistence**：`SaveRepository`（PlayerPrefsへの保存・読込、版数/型/範囲の検証、部分復元はしない）と `ResetPolicy`（JST 03:00を境とした営業日判定、ページロード時のみ実行）。
- **View**：`BuddyView`・`StatusPanel`・`HistoryChart`・`InputForm`。段階・きぶん・体調は色だけでなく表情・形・文言でも区別する。

ゲーム本体は状態機械として `LOADING → INPUT → PROCESSING → REACTING → (EVOLVING) → INPUT` のサイクルを回す（requirements.md 10.3節）。PROCESSING/REACTING/EVOLVING中は「1日を終える」「はじめから」を受け付けない。仮想日番号による二重適用防止と処理中フラグの二重防御がある（4.5節）。

保存先はPlayerPrefsのキー・値のみで、論理的に `save_meta` / `character` / `daily_history` の3つのまとまりに分かれる（5章）。段階・きぶんは保存せず、経験値・スコアから毎回再計算する。

## 開発フロー・ルール

- ブランチ運用：`main` への直接コミット・プッシュを禁止する（ドキュメントのみの変更でも例外なし）。必ず `git checkout -b <branch>` でブランチを切り、`gh pr create` でPRを作成する。
- コミット前に security review、マージ前に reviewer と pr-checker を通す（フックはこのリポジトリをプロジェクトディレクトリにしたセッションでのみ発火する）。
- 参照する共有チェックリスト：`.claude/OWASP10.md`・`.claude/QC10.md`・`.claude/CC.md`・`DOCS/CRAP.md`・`DOCS/TM.md`（ローカルのみ配置。publicリポジトリのため `.claude/*` は `.gitignore` で除外されコミットされない）。
- リリースフロー（デモ版のため簡略化）：`issue > setting & coding > security review > add, commit, push > reviewer & pr-checker > merge > user test` のみ。code-review・audit・security-gate・正式release・reportは省略してよい。
- 規模は1 issueワンショット実装のため、`.claude/agents/` の追加サブエージェント（director等）は原則作らない。
- コーディング規約：制御構文・条件構文以外はクラスまたは関数に書く。グローバルな可変状態（static）を禁止する。文言・定数はrequirements.md 4.9節の定数一覧に準拠し、ハードコードせず設定として分離する。ネイティブの `alert()`/`confirm()`/`prompt()` に相当する割り込みダイアログは使わずUI内で完結させる。
- 口調方針：README・UI説明文・ステータスパネル等の一般コンテンツはですます調で書く。ただしバディの吹き出し（キャラクター台詞）はキャラクター性のためくだけた口調のまま実装し、ですます調へ書き換えない（requirements.md 3.3節の例が正）。
- README.mdと `docs/spec.md` に未実装のものを書かない。フォールバックで処理を握りつぶさず、例外処理とデバッグトレース可能なログ出力を書く。
- CI/CDはコンパイル確認とEditModeテスト実行をCIとして用意する。CD（Unity PlayへのアップロードとGitHub Releases発行）は人間が手動で行うため自動化しない。
- 連絡先に個人名を使用しない。必要な場合は `info@rictaworks.jp` とする。
