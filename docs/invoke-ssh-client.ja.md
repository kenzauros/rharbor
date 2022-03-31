外部 SSH クライアントの呼び出し
=====

- [RHarbor](index.ja.md) トップページに戻る

## 概要

RHarbor では **保存した SSH の接続情報を利用して Tera Term Pro や RLogin などの SSH クライアントを呼び出す** ことができます。

## 外部プログラムの設定

### 設定の追加

[設定] → [外部プログラム] の <img src="images/buttons/add-button.png" alt="＋" style="height:2ex"> ボタンをクリックして、テンプレートを選択します。リストに希望のプログラムがない場合は「自分で入力」をクリックして編集ください。

<img src="images\invoke-ssh-client\settings.ja.png" alt="外部プログラムの設定" width="480">

Tera Term などいくつかのテンプレートでは実行ファイルパスやコマンドライン引数が自動で設定されます。
**実行ファイルパスやコマンドライン引数は手元の環境に合わせて適宜設定してください。**

**コマンドライン引数では `{host}`, `{username}` などのパラメーターが利用できます。**
それぞれ SSH の接続情報で定義したデータで置き換えられます。詳細は「コマンドライン引数について」の項を参照してください。

設定できたら、設定画面右下の <img src="images/buttons/save-button.png" alt="設定の保存" style="height:2ex"> ボタンをクリックして、設定を保存します。

### 標準のテンプレート

標準のテンプレートとして下記の SSH クライアントのものを用意しています。

- OpenSSH (クリップボードに SSH コマンドをコピー, ※パスワード非対応)
- PuTTY
- Tera Term Pro
- RLogin

これ以外のものは適宜カスタマイズしてご利用ください。

### コマンドライン引数について

**コマンドライン引数には `{KEY}` の形式で記述することでパラメーターを埋め込むことができます。**

下記のパラメーターは標準で利用できます。それぞれ SSH 接続情報で設定した値が使用されます。

キー | 埋め込まれる値
-- | --
`{host}` | ホスト
`{port}` | ポート
`{username}` | ユーザー名
`{password}` | パスワード/パスフレーズ
`{keyfile}` | 鍵ファイルのパス

これに加えて、 SSH 接続情報の **「追加パラメーター」** にキーと値を指定することで、その値も埋め込むことができます。

また、 **条件演算子（三項演算子） (`条件 ? 成立時の値 : 非成立時の値`)** が使用できますので、パラメーターの値に応じて、埋め込む値を変化させることができます。
条件演算子の「条件」部分には下記のような式が使用できます。最初の被演算子 (下表の KEY) はパラメーターのキーとして評価されます。右辺の値 (下表の A) はすべて文字列として評価されます。

式 | 解釈
-- | --
`KEY=A` | キー `KEY` の値と `A` が等しい場合に成立
`KEY!=A` | キー `KEY` の値と `A` が等しくない場合に成立
`KEY` | キー `KEY` が設定されていて、その値が空以外の文字列の場合に成立
`!KEY` | キー `KEY` が設定されていないか、その値が空文字列の場合に成立

#### コマンドライン引数の例

たとえば下記のような SSH 接続情報があるとします。

接続情報 | 値
-- | --
ホスト名 | `my-host`
鍵ファイル | `C:\my-host.key`
追加パラメーター `stage` | `staging`

このとき、コマンドライン引数で「設定例」のように設定すると「結果」のような値がプログラムの引数として渡されます。

設定例 | 結果 | 説明
-- | -- | --
`{stage}` | `staging` | 追加パラメーター `stage` に設定した内容に置き換わる
`{stage=staging?blue:red}` | `blue` | `stage` の値が `staging` なので `blue` に置き換わる
`{stage?hoge:fuga}` | `hoge` | `stage` が指定されているので `hoge` に置き換わる
`{!stage?{host}:{host}-{stage}}` | `my-host-staging` | `stage` が指定されているので `{host}-{stage}` が評価され `my-host-staging` に置き換わる
`/auth={keyfile?publickey:password}` | `/auth=publickey` | 鍵ファイルが指定されているので `publickey` に置き換わる


### 設定の削除

不要な設定は <img src="images/buttons/remove-button.png" alt="削除" style="height:2ex"> ボタンで削除した後、設定を保存することで削除できます。

## 設定した外部プログラムの起動

SSH 接続情報で <img src="images/buttons/ssh-client-button.png" alt="SSH クライアントの起動" style="height:2ex"> を押すと設定した外部プログラムの一覧が表示されます。

<img src="images\invoke-ssh-client\invoke-ssh-client.ja.png" alt="設定した外部プログラムの起動" width="480">

**希望のプログラムを選択すると設定した実行ファイルが接続情報とともに呼び出されます。**

外部プログラム設定で「クリップボードにコピー」にチェックが入っている場合はプログラムは起動されず、起動用のコマンド文字列がクリップボードにコピーされます。

## ヒント

### PuTTY の鍵ファイルについて

**RHarbor で利用できる SSH の鍵ファイルは OpenSSH 形式であるため、 [PuTTY](https://www.putty.org/) でそのまま利用することはできません。**

ここでは RHarbor の外部プログラム呼び出し機能を使って PuTTY で接続する方法をご紹介します。

まず、鍵ファイルは [PuTTYgen](https://www.puttygen.com/) (PuTTY に同梱) を利用して PuTTY 形式に変換しておきます。
また、`<OpenSSH の鍵ファイル名>.ppk` にして同じフォルダに配置します。

たとえば OpenSSH 形式の鍵ファイル名が `key.pem` であれば `key.pem.ppk` のようになります。

これでコマンドライン引数の設定で `putty -ssh -i "{keyfile}.ppk"` のように指定すれば PuTTY 形式の鍵ファイルを渡すことができます。


