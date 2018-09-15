RHarbor - Remote Desktop via SSH Servers
=====

RHarbor は **SSH を経由したリモートデスクトップ接続** のサポートツールです。

また、複数の SSH を経由する **多段 SSH 越しのポートフォワード機能** も備えています。

- [English](README.md)

## 説明

RHarbor を使うとワンクリックで、 **複数の SSH サーバーを経由した Windows 機へのリモートデスクトップ接続** が確立できます。

SSH サーバーを経由しないリモートデスクトップ接続も管理できます。

## 動作環境

- Windows 7 以降
- .NET Framework 4.6.1

## ダウンロードとインストール

[リリースページ](https://github.com/kenzauros/rharbor/releases) から zip ファイル (`RHarbor_vN.N.N.zip`) をダウンロードしてください。

インストール作業は特にありません。ダウンロードした zip ファイルを適当なフォルダーに展開し、 RHarbor.exe を起動してください。

## アップデート

バージョンアップの際は zip ファイルに含まれるファイルをすべて元のインストールフォルダに上書きしてください。

## 使い方

使い方については詳細ページを参照してください。

## 設定の初期化

RHarbor.exe と同じフォルダーに RHarbor.db があります。このファイルに設定が保存されています。
設定を初期化するには RHarbor を閉じた状態で、 RHarbor.db を削除してください。

再度 RHarbor を起動すると RHarbor.db が再生成されます。

## 注意

### SSH の鍵ファイルについて

鍵ファイルには `ssh-keygen` 等で生成できる OpenSSH 形式が利用できます。 (拡張子には依存しません)

PuTTy 形式の鍵ファイルは [puttygen.exe](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html) を利用して OpenSSH 形式に変換する必要があります。

### セキュリティについて

RHarbor に保存されるパスワードは簡易的な暗号化を施していますが、完全ではありません。

可能性は低いと思いますが、悪意のあるプログラムや人間により設定ファイル内やメモリ内のパスワードが読み取られる可能性があります。

使用されるコンピューターに適切なウイルス対策ソフト等がインストールされていることを確認の上、 RHarbor を共有のコンピューターにインストールすることは避け、自己責任でご利用ください。

## ライセンス

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

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
