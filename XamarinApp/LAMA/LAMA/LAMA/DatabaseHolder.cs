

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
            var sql = new OurSQL<T>("database.db");
            rememberedList = new RememberedList<T>(sql);
        }

        public RememberedList<T> rememberedList;
    }
}
