using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Models.MongoDB;

/// <summary>
/// Request model for updating specific fields in MongoDB documents.
/// </summary>
/// <remarks>
/// This model enables partial updates of MongoDB documents by specifying a filter to match documents
/// and an update object containing only the fields that need to be modified.
///
/// Example request body:
/// <code>
/// {
///   "filter": {
///     "email": "john@example.com",
///     "status": "active"
///   },
///   "updates": {
///     "lastLoginDate": "2024-01-01",
///     "loginCount": 42,
///     "profile": {
///       "address": {
///         "city": "New York"
///       }
///     }
///   }
/// }
/// </code>
///
/// > [!NOTE]
/// > The update operation only modifies the specified fields while preserving other existing fields.
///
/// > [!IMPORTANT]
/// > The filter must contain valid MongoDB query operators and match the collection's schema.
///
/// > [!TIP]
/// > Use dot notation in updates object to modify nested fields (e.g., "profile.address.city").
/// </remarks>
/// <seealso cref="JObject"/>
/// <seealso href="https://www.mongodb.com/docs/manual/tutorial/update-documents/">MongoDB Update Operations</seealso>
/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">Newtonsoft.Json.Linq.JObject</seealso>
public class MongoUpdateFieldsRequest
{
    /// <summary>
    /// Filter criteria to match documents for update
    /// </summary>
    /// <remarks>
    /// Specifies the conditions that documents must meet to be updated. Supports all MongoDB query operators.
    ///
    /// Example:
    /// <code>
    /// {
    ///   "age": { "$gt": 18 },
    ///   "status": { "$in": ["active", "pending"] },
    ///   "email": "user@example.com",
    ///   "profile.verified": true
    /// }
    /// </code>
    ///
    /// > [!IMPORTANT]
    /// > Ensure indexes exist for frequently used filter fields to optimize query performance
    ///
    /// > [!TIP]
    /// > Use dot notation for querying nested document fields
    /// </remarks>
    /// <seealso href="https://www.mongodb.com/docs/manual/reference/operator/query/">MongoDB Query Operators</seealso>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Class</seealso>
    public required JObject Filter { get; set; }

    /// <summary>
    /// Object containing field paths and their new values for updating MongoDB documents
    /// </summary>
    /// <remarks>
    /// Specifies the fields to be updated and their new values. Supports nested document updates using dot notation.
    ///
    /// Example:
    /// <code>
    /// {
    ///   "lastLoginDate": "2024-01-01",
    ///   "loginCount": 42,
    ///   "profile.address.city": "New York",
    ///   "tags": ["active", "verified"],
    ///   "metadata.lastModified": "2024-01-15T10:30:00Z"
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Only specified fields will be updated; existing fields not included in the updates object remain unchanged
    ///
    /// > [!IMPORTANT]
    /// > Field names must match the collection schema and follow MongoDB naming conventions
    ///
    /// > [!TIP]
    /// > Use dot notation (e.g., "profile.address.city") to update nested fields without modifying the entire object
    /// </remarks>
    /// <seealso href="https://www.mongodb.com/docs/manual/tutorial/update-documents/">MongoDB Update Operations</seealso>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Class</seealso>
    public required JObject Updates { get; set; }
}
