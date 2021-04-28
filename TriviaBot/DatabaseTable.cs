using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TriviaBot.Properties;

namespace TriviaBot
{
    public class DatabaseTable<T, K> : IEnumerable where T : new()
    {
        string TableName { get; set; }

        public DatabaseTable(string tableName)
        {
            TableName = tableName;
        }

        private string GetKeyColumnName()
        {
            Type t = typeof(T).GetProperties().
                            Where(prop => prop.IsDefined(typeof(KeyColumn), false)).First().GetType();

            if (t == null)
            {
                throw new Exception("No key attribute set"); ;
            }

            if (t != typeof(K))
            {
                throw new Exception("Key value type is not the same as marked key attribute.");
            }

            // If we get here, there is a key property and its type matches the passed K type. Return it to the caller
            return t.Name;
            /*foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                var attr = prop.GetCustomAttribute(typeof(KeyColumn));
                if (attr != null)
                {
                    //Make sure the property marked as a key is the same type as the passed Key parameter (so our GetRowByID function works correctly)
                    if (prop.GetType() != typeof(K))
                    {
                        throw new Exception("Key value type is not the same as marked key attribute.");
                    }

                    // If we get here, there is a key property and its type matches the passed K type. Return it to the caller
                    return prop.Name;
                }
            }

            if we get here, we went through every prop and none were set as a key.
             There must be a key, so throw an error
            throw new Exception("No key attribute set");*/
        }

        private string GetColumnNameOfProperty(PropertyInfo propInfo)
        {
            // If the prop has the ColumnName attribute, return that instead
            var col = propInfo.GetCustomAttribute(typeof(ColumnName)) as ColumnName;
            if (col != null)
            {
                return col.Name;
            }
            else
            {
                return propInfo.Name;
            }
        }

        public bool AddRow(T newRow)
        {
            //Column name/Value pairs to be used for our new row
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                string propColumnName = GetColumnNameOfProperty(prop);
                values.Add(propColumnName, newRow.GetType().GetProperty(prop.Name).GetValue(newRow, null));
            }

            using (SqlConnection connection = new SqlConnection(Settings.Default.ConnectionString))
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
                SqlCommand command = new SqlCommand(queryString, connection);
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

        public T GetRow(K id)
        {
            using (SqlConnection connection = new SqlConnection(Settings.Default.ConnectionString))
            {
                string keyName = GetKeyColumnName();
                string queryString = $"SELECT * WHERE {keyName} = {id}";
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(queryString, connection);
                    var rows = command.ExecuteReader();
                    if (rows.HasRows)
                    {
                        var temp = new T();
                        rows.NextResult();
                        foreach (PropertyInfo prop in typeof(T).GetProperties())
                        {
                            temp.GetType().GetProperty(prop.Name).SetValue(rows[GetColumnNameOfProperty(prop)].GetType(), rows[GetColumnNameOfProperty(prop)]);
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
        }

        public bool ChangeValue(string propName, object value)
        {
            throw new NotImplementedException();
        }

        public bool UpdateRow(T newRow, K oldRowId)
        {
            // Get all cols including key (in case that is being updated)
            var cols = GetAllColumnNames(true);
            var values = new List<string>();
            // Get the key name
            var keyCol = GetKeyColumnName();

            // Compose the UPDATE query string
            string queryString = $"UPDATE {TableName} SET ";
            foreach(string col in cols)
            {
                var value = newRow.GetType().GetProperty(col).GetValue(newRow).ToString();
                if (value != null)
                {
                    queryString += $"{col} = {value},";
                    values.Add(value);
                }
            }
            queryString = queryString.Substring(0, queryString.Length);
            queryString += $"WHERE {keyCol} = {oldRowId}";

            // Open a connection and execute the query string
            using (SqlConnection connection = new SqlConnection(Settings.Default.ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.ExecuteScalar();
                    return true;
                }
                catch
                {
                    throw;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            string queryString = $"SELECT * FROM {TableName}";
            // Open a connection and execute the query string
            using (SqlConnection connection = new SqlConnection(Settings.Default.ConnectionString))
            {
                try
                {
                    List<T> objects = new List<T>();
                    connection.Open();
                    SqlCommand command = new SqlCommand(queryString, connection);
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        var obj = new T();
                        foreach(string prop in GetAllColumnNames(true))
                        {
                            obj.GetType().GetProperty(prop).SetValue(obj, reader[prop]);
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
        }

        private List<string> GetAllColumnNames(bool includeKey)
        {
            List<string> stringList = new List<string>();
            var keyColumn = GetKeyColumnName();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
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
