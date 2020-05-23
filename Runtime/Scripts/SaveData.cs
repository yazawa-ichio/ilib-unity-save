using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Save
{
	public interface ISaveData
	{
		void Save();
		void DeleteKeys();
		void Set(Data value);
		U Get<U>() where U : Data, new();
		bool TryGet<U>(out U data) where U : Data, new();
	}

	/// <summary>
	/// セーブデータを保存するためのクラスです。
	/// TにはEnumを指定してください。
	/// </summary>
	public class SaveData<T> : ISaveData where T : struct, System.IConvertible
	{
		ISaveDataStore m_Store;
		System.Func<string> m_KeyPrefix;

		public SaveData(ISaveDataStore store, System.Func<string> keyPrefix)
		{
			m_Store = store;
			m_KeyPrefix = keyPrefix;
		}

		/// <summary>
		/// PlayerPrefsに対してキー難読化し文字列を暗号化して保存する方式で作成します。
		/// </summary>
		public SaveData(string iv, string key, System.Func<string> keyPrefix)
		{
			m_Store = new PlayerPrefsStore(new Encrypter(iv, key));
			m_KeyPrefix = keyPrefix;
		}

		string GetSaveKey(T key)
		{
			return m_KeyPrefix() + SaveKeyAttribute.ToKey(key);
		}

		public void Set(T key, bool value)
		{
			m_Store.Set(GetSaveKey(key), value);
		}

		public void Set(T key, int value)
		{
			m_Store.Set(GetSaveKey(key), value);
		}

		public void Set(T key, float value)
		{
			m_Store.Set(GetSaveKey(key), value);
		}

		public void Set(T key, string value)
		{
			m_Store.Set(GetSaveKey(key), value);
		}

		public void Set(T key, byte[] value)
		{
			m_Store.Set(GetSaveKey(key), value);
		}

		public void Set(Data value)
		{
			var key = m_KeyPrefix() + Data.ToSaveKey(value.GetType(), typeof(T));
			m_Store.Set(key, JsonUtility.ToJson(value));
		}

		public bool GetBool(T key, bool def = false)
		{
			return m_Store.GetBool(GetSaveKey(key), def);
		}

		public int GetInt(T key, int def = 0)
		{
			return m_Store.GetInt(GetSaveKey(key), def);
		}

		public float GetFloat(T key, float def = 0)
		{
			return m_Store.GetFloat(GetSaveKey(key), def);
		}

		public string GetString(T key, string def = "")
		{
			return m_Store.GetString(GetSaveKey(key), def);
		}

		public byte[] GetBytes(T key)
		{
			return m_Store.GetBytes(GetSaveKey(key));
		}

		/// <summary>
		/// データを取得します。
		/// 読み取りに失敗した場合は新しく生成します。
		/// </summary>
		public bool TryGet<U>(out U data) where U : Data, new()
		{
			try
			{
				data = Get<U>();
				return true;
			}
			catch (System.Exception ex)
			{
				data = new U();
				data.Init(this);
				Debug.LogException(ex);
				return false;
			}
		}

		/// <summary>
		/// データを取得します
		/// </summary>
		public U Get<U>() where U : Data, new()
		{
			var key = m_KeyPrefix() + Data.ToSaveKey(typeof(U), typeof(T));
			var json = m_Store.GetString(key);
			if (string.IsNullOrEmpty(json))
			{
				var data = new U();
				data.Init(this);
				return data;
			}
			else
			{
				var data = JsonUtility.FromJson<U>(json);
				data.Load(this);
				return data;
			}
		}

		/// <summary>
		/// 指定のキーを削除します。
		/// </summary>
		public void Delete(T key)
		{
			m_Store.Delete(GetSaveKey(key));
		}

		/// <summary>
		/// データを永続化します。
		/// </summary>
		public void Save()
		{
			m_Store.Save();
		}

		/// <summary>
		/// EnumのKeyをすべて削除します。
		/// </summary>
		public void DeleteKeys()
		{
			foreach (var key in System.Enum.GetValues(typeof(T)))
			{
				var keyStr = m_KeyPrefix() + SaveKeyAttribute.ToKey(key);
				m_Store.Delete(keyStr);
			}
		}

	}
}