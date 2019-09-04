using System;

namespace ILib.Save
{
	/// <summary>
	/// Dataクラスを保存する際の型を指定する属性です
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class SaveKeyAttribute : Attribute
	{
		public static string ToKey(object key)
		{
			if (key is string)
			{
				return key.ToString();
			}
			else
			{
				//_∩(@_@)彡
				return key.GetType().FullName + "@_@" + key.ToString();
			}
		}

		public Type KeyRawType { get; private set; }
		public string Key { get; private set; }
		public SaveKeyAttribute(object key)
		{
			KeyRawType = key.GetType();
			Key = ToKey(key);
		}
	}

}
