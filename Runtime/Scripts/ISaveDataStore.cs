namespace ILib.Save
{
	public interface ISaveDataStore
	{
		bool HasKey(string key);
		void Set(string key, bool value);
		void Set(string key, int value);
		void Set(string key, float value);
		void Set(string key, string value);
		void Set(string key, byte[] value);

		bool GetBool(string key, bool def = false);
		int GetInt(string key, int def = 0);
		float GetFloat(string key, float def = 0);
		string GetString(string key, string def = "");
		byte[] GetBytes(string key);

		void Delete(string key);
		void DeleteAll();
		void Save();
	}
}
