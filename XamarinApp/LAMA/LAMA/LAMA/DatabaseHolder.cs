

namespace LAMA
{
    internal class DatabaseHolder<T, Storage> where T : Serializable, new() where Storage : database.StorageInterface, new()
    {
        // Singleton
        private static DatabaseHolder<T, Storage> _instance;
        public static DatabaseHolder<T, Storage> Instance
        {
            get
            {
                if (_instance == null) _instance = new DatabaseHolder<T, Storage>();
                return _instance;
            }
        }

        // Object Specific
        private DatabaseHolder()
        {
            var sql = new OurSQL<T, Storage>(".\\database.db");
            rememberedList = new RememberedList<T, Storage>(sql);
        }

        public RememberedList<T, Storage> rememberedList;
    }

    internal class DatabaseHolderStringDictionary<T, Storage> where T : SerializableDictionaryItem, new() where Storage : database.StorageInterface, new()
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
            var sql = new OurSQLDictionary<T, Storage>(".\\database.db");
            rememberedDictionary = new RememberedStringDictionary<T, Storage>(sql);
        }

        public RememberedStringDictionary<T, Storage> rememberedDictionary;
    }
}
