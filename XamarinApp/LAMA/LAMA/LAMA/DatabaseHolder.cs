

namespace LAMA
{
    internal class DatabaseHolder<T> where T : Serializable, new()
    {
        // Singleton
        private static DatabaseHolder<T> _instance;
        public static DatabaseHolder<T> Instance
        {
            get
            {
                if (_instance == null) _instance = new DatabaseHolder<T>();
                return _instance;
            }
        }

        // Object Specific
        private DatabaseHolder()
        {
            var sql = new OurSQL<T>(".\\database.db");
            rememberedList = new RememberedList<T>(sql);
        }

        public RememberedList<T> rememberedList;
    }

    internal class DatabaseHolderStringDictionary<T> where T : SerializableDictionaryItem, new()
    {
        // Singleton
        private static DatabaseHolderStringDictionary<T> _instance;
        public static DatabaseHolderStringDictionary<T> Instance
        {
            get
            {
                if (_instance == null) _instance = new DatabaseHolderStringDictionary<T>();
                return _instance;
            }
        }

        // Object Specific
        private DatabaseHolderStringDictionary()
        {
            var sql = new OurSQLDictionary<T>(".\\database.db");
            rememberedDictionary = new RememberedStringDictionary<T>(sql);
        }

        public RememberedStringDictionary<T> rememberedDictionary;
    }
}
