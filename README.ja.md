RHarbor - Remote Desktop Management Tool
=====

RHarbor は **リモートデスクトップ接続の一括管理・サポートツール** です。

## 機能

RHarbor は下記のような機能を有しています。

- リモートデスクトップ接続情報 (RDP) の管理
    - 接続情報設定: ホスト, ポート, ユーザー名, 画面サイズ, フルスクリーン,  Admin モード
    - 接続に必要な SSH 接続の指定
    - グループ別管理
    - 名称/ホスト名での検索
    - 接続情報の複製
- SSH 接続情報の管理
    - プロパティ設定: ホスト, ポート, ユーザー名, パスワード/パスフレーズ, 秘密鍵, KeepAlive
    - 固定ポートフォワーディング設定
    - 接続に必要な SSH 接続の指定
    - グループ別管理
    - 名称/ホスト名での検索
    - 接続情報の複製
- ジャンプリスト機能
    - RDP/SSH への接続を Windows タスクバーのジャンプリストから開始

## 説明

RHarbor を使うと複数のリモートデスクトップの接続情報を一元管理できるだけでなく、ワンクリックでリモートデスクトップ接続が開始できます。

またリモートデスクトップの機能自体は Windows 標準の「リモートデスクトップ接続 (`mstsc.exe`)」 を利用しているため、操作感が変わることなく利用できます。

RHarbor はこんな方に便利です。

- 仕事で多くのリモートサーバーに RDP で接続する
- リモートデスクトップ先のマシンに接続するために複数の SSH を介さないといけない
- SSH のポートフォワーディングを手軽に行いたい

特に複数の SSH サーバーを経由した先の Windows マシンにリモートデスクトップに接続するときにとても役立ちます。

## 動作環境

- v1 系 (.NET Framework 向け)
    - Windows 7 以降
    - .NET Framework 4.6.1
- v2 系 (.NET Core 向け)
    - Windwos 7 SP1 以降
    - .NET Core 3.1

各バージョンに適した .NET ランタイムをインストールしてください。

- [Download .NET (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download)

## ダウンロードとインストール

[リリースページ](https://github.com/kenzauros/rharbor/releases) から zip ファイル (`RHarbor_vN.N.N.zip`) をダウンロードしてください。

インストール作業は特にありません。ダウンロードした zip ファイルを適当なフォルダーに展開し、 `RHarbor.exe` を起動してください。

アンインストールする場合は `RHarbor.exe` を展開したフォルダーを削除してください。

## アップデート

バージョンアップの際は zip ファイルに含まれるファイルをすべて元のインストールフォルダに上書きしてください。

## 使い方

使い方については詳細ページを参照してください。

1. [多段 SSH 経由のリモートデスクトップ接続](https://kenzauros.github.io/rharbor/rdp-with-multi-hop-ssh.ja.html)
1. [Windows のジャンプリストを使った接続](https://kenzauros.github.io/rharbor/jump-list.ja.html)

多段SSHでない場合のリモートデスクトップも 1 をご覧ください。

## 接続情報のデータについて

### 保存場所

`RHarbor.exe` と同じフォルダーに `RHarbor.db` があります。このファイルに接続情報が保存されています。

### 初期化

設定を初期化するには RHarbor を閉じた状態で、 `RHarbor.db` を削除してください。再度 RHarbor を起動すると `RHarbor.db` が再生成されます。

### バックアップ

設定を手動でバックアップする場合は `RHarbor.db` をコピーしてください。

なお、アプリ起動時にも自動でバックアップが行われています。このバックアップされたファイルはユーザーのプロファイルフォルダ (典型的には `C:\Users\<ユーザー名>\AppData\Roaming\RHarbor`) に保存されます。

## 注意

### SSH の鍵ファイルについて

鍵ファイルには `ssh-keygen` 等で生成できる OpenSSH 形式が利用できます。 (拡張子には依存しません)

PuTTy 形式の鍵ファイルは [puttygen.exe](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html) を利用して OpenSSH 形式に変換する必要があります。

### セキュリティについて

RHarbor に保存されるパスワードは簡易的な暗号化を施していますが、完全ではありません。

可能性は低いと思いますが、悪意のあるプログラムや人間により設定ファイル内やメモリ内のパスワードが読み取られる可能性があります。

使用されるコンピューターに適切なウイルス対策ソフト等がインストールされていることを確認の上、 RHarbor を共有のコンピューターにインストールすることは避け、自己責任でご利用ください。

## ライセンス

MIT

## Special Thanks to

- [Extended.Wpf.Toolkit](https://github.com/xceedsoftware/wpftoolkit)
- [Json.NET](https://www.newtonsoft.com/json)
- [NLog](https://nlog-project.org/)
- [ReactiveProperty](https://github.com/runceel/ReactiveProperty)
- [SSH.NET](https://github.com/sshnet/SSH.NET/)
- [System.Data.SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
- [System.Data.SQLite.EF6.Migrations](https://github.com/bubibubi/db2ef6migrations)

## 作者

- [kenzauros](https://github.com/kenzauros)
