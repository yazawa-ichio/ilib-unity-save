using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Save
{
	/// <summary>
	/// 保存するデータを持つクラスです。
	/// [SaveKey(Enum.Type)]の用に保存用のKeyを設定してください。
	/// バージョンを埋め込み差分を検知できます。
	/// 継承して利用してください。
	/// </summary>
	[System.Serializable]
	public abstract class Data : ISerializationCallbackReceiver
	{
		/// <summary>
		/// データのバージョンです。
		/// 互換性のない変更があった際に上げることで古いバージョンのイベントを受け取れます。
		/// </summary>
		protected virtual int Version { get { return 1; } }

		[SerializeField]
		private int m_Version;

		ISaveData m_Parent;

		internal void Init(ISaveData parent)
		{
			m_Parent = parent;
			OnInit();
		}

		internal void Load(ISaveData parent)
		{
			m_Parent = parent;
		}

		/// <summary>
		/// セーブデータになく初回に生成された際に実行されます
		/// </summary>
		protected virtual void OnInit() { }

		/// <summary>
		/// 保存されたバージョンと現在のバージョンに呼び出されます。
		/// </summary>
		/// <param name="oldVersion"></param>
		protected virtual void OnChangeVersion(int oldVersion) { }

		/// <summary>
		/// デシリアライズ直後に実行されます
		/// </summary>
		protected virtual void OnDeserialize() { }

		/// <summary>
		/// シリアライズ直前に実行されます
		/// </summary>
		protected virtual void OnSerialize() { }

		/// <summary>
		/// 親へデータの更新を行います。
		/// saveフラグを有効にすると永続化します。
		/// </summary>
		public void SetDitry(bool save = false)
		{
			m_Parent.Set(this);
			if (save)
			{
				m_Parent.Save();
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			bool changed = m_Version != Version;
			if (m_Version != Version)
			{
				OnChangeVersion(m_Version);
				m_Version = Version;
			}
			OnDeserialize();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Version = Version;
			OnSerialize();
		}

		internal static string ToSaveKey(System.Type dataType, System.Type keyType)
		{
			//アトリビュートをキャッシュする？
			var attributes = dataType.GetCustomAttributes(typeof(SaveKeyAttribute), false);
			foreach (var attr in attributes)
			{
				var key = attr as SaveKeyAttribute;
				if (key.KeyRawType == keyType)
				{
					return key.Key;
				}
			}
			if (attributes.Length == 0)
			{
				throw new System.InvalidOperationException("not found SaveKeyAttribute.");
			}
			else
			{
				throw new System.ArgumentException("not found KeyType.", nameof(keyType));
			}
		}

	}
}
