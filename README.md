# Yog-Sothoth's Yard Unofficial Japanese Translation Mod

**Yog-Sothoth's Yard**の非公式日本語化MODを公開するリポジトリです.

Unity汎用Moddingフレームワークの**BepInEx**のプラグインとして作成しています.

## 動作環境

以下の環境で作成および動作確認しています.

* Yog-Sothoth's Yard - ver1.0.10
* BepInEx 5.4.22
* Windows 11

## MOD導入

以下の手順に従って導入してください.

#### 1.BepInExの導入

[BepInEx](https://github.com/BepInEx/BepInEx/releases)の64bit版`BepInEx_x64_5.4.22.0.zip`をダウンロードして以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * **winhttp.dll**
  * **doorstop_config.ini**
  * **BepInEx**

*! 注意 !*
Program Files以下など管理者権限が必要なディレクトリにインストールされている場合の動作は確認していません.

#### 2.当MODの導入

[Releases](https://github.com/fronsglaciei/YSYMod.Translations/releases)から`fg.mods.ysyard.translations.jp.zip`をダウンロードして以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * winhttp.dll
  * doorstop_config.ini
  * BepInEx
	* core等のBepInEx公式フォルダ
	* **plugins**
	  * **fg.mods.ysyard.translations.jp**
		* **FG.Defs.YSYard.Translations.dll**
		* **FG.Mods.YSYard.Translations.dll**
		* **translatedLanguages.json**
		* **translatedLanguageTalks.json**

![ファイルの配置](assets/00_directory.png "ファイルの配置")

## MOD使用

MODの導入に成功すると, ゲーム内設定画面で日本語が選択可能になります.

![設定画面](assets/01_settings.png "設定画面")

## MOD削除

以下の手順に従って削除してください.

1. ゲーム内設定画面で中国語か英語を選択して終了
2. MOD導入の際に追加した全ファイルを削除

手順1を忘れてファイルを削除してしまった場合も同様に, ゲーム内設定画面で中国語か英語を選択してゲームを終了してください.

## 注意

当MODの使用は自己責任でお願いします.
