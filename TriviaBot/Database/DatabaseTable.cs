using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace TriviaBot
{
    public class DatabaseTable<TRowDataClass, TKeyType> : IEnumerable where TRowDataClass : new()
    {
        public string TableName { get; internal set; }

        private static string ConnectionString =>
            System.Configuration.ConfigurationManager.
                ConnectionStrings["TriviaBotDb"].ConnectionString;

        public DatabaseTable(string tableName)
        {
            TableName = tableName;
            CreateTableIfNotExists();
        }

        public bool AddRow(TRowDataClass newRow)
        {
            //Column name/Value pairs to be used for our new row
            Dictionary<string, object> values = new();

            foreach (string propName in GetAllColumnNames(true))
            {
                values.Add(propName, GetPropertyValue(newRow, propName));
            }

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                string colNames = "";
                string colValues = "";
                foreach (string colName in values.Keys)
                {
                    colNames += "," + colName;
                    colValues += "," + values[colName];
                }
                // remove leading comma
                colValues = colValues.Substring(1);
                colNames = colNames.Substring(1);

                string queryString = $"INSERT INTO { TableName } ({colNames}) VALUES ({colValues})";
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();
                try
                {
                    int rows = command.ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
            }
            return true;
        }

        public TRowDataClass GetRow(TKeyType id)
        {
            using SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            string keyName = GetKeyColumnName();
            string queryString = $"SELECT * FROM {TableName} WHERE {keyName} = {id}";
            try
            {
                connection.Open();
                using SQLiteCommand command = new SQLiteCommand(queryString, connection);
                using SQLiteDataReader rows = command.ExecuteReader();
                if (rows.HasRows)
                {
                    rows.Read();
                    var temp = new TRowDataClass();
                    //rows.NextResult();
                    foreach (string propName in GetAllColumnNames(true))
                    {
                        SetPropertyValue(temp, rows[propName], propName);
                    }
                    return temp;
                }
                else
                {
                    return default;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool UpdateRow(TRowDataClass newRow, TKeyType oldRowId)
        {
            // Get all cols excluding key
            var cols = GetAllColumnNames(false);
            var values = new List<string>();
            // Get the key name
            var keyCol = GetKeyColumnName();

            // Compose the UPDATE query string
            string queryString = $"UPDATE {TableName} SET ";
            foreach(string col in cols)
            {
                string colValue = GetPropertyValue(newRow, col).ToString();
                if (colValue != null)
                {
                    queryString += $"{col} = {colValue},";
                    values.Add(colValue);
                }
            }
            queryString = queryString.Substring(0, queryString.Length-1);
            queryString += $" WHERE {keyCol} = {oldRowId};";

            // Open a connection and execute the query string
            using SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            try
            {
                connection.Open();
                using SQLiteCommand command = new SQLiteCommand(queryString, connection);
                int rowsAffected = command.ExecuteNonQuery();
                // Only return true if at least one row was updated
                return rowsAffected > 0 ? true : false;
            }
            catch
            {
                throw;
            }
        }

        public IEnumerator GetEnumerator()
        {
            string queryString = $"SELECT * FROM {TableName}";
            // Open a connection and execute the query string
            using SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            try
            {
                List<TRowDataClass> objects = new List<TRowDataClass>();
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                using SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    TRowDataClass obj = new TRowDataClass();
                    foreach (string prop in GetAllColumnNames(true))
                    {
                        SetPropertyValue(obj, reader[prop], prop);
                    }
                    objects.Add(obj);
                }
                return objects.GetEnumerator();
            }
            catch
            {
                throw;
            }
        }

        public List<string> GetAllColumnNames(bool includeKey)
        {
            List<string> stringList = new List<string>();
            var keyColumn = GetKeyColumnName();
            foreach (PropertyInfo prop in typeof(TRowDataClass).GetProperties())
            {
                string propColumnName = GetColumnNameOfProperty(prop);
                // If the property is a key column and we want to exclude those, continue next loop iteration
                if(propColumnName == keyColumn && includeKey == false)
                {
                    continue;
                }
                stringList.Add(propColumnName);
            }
            return stringList;
        }

        private void CreateTableIfNotExists()
        {
            var colNames = GetAllColumnNames(false);
            var createTableString = $"CREATE TABLE IF NOT EXISTS {TableName} (";
            var keyCol = GetKeyColumnName();
            var keyType = GetSQLType(GetPropertyType(keyCol));
            createTableString += $"{keyCol} {keyType} PRIMARY KEY,";
            foreach (string col in colNames)
            {
                string type = GetSQLType(GetPropertyType(col));
                createTableString += $"{col} {type},";
            }
            createTableString = createTableString.Substring(0, createTableString.Length - 1);
            createTableString += ");";

            using SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            SQLiteCommand command = new SQLiteCommand(createTableString, connection);
            connection.Open();
            try
            {
                int rows = command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }

        private static string GetSQLType(Type t)
        {
            switch (t)
            {
                case Type intType when intType == typeof(int):
                case Type uintType when uintType == typeof(uint):
                case Type ulongType when ulongType == typeof(ulong):
                case Type longType when longType == typeof(long):
                    return "INTEGER";
                case Type stringType when stringType == typeof(string):
                    return "TEXT";
                default:
                    throw new Exception("Unknown SQL type encountered");
            }
        }

        private static string GetKeyColumnName()
        {
            PropertyInfo t = typeof(TRowDataClass).
                GetProperties().First(prop => Attribute.IsDefined(prop, typeof(KeyColumn), false));

            if (t == null)
            {
                throw new Exception("No key attribute set"); ;
            }

            if (t.PropertyType != typeof(TKeyType))
            {
                throw new Exception("Key value type is not the same as marked key attribute.");
            }

            // If we get here, there is a key property and its type matches the passed K type. Return it to the caller
            return t.Name;
        }

        private static string GetColumnNameOfProperty(PropertyInfo propInfo)
        {
            // If the prop has the ColumnName attribute, return that instead
            if (propInfo.GetCustomAttribute(typeof(ColumnName)) is ColumnName col)
            {
                return col.Name;
            }

            return propInfo.Name;
        }

        private static PropertyInfo GetProperty(object t, string propertyName)
        {
            return t.GetType().GetProperty(propertyName);
        }

        private static Type GetPropertyType(object t, string propertyName)
        {
            return t.GetType().GetProperty(propertyName)?.PropertyType;
        }

        private static Type GetPropertyType(string propertyName)
        {
            TRowDataClass t = new TRowDataClass();
            return t.GetType().GetProperty(propertyName)?.PropertyType;
        }

        private object GetPropertyValue(object t, string propertyName)
        {
            Type type = GetPropertyType(t, propertyName);
            return Convert.ChangeType(GetProperty(t, propertyName).GetValue(t), type);
        }

        private void SetPropertyValue(object t, object value, string propertyName)
        {
            Type type = GetPropertyType(t, propertyName);
            GetProperty(t, propertyName).SetValue(t, Convert.ChangeType(value, type));
        }
    }

    #region Attribute Definitions
    public class ColumnName : System.Attribute
    {
        public string Name { get; }

        public ColumnName(string name)
        {
            Name = name;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class KeyColumn : System.Attribute
    {
        public KeyColumn()
        {

        }
    }
    #endregion


}
