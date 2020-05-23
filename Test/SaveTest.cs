using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

using ILib.Save;

public class SaveTest
{
	public enum SaveKey
	{
		Bool,
		String,
		Int,
		Float,
		Bytes,
		Data,
	}

	class Context : System.IDisposable
	{
		public SaveData<SaveKey> Save { get; private set; }

		public Context(string keyPrefix, int keySize = 256, int blockSize = 128)
		{
			TestData.TestVersion = 1;

			//keyとivはネイティブ側やランタイムで動的生成を行い
			//解析されずらくしてください。

			byte[] salt = System.Text.Encoding.UTF8.GetBytes("ソルト生成");
			var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes("ILbSave", salt);
			var key = System.Convert.ToBase64String(deriveBytes.GetBytes(keySize / 8));
			var iv = System.Convert.ToBase64String(deriveBytes.GetBytes(blockSize / 8));
			Debug.Log($"key:{key},iv:{iv}");
			Save = new SaveData<SaveKey>(iv, key, () => keyPrefix);
		}

		public void Dispose()
		{
			Save.DeleteKeys();
		}
	}

	[SaveKey(SaveKey.Data)]
	class TestData : Data
	{
		public static int TestVersion;

		protected override int Version => TestVersion;

		public bool InitCheck { get; private set; }

		public int OldVersion { get; private set; }

		public string TestString;

		protected override void OnInit()
		{
			InitCheck = true;
		}

		protected override void OnChangeVersion(int oldVersion)
		{
			OldVersion = oldVersion;
		}

	}



	[Test]
	public void Test1()
	{
		//単純な単一パラメータの保存
		using (var ctx = new Context("Test1"))
		{
			Assert.IsFalse(ctx.Save.GetBool(SaveKey.Bool));
			ctx.Save.Set(SaveKey.Bool, true);
			Assert.IsTrue(ctx.Save.GetBool(SaveKey.Bool));

			Assert.AreEqual(ctx.Save.GetString(SaveKey.String, "DefString"), "DefString");
			ctx.Save.Set(SaveKey.String, "TestTest");
			Assert.AreEqual(ctx.Save.GetString(SaveKey.String), "TestTest");

			Assert.AreEqual(ctx.Save.GetInt(SaveKey.Int, 5), 5);
			ctx.Save.Set(SaveKey.Int, 22);
			Assert.AreEqual(ctx.Save.GetInt(SaveKey.Int), 22);

			Assert.AreEqual(ctx.Save.GetFloat(SaveKey.Float, 5.3f), 5.3f);
			ctx.Save.Set(SaveKey.Float, 22.3f);
			Assert.AreEqual(ctx.Save.GetFloat(SaveKey.Float), 22.3f);

			Assert.IsNull(ctx.Save.GetBytes(SaveKey.Bytes));
			var bytes = System.Text.Encoding.UTF8.GetBytes("TTTTTest");
			ctx.Save.Set(SaveKey.Bytes, bytes);
			var getBytes = ctx.Save.GetBytes(SaveKey.Bytes);
			for (int i = 0; i < bytes.Length; i++)
			{
				Assert.AreEqual(bytes[i], getBytes[i]);
			}

			ctx.Save.Save();

			ctx.Save.Delete(SaveKey.Bool);
			Assert.IsFalse(ctx.Save.GetBool(SaveKey.Bool));
			ctx.Save.Set(SaveKey.Bool, true);
			Assert.IsTrue(ctx.Save.GetBool(SaveKey.Bool));

			ctx.Save.DeleteKeys();
			Assert.IsFalse(ctx.Save.GetBool(SaveKey.Bool));
			Assert.IsTrue(string.IsNullOrEmpty(ctx.Save.GetString(SaveKey.String)));
			Assert.AreEqual(ctx.Save.GetInt(SaveKey.Int), 0);
			Assert.AreEqual(ctx.Save.GetFloat(SaveKey.Float), 0);

		}
	}

	[Test]
	public void Test2()
	{
		//データクラスのセーブ
		using (var ctx = new Context("Test2"))
		{
			TestData data = null;
			for (int i = 0; i < 2; i++)
			{
				ctx.Save.TryGet(out data);
				Assert.IsTrue(string.IsNullOrEmpty(data.TestString));
				Assert.IsTrue(data.InitCheck);
				Assert.AreEqual(data.OldVersion, 0);
				data.TestString = "Test";
			}
			data.SetDitry();

			ctx.Save.TryGet(out data);
			Assert.IsFalse(string.IsNullOrEmpty(data.TestString));
			Assert.IsFalse(data.InitCheck);
			Assert.AreEqual(data.OldVersion, 0);
			data.SetDitry(true);

			TestData.TestVersion = 2;
			ctx.Save.TryGet(out data);
			Assert.AreEqual(data.TestString, "Test");
			Assert.IsFalse(data.InitCheck);
			Assert.AreEqual(data.OldVersion, 1);

			data.TestString = "TestTest";

			data.SetDitry(true);
			ctx.Save.TryGet(out data);
			Assert.AreEqual(data.TestString, "TestTest");
			Assert.IsFalse(data.InitCheck);
			Assert.AreEqual(data.OldVersion, 0);

		}
	}

	[Test]
	public void Test3()
	{
		//キーを変えれば共存できる
		using (var ctx4 = new Context("Test4"))
		using (var ctx5 = new Context("Test5"))
		{
			ctx4.Save.Set(SaveKey.Bool, true);
			ctx4.Save.Save();
			Assert.IsFalse(ctx5.Save.GetBool(SaveKey.Bool));
		}
	}
}