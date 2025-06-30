using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Models.MongoDB;

/// <summary>
/// Represents configuration for MongoDB TTable join operations
/// </summary>
/// <remarks>
/// Used to specify field paths and filtering criteria for joining MongoDB collections in a TTable format.
///
/// > [!IMPORTANT]
/// > Ensure proper field paths exist in collections to prevent runtime errors
///
/// > [!TIP]
/// > Use dot notation for nested fields, e.g. "address.city"
///
/// > [!NOTE]
/// > Filter uses standard MongoDB query operators ($eq, $gt, $lt, etc.)
///
/// Example request body:
/// <code>
/// {
///   "fieldPath": [
///     "users.name",
///     "users.email",
///     "orders.orderNumber",
///     "orders.total",
///     "products.title",
///     "products.price"
///   ],
///   "filter": {
///     "status": "active",
///     "createdDate": { "$gt": "2024-01-01T00:00:00Z" },
///     "total": { "$gte": 100 },
///     "$or": [
///       { "category": "electronics" },
///       { "category": "books" }
///     ]
///   }
/// }
/// </code>
///
/// Example usage in code:
/// <example>
/// <code>
/// var joinConfig = new MongoDBTTableJoin
/// {
///     FieldPath = new[] {
///         "users.name",
///         "orders.total",
///         "products.title"
///     },
///     Filter = new JObject
///     {
///         ["status"] = "active",
///         ["createdDate"] = new JObject {
///             ["$gt"] = DateTime.Now.AddDays(-30)
///         }
///     }
/// };
/// </code>
/// </example>
/// </remarks>
/// <seealso href="https://www.mongodb.com/docs/manual/reference/operator/query/">MongoDB Query Operators</seealso>
/// <seealso href="https://www.mongodb.com/docs/manual/tutorial/project-fields-from-query-results/">MongoDB Field Projection</seealso>
public class MongoDBTTableJoin
{
    /// <summary>
    /// Gets or sets the array of field paths to include in the join operation
    /// </summary>
    /// <remarks>
    /// Each path should be in the format "collectionName.fieldName".
    /// Supports nested fields using dot notation.
    ///
    /// > [!WARNING]
    /// > Field paths must exist in collections to avoid runtime errors
    ///
    /// Example paths:
    /// <code>
    /// [
    ///   "users.profile.name",
    ///   "orders.details.total",
    ///   "products.category.name"
    /// ]
    /// </code>
    /// </remarks>
    /// <seealso href="https://www.mongodb.com/docs/manual/core/document/#dot-notation"/>
    public required string[] FieldPath { get; set; }

    /// <summary>
    /// Gets or sets the filter criteria for the join operation
    /// </summary>
    /// <remarks>
    /// Uses MongoDB query syntax for filtering documents.
    /// Supports all standard MongoDB query operators.
    ///
    /// > [!TIP]
    /// > Use compound filters with $and, $or for complex queries
    ///
    /// Example filters:
    /// <code>
    /// {
    ///   "age": { "$gte": 18 },
    ///   "status": "active",
    ///   "$or": [
    ///     { "role": "admin" },
    ///     { "role": "manager" }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    /// <seealso href="https://www.mongodb.com/docs/manual/reference/operator/query/"/>
    public required JObject Filter { get; set; }
}
