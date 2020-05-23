using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ILib.Save
{
	public class Encrypter
	{
		byte[] m_IV;
		byte[] m_Key;
		CipherMode m_Mode;
		PaddingMode m_Padding;
		Func<string, string> m_HashProvider;

		public Encrypter(string iv, string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, Func<string, string> hashProvider = null)
		{
			m_IV = Convert.FromBase64String(iv);
			m_Key = Convert.FromBase64String(key);
			m_Mode = mode;
			m_Padding = padding;
			m_HashProvider = hashProvider ?? CreateDefaultHashProvider(iv + key + mode.ToString() + padding.ToString());
		}

		public string EncryptString(string text)
		{
			var data = Encrypt(Encoding.UTF8.GetBytes(text));
			return Convert.ToBase64String(data);
		}

		public byte[] Encrypt(byte[] data)
		{
			using (var rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = m_Key;
				rijAlg.IV = m_IV;
				rijAlg.Mode = m_Mode;
				rijAlg.Padding = m_Padding;
				using (var encryptor = rijAlg.CreateEncryptor())
				{
					return encryptor.TransformFinalBlock(data, 0, data.Length);
				}
			}
		}

		public string DecryptString(string text)
		{
			var data = Convert.FromBase64String(text);
			return Encoding.UTF8.GetString(Decrypt(data));
		}

		public byte[] Decrypt(byte[] data)
		{
			using (var rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = m_Key;
				rijAlg.IV = m_IV;
				rijAlg.Mode = m_Mode;
				rijAlg.Padding = m_Padding;
				using (var decryptor = rijAlg.CreateDecryptor())
				{
					return decryptor.TransformFinalBlock(data, 0, data.Length);
				}
			}
		}

		public string ToHash(string text)
		{
			return m_HashProvider(text);
		}

		Func<string, string> CreateDefaultHashProvider(string salt)
		{
			var sb = new StringBuilder();
			string hashSalt = "";
			var md5 = new MD5CryptoServiceProvider();
			Func<string, string> provider = (text) =>
			{
				text = hashSalt + text;
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
				sb.Clear();
				for (int i = 0; i < hash.Length; i++)
				{
					sb.Append($"{hash[i]:x2}");
				}
				return sb.ToString();
			};
			hashSalt = provider(salt);
			return provider;
		}


	}
}