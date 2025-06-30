using IDC.Utilities.Data;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Data;

/// <summary>
/// Provides SQLite database operations for rule evaluation
/// </summary>
/// <remarks>
/// Handles in-memory SQLite operations for evaluating rule conditions against table data.
/// Creates temporary tables, inserts data, and evaluates conditions using SQLite's query engine.
/// </remarks>
/// <param name="sqliteHelper">Helper for SQLite database operations</param>
/// <seealso cref="SQLiteHelper"/>
/// <seealso href="https://www.sqlite.org/index.html">SQLite Documentation</seealso>
public class SqLite(SQLiteHelper sqliteHelper)
{
    internal object? RuleQuestionEvaluator(
        string appNo,
        JObject ttableData,
        string fieldToEval,
        string fieldCondition
    )
    {
        ttableData.Remove("_id");
        fieldCondition = fieldCondition.Replace("\\n", " ").Replace("  ", " ").Trim();

        var ttableFields = string.Join(
            separator: ", ",
            values: ttableData.Properties().Select(p => p.Name)
        );

        var ttableFieldsAndType = string.Join(
            separator: ", ",
            values: ttableData
                .Properties()
                .Select(p => $"{p.Name} {GetSQLiteDataType(p.Value.Type)}")
        );
        var ttableValues = string.Join(
            separator: ", ",
            values: ttableData
                .Properties()
                .Select(p =>
                {
                    return
                        GetSQLiteDataType(p.Value.Type) == "TEXT"
                        || GetSQLiteDataType(p.Value.Type) == "DATETIME"
                        ? (JToken)$"\"{p.Value}\""
                        : p.Value;
                })
        );

        sqliteHelper
            .Connect()
            .ExecuteNonQuery(
                query: $@"CREATE TABLE IF NOT EXISTS {appNo} ({ttableFieldsAndType});",
                affectedRows: out var affectedRows
            )
            .ExecuteNonQuery(
                query: $@"
                INSERT INTO 
                    {appNo} 
                    ({ttableFields})
                VALUES 
                    ({ttableValues});
            ",
                affectedRows: out affectedRows
            )
            .ExecuteScalar(
                query: $@"SELECT ({fieldCondition}) as {fieldToEval} FROM {appNo}",
                result: out var result
            );

        return result;
    }

    static string GetSQLiteDataType(JTokenType type)
    {
        return type switch
        {
            JTokenType.Boolean => "INTEGER",
            JTokenType.Date => "DATETIME",
            JTokenType.Float => "REAL",
            JTokenType.Guid => "TEXT",
            JTokenType.Integer => "INTEGER",
            JTokenType.Null => "NULL",
            JTokenType.Object => "TEXT",
            JTokenType.String => "TEXT",
            JTokenType.TimeSpan => "DATETIME",
            JTokenType.Uri => "TEXT",
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(type),
                actualValue: type,
                message: null
            ),
        };
    }
}
