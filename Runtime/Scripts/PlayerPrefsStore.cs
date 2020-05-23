using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Save
{

	public class PlayerPrefsStore : ISaveDataStore
	{
		Encrypter m_Encrypter;
		public PlayerPrefsStore(Encrypter encrypter)
		{
			m_Encrypter = encrypter;
		}

		string ToHash(string key)
		{
			return m_Encrypter.ToHash(key);
		}

		public void Delete(string key)
		{
			PlayerPrefs.DeleteKey(ToHash(key));
		}

		public void DeleteAll()
		{
			PlayerPrefs.DeleteAll();
		}

		public void Save()
		{
			PlayerPrefs.Save();
		}

		public bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(ToHash(key));
		}

		public void Set(string key, bool value)
		{
			PlayerPrefs.SetInt(ToHash(key), value ? 1 : 0);
		}

		public void Set(string key, int value)
		{
			PlayerPrefs.SetInt(ToHash(key), value);
		}

		public void Set(string key, float value)
		{
			PlayerPrefs.SetFloat(ToHash(key), value);
		}

		public void Set(string key, string value)
		{
			value = m_Encrypter.EncryptString(value);
			PlayerPrefs.SetString(ToHash(key), value);
		}

		public void Set(string key, byte[] value)
		{
			value = m_Encrypter.Encrypt(value);
			PlayerPrefs.SetString(ToHash(key), System.Convert.ToBase64String(value));
		}

		public bool GetBool(string key, bool def = false)
		{
			return PlayerPrefs.GetInt(ToHash(key), def ? 1 : 0) == 1;
		}

		public int GetInt(string key, int def = 0)
		{
			return PlayerPrefs.GetInt(ToHash(key), def);
		}

		public float GetFloat(string key, float def = 0)
		{
			return PlayerPrefs.GetFloat(ToHash(key), def);
		}

		public string GetString(string key, string def = "")
		{
			key = ToHash(key);
			if (PlayerPrefs.HasKey(key))
			{
				var str = PlayerPrefs.GetString(key);
				return m_Encrypter.DecryptString(str);
			}
			else
			{
				return def;
			}
		}

		public byte[] GetBytes(string key)
		{
			key = ToHash(key);
			if (PlayerPrefs.HasKey(key))
			{
				var str = PlayerPrefs.GetString(key);
				var data = System.Convert.FromBase64String(str);
				return m_Encrypter.Decrypt(data);
			}
			else
			{
				return null;
			}
		}
	}

}