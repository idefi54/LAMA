using System.Diagnostics;

namespace LAMA
{
    internal class DatabaseHolder<T, Storage> where T : Serializable, new() where Storage : Database.StorageInterface, new()
    {
        // Singleton
        private static DatabaseHolder<T, Storage> _instance;
        public static DatabaseHolder<T, Storage> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseHolder<T, Storage>();
                    Debug.WriteLine("setting instance");
                    Debug.WriteLine(_instance);
                }
                return _instance;
            }
        }

        // Object Specific
        private DatabaseHolder()
        {
            Debug.WriteLine("Database Holder");
            var sql = new OurSQL<T, Storage>();
            Debug.WriteLine("OurSQL finished");
            rememberedList = new RememberedList<T, Storage>(sql);
            Debug.WriteLine("Remembered list initialized");
        }

        public RememberedList<T, Storage> rememberedList;
    }

    internal class DatabaseHolderStringDictionary<T, Storage> where T : SerializableDictionaryItem, new() where Storage : Database.DictionaryStorageInterface, new()
    {
        // Singleton
        private static DatabaseHolderStringDictionary<T, Storage> _instance;
        public static DatabaseHolderStringDictionary<T, Storage> Instance
        {
            get
            {
                if (_instance == null) _instance = new DatabaseHolderStringDictionary<T, Storage>();
                return _instance;
            }
        }

        // Object Specific
        private DatabaseHolderStringDictionary()
        {
            var sql = new OurSQLDictionary<T, Storage>();
            rememberedDictionary = new RememberedStringDictionary<T, Storage>(sql);
        }

        public RememberedStringDictionary<T, Storage> rememberedDictionary;
    }
}
