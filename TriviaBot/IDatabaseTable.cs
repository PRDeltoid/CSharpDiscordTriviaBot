using System.Collections;

namespace TriviaBot
{
    public interface IDatabaseTable<T, K> where T : new()
    {
        string TableName { get; }

        bool AddRow(T newRow);
        bool ChangeValue(string propName, object value);
        IEnumerator GetEnumerator();
        T GetRow(K id);
        bool UpdateRow(T newRow, K oldRowId);
    }
}