# [lib-unity-save](https://github.com/yazawa-ichio/ilib-unity-save)

Unity Save Data Package

リポジトリ https://github.com/yazawa-ichio/ilib-unity-save

## 概要

Unity用のデータ保存のパッケージです。  
簡単な暗号化や難読化を行えます。  
アプリケーション毎に作った方が速いぐらい簡易な実装です。

## セットアップ方法

### 保存用のキーと作成

セーブ用のキーを列挙型で作成してください。  
キーは文字列で解決します。  
そのため、上下の入れ替えが可能ですが、名前が変更された場合に読み取りが出来なくなります。  
*リリース後は、名前の変更は避けるようにしてください。*

```csharp
//セーブ用のキー
//ToString()を行った文字列で解決します。
public enum AppSaveKey
{
	UserId,
	Config,
	PlayData,
}
```

保存キーは`Ilib.Save.SaveData<T>`のジェネリックとして指定します。  
以下の例では継承して利用していますが、`SaveData<AppSaveKey>`のインスタンスをそのまま使用することも可能です。

```csharp
//パッケージの名前空間
using ILib.Save;

public class AppSaveData : SaveData<AppSaveKey>
{
	public AppSaveData(string iv, string key, System.Func<string> keyPrefix) : base(iv, key, keyPrefix) {
	}
}
```

### 暗号化用の鍵の作成

事前にアプリケーションで利用する暗号化用の情報を作成します。  
暗号化のため`System.Security.Cryptography.RijndaelManaged`を利用しているためIVとKeyが必要になります。  

IVとKeyの作り方が分からない場合、以下のようなスクリプトで作成できます。  
ただし、`ソルト生成`と`ILibSave`の部分は、必ず適当な置き換えてください。

例: 暗号化用のキーを作成
```csharp
using UnityEditor;
class Tool
{
	[MenuItem("Tool/ILib/GenSave IV and Key")]
	static void GenIVAndKey()
	{
		byte[] salt = System.Text.Encoding.UTF8.GetBytes("ソルト生成");
		//ソルトとパスワードからKeyとIVを作成。
		var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes("ILbSave", salt);
		var key = System.Convert.ToBase64String(deriveBytes.GetBytes(keySize / 8));
		var iv = System.Convert.ToBase64String(deriveBytes.GetBytes(blockSize / 8));
		Debug.Log($"key:{key},iv:{iv}");
	}
}
```

ここでは説明しませんが、生成した`Key`と`IV`は可能であれはbyte配列等にして、さらにXORなどを利用し難読化した型で組み込んでください。  
殆どのデータがサーバー側で保存されている、コンフィグ等のデータしかない場合は、ハードコードしてもそれ程問題はありません。

### インスタンスの作成

後はKeyとIVを`SaveData<AppSaveKey>`の作成時に渡したり、継承したクラスでラップするなどして、インスタンスを作成します。

例: `SaveData<T>`をそのまま使う
```csharp
//パッケージの名前空間
using ILib.Save;

var key = "hogehoge";
var iv = "hogehoge";
var data = new SaveData<AppSaveKey>(key,iv, keyPrefix: () => "");

```

例: `SaveData<T>`をラップして使う
```csharp
//パッケージの名前空間
using ILib.Save;

// クラスでラップする
public class AppSaveData : SaveData<AppSaveKey>
{
	//可能であれば難読化した物をハードコードしておき、実行時に復号化してデータを返す
	static string GetKey() => "hogehoge";
	static string GetIV() => "hogehoge";

	public AppSaveData(System.Func<string> keyPrefix) : base(GetKey(), GetIV(), keyPrefix) {
	}
}

var data = new AppSaveData(keyPrefix: () => "");
```

### keyPrefixについて
keyPrefixは環境ごとに保存したい場合が違う場合に使用します。  
例えば、複数のセーブデータを持てるタイプのゲームであれば、そのスロット毎に別の `KeyPrefix` を変えれば、同じキーでスロット毎に保存ができます。  
また、サーバーを利用するゲームであれば、接続先ごとに保存するデータを変更するなども可能です。  

## クラスをシリアライズして保存する

### 保存用のクラスを作成する

`Ilib.Save.Data`を継承したクラスに、`ILib.Save.SaveKeyAttribute`属性を設定します。  
Unityの`JsonUtility`クラスを通して保存するため、Unityのシリアライズの制約に縛られます。  

```csharp
//パッケージの名前空間
using ILib.Save;
using UnityEngine;

[SaveData(AppSaveKey.Config)]
public class Config : Data
{
	public float SoundVolume = 1f;
}
```

### データを取得・保存する

データを取り出す場合は`SaveData<TKey>.TryGet<TData>(out TData data)`を使用します。  
保存する場合は、`ILib.Save.Data.SetDitry`クラスを利用します。  内部的には、`SaveData<T>.Set`関数を実行した後に`SaveData<T>.Save`関数を実行しています。

```csharp
SaveData<AppSaveKey> data = GetData(); 
Config config;
//<Config>は省略可
var ret = data.TryGet<Config>(out config);
Debug.Assert(ret, "デシリアライズに失敗した場合ret=falseが返る。新規作成時とロード成功時はret=trueが返る");

//保存する
//saveがtrueの場合はPlayerPrefへの永続化も行います。
data.SetDitry(save: true);

```



## LICENSE

https://github.com/yazawa-ichio/ilib-unity-save/blob/master/LICENSE