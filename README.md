# [Yog-Sothoth's Yard Unofficial Japanese Translation Mod](https://github.com/fronsglaciei/ysytrans/releases)

**Yog-Sothoth's Yard**の非公式日本語化MODを公開するリポジトリです.

Unity汎用Moddingフレームワークの**BepInEx**のプラグインとして作成しています.

## 動作環境

当MODの最新版は以下の環境で作成および動作確認しています.

* Yog-Sothoth's Yard - steam v1.0.11
* BepInEx IL2CPP 6.0.0 build 692
* Windows 11

<details>
<summary>当MOD v1.0.2の動作環境</summary>

* Yog-Sothoth's Yard - steam 2024/06/28版
* BepInEx 5.4.23.2
* Windows 11

</details>

## MOD導入

以下の手順に従って導入してください.

#### 1.BepInExの導入

[BepInEx](https://builds.bepinex.dev/projects/bepinex_be)のIL2CPP Windows 64bit版`BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.692+851521c.zip`をダウンロードして展開の後, 以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * **winhttp.dll**
  * **doorstop_config.ini**
  * **dotnet**
  * **BepInEx**

⚠ 注意 ⚠
Program Files以下など管理者権限が必要なディレクトリにインストールされている場合の動作は確認していません.

![BepInExのダウンロード](assets/00_download.png "BepInExのダウンロード")

#### 2.当MODの導入

[Releases](https://github.com/fronsglaciei/YSYMod.Translations/releases)から最新の`FG.Mods.YSYard.Translations.zip`をダウンロードして展開の後, 以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * winhttp.dll
  * doorstop_config.ini
  * dotnet
  * BepInEx
    * core等のBepInEx公式フォルダ
    * **plugins**
      * **FG.Mods.YSYard.Translations**
        * **FG.Defs.YSYard.Translations.dll**
        * **FG.Mods.YSYard.Translations.dll**
        * **translatedLanguages.json**
        * **translatedLanguageTalks.json**

![ファイルの配置](assets/00_directory.png "ファイルの配置")

<details>
<summary>当MOD v1.0.2の導入手順</summary>

#### 1.BepInExの導入

[BepInEx](https://github.com/BepInEx/BepInEx/releases)の64bit版`BepInEx_win_x64_5.4.23.2.zip`をダウンロードして展開の後, 以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * **winhttp.dll**
  * **doorstop_config.ini**
  * **BepInEx**

⚠ 注意 ⚠
Program Files以下など管理者権限が必要なディレクトリにインストールされている場合の動作は確認していません.

#### 2.当MODの導入

[Releases](https://github.com/fronsglaciei/YSYMod.Translations/releases)からv1.0.2の`FG.Mods.YSYard.Translations.zip`をダウンロードして展開の後, 以下のようなディレクトリ階層に配置してください.

* Yog-Sothoth's Yardインストール先(Steam)\steamapps\common\Yog-Sothoth's Yard等
  * Yog-Sothoth's Yard.exe等の公式ファイル
  * winhttp.dll
  * doorstop_config.ini
  * BepInEx
    * core等のBepInEx公式フォルダ
    * **plugins**
      * **FG.Mods.YSYard.Translations**
        * **FG.Defs.YSYard.Translations.dll**
        * **FG.Mods.YSYard.Translations.dll**
        * **translatedLanguages.json**
        * **translatedLanguageTalks.json**

![ファイルの配置](assets/00_directory_old.png "ファイルの配置")

</details>

<details>
<summary>導入するバージョンの選び方</summary>

ゲーム本体のバージョンが最新の場合, 当MOD[最新版の導入手順](#1bepinexの導入)に従ってください.

ゲーム本体のバージョンがわからない場合, インストールされたディレクトリの構成を確認し, `GameAssembly.dll`が`Yog-Sothoth's Yard.exe`と同じ階層に存在する場合は, 当MOD[最新版の導入手順](#1bepinexの導入)に従ってください. 存在しない場合, 当MOD[v1.0.2の導入手順](#1bepinexの導入-1)に従ってください.

</details>

## MOD使用

MODの導入に成功すると, ゲーム内設定画面で日本語が選択可能になります.

⚠ 注意 ⚠

* IL2CPP版のBepInExは初回起動に少し時間がかかります. ゲームのスタート画面が表示されるまでお待ちください.
* BepInExのバージョンによってはログ出力用のコンソールウィンドウが同時に起動することがあります. 起動したくない場合は`BepInEx\config\BepInEx.cfg`を編集し`Logging.Console`セクション
で`Enabled = false`に設定してください.

![設定画面](assets/01_settings.png "設定画面")

## MOD削除

以下の手順に従って削除してください.

1. ゲーム内設定画面で中国語か英語を選択して終了
2. MOD導入の際に追加した全ファイルを削除

手順1を忘れてファイルを削除してしまった場合も同様に, ゲーム内設定画面で中国語か英語を選択してゲームを終了してください.

## 注意

当MODの使用は自己責任でお願いします.
