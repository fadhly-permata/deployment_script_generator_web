using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Models.MongoDB;

/// <summary>
/// Represents base configuration for MongoDB collection joins
/// </summary>
/// <remarks>
/// Provides core properties needed for joining MongoDB collections using the $lookup aggregation stage.
///
/// Features:
/// - Collection joining with aliases
/// - Field projection support
/// - Filtering capabilities
/// - Nested join operations
///
/// > [!NOTE]
/// > All required properties must be set for successful join operations
///
/// > [!TIP]
/// > Use FieldNames to optimize query performance by limiting returned fields
///
/// > [!IMPORTANT]
/// > Ensure referenced collections exist in the database before performing joins
///
/// Example Request:
/// <code>
/// {
///   "collection": "users",
///   "as": "userData",
///   "fieldNames": ["name", "email", "phone"],
///   "filter": {
///     "status": "active",
///     "age": { "$gt": 18 }
///   },
///   "nextJoins": [{
///     "collection": "orders",
///     "as": "userOrders",
///     "localField": "_id",
///     "foreignField": "userId",
///     "fieldNames": ["orderId", "total"]
///   }]
/// }
/// </code>
///
/// Example Usage:
/// <example>
/// <code>
/// var joinConfig = new MongoDBJoin
/// {
///     Collection = "users",
///     As = "userData",
///     FieldNames = ["name", "email"],
///     Filter = new JObject { ["status"] = "active" },
///     NextJoins = new[] {
///         new MongoDBNextJoinItem {
///             Collection = "orders",
///             As = "userOrders",
///             LocalField = "_id",
///             ForeignField = "userId"
///         }
///     }
/// };
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="MongoDBNextJoinItem"/>
/// <seealso href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/">MongoDB $lookup Aggregation</seealso>
/// <seealso href="https://www.mongodb.com/docs/manual/core/aggregation-pipeline/">MongoDB Aggregation Pipeline</seealso>
public class MongoDBJoin
{
    /// <summary>
    /// Gets or sets the name of the collection to join with the current collection in MongoDB
    /// </summary>
    /// <remarks>
    /// Specifies the target collection name for the $lookup aggregation operation. The collection must exist
    /// in the MongoDB database.
    ///
    /// > [!IMPORTANT]
    /// > Collection name is case-sensitive and must match exactly with the collection name in MongoDB
    ///
    /// > [!NOTE]
    /// > Collection names cannot contain these characters: /\. "$
    ///
    /// > [!TIP]
    /// > Ensure the collection has appropriate indexes for optimal join performance
    ///
    /// Example:
    /// <example>
    /// <code>
    /// var join = new MongoDBJoin
    /// {
    ///     Collection = "users",
    ///     As = "userData",
    ///     FieldNames = ["name", "email"]
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>The MongoDB collection name as string</returns>
    /// <exception cref="ArgumentNullException">Thrown when collection name is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when collection name contains invalid characters</exception>
    /// <see href="https://www.mongodb.com/docs/manual/core/databases-and-collections/">MongoDB Collections</see>
    /// <see href="https://www.mongodb.com/docs/manual/reference/limits/#naming-restrictions">MongoDB Naming Restrictions</see>
    [Required(ErrorMessage = "Collection name is required.", AllowEmptyStrings = false)]
    public required string Collection { get; set; }

    /// <summary>
    /// Gets or sets the alias name for the joined collection in MongoDB $lookup operation
    /// </summary>
    /// <remarks>
    /// Defines the field name under which the joined documents will be stored in the output documents.
    /// This property is crucial for accessing the joined data in subsequent pipeline stages or final results.
    ///
    /// > [!IMPORTANT]
    /// > The alias name must be unique across all joins in the pipeline
    ///
    /// > [!NOTE]
    /// > The alias becomes a field name in the result document, so it must follow MongoDB field naming rules
    ///
    /// > [!TIP]
    /// > Choose descriptive alias names that reflect the relationship or data being joined
    ///
    /// Example:
    /// <example>
    /// <code>
    /// var joinConfig = new MongoDBJoin
    /// {
    ///     Collection = "orders",
    ///     As = "customerOrders",  // Results will be accessible as 'customerOrders' field
    ///     FieldNames = ["orderId", "total", "status"]
    /// };
    ///
    /// // Accessing joined data in results:
    /// // {
    /// //     "_id": "customerId123",
    /// //     "name": "John Doe",
    /// //     "customerOrders": [
    /// //         { "orderId": "ORD001", "total": 100, "status": "completed" },
    /// //         { "orderId": "ORD002", "total": 150, "status": "pending" }
    /// //     ]
    /// // }
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>The alias name as string that will be used to access joined documents</returns>
    /// <exception cref="ArgumentNullException">Thrown when alias name is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when alias name contains invalid characters</exception>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/#syntax">MongoDB $lookup Syntax</see>
    /// <see href="https://www.mongodb.com/docs/manual/reference/limits/#naming-restrictions">MongoDB Field Naming Rules</see>
    [Required(ErrorMessage = "Alias name is required.", AllowEmptyStrings = false)]
    public required string As { get; set; }

    /// <summary>
    /// Gets or sets the field names to include in results from MongoDB collection
    /// </summary>
    /// <remarks>
    /// Specifies which fields should be included in the projection for optimized query results.
    /// This helps reduce network bandwidth and memory usage by limiting the returned data.
    ///
    /// > [!NOTE]
    /// > If null or empty, all fields will be included in the results
    ///
    /// > [!TIP]
    /// > Use dot notation for accessing nested fields (e.g., "address.city")
    ///
    /// > [!IMPORTANT]
    /// > The _id field is always included unless explicitly excluded
    ///
    /// Example:
    /// <example>
    /// <code>
    /// // Basic field projection
    /// var join = new MongoDBJoin
    /// {
    ///     Collection = "customers",
    ///     As = "customerData",
    ///     FieldNames = ["name", "email", "phone"]
    /// };
    ///
    /// // With nested fields
    /// var joinWithNested = new MongoDBJoin
    /// {
    ///     Collection = "customers",
    ///     As = "customerData",
    ///     FieldNames = ["name", "contact.email", "address.city"]
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>Array of field names to include in the query results</returns>
    /// <see href="https://www.mongodb.com/docs/manual/tutorial/project-fields-from-query-results/">MongoDB Field Projection</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/document/#dot-notation">MongoDB Dot Notation</see>
    public string[]? FieldNames { get; set; }

    /// <summary>
    /// Gets or sets additional filter conditions for MongoDB query operations
    /// </summary>
    /// <remarks>
    /// Defines additional query criteria using a JSON object to filter documents in the collection.
    /// Supports all MongoDB query operators and complex filtering conditions.
    ///
    /// > [!NOTE]
    /// > If null, no additional filtering will be applied to the query
    ///
    /// > [!TIP]
    /// > Use MongoDB comparison operators ($gt, $lt, etc.) for range queries
    ///
    /// > [!IMPORTANT]
    /// > Ensure indexes exist for frequently filtered fields to optimize query performance
    ///
    /// Example:
    /// <example>
    /// <code>
    /// // Simple equality filter
    /// var simpleFilter = new MongoDBJoin
    /// {
    ///     Collection = "users",
    ///     As = "activeUsers",
    ///     Filter = new JObject { ["status"] = "active" }
    /// };
    ///
    /// // Complex filter with multiple conditions
    /// var complexFilter = new MongoDBJoin
    /// {
    ///     Collection = "products",
    ///     As = "availableProducts",
    ///     Filter = new JObject
    ///     {
    ///         ["price"] = new JObject { ["$gt"] = 100, ["$lt"] = 1000 },
    ///         ["stock"] = new JObject { ["$gt"] = 0 },
    ///         ["categories"] = new JObject { ["$in"] = new JArray { "electronics", "gadgets" } }
    ///     }
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>A <see cref="JObject"/> containing MongoDB query filter criteria</returns>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/query/">MongoDB Query Operators</see>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/query-comparison/">MongoDB Comparison Operators</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/index/">MongoDB Indexes</see>
    public JObject? Filter { get; set; }

    /// <summary>
    /// Gets or sets the subsequent join operations to perform in MongoDB aggregation pipeline
    /// </summary>
    /// <remarks>
    /// Configures nested document relationships for complex joins using MongoDB's $lookup aggregation stage.
    /// Supports multiple levels of joins with field projections and filtering.
    ///
    /// > [!NOTE]
    /// > Each join operation creates a new array field in the resulting documents
    ///
    /// > [!IMPORTANT]
    /// > Performance may degrade with multiple nested joins. Use indexes on join fields
    ///
    /// > [!TIP]
    /// > Limit the number of joins and projected fields to optimize query performance
    ///
    /// Example:
    /// <example>
    /// <code>
    /// var userOrdersJoin = new MongoDBJoin
    /// {
    ///     Collection = "users",
    ///     As = "userData",
    ///     NextJoins = new[]
    ///     {
    ///         new MongoDBNextJoinItem
    ///         {
    ///             Collection = "orders",
    ///             As = "userOrders",
    ///             LocalField = "_id",
    ///             ForeignField = "userId",
    ///             FieldNames = ["orderId", "total", "status"],
    ///             NextJoins = new[]
    ///             {
    ///                 new MongoDBNextJoinItem
    ///                 {
    ///                     Collection = "products",
    ///                     As = "orderProducts",
    ///                     LocalField = "productId",
    ///                     ForeignField = "_id",
    ///                     FieldNames = ["name", "price"]
    ///                 }
    ///             }
    ///         }
    ///     }
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>Array of <see cref="MongoDBNextJoinItem"/> configurations for nested document relationships</returns>
    /// <see cref="MongoDBNextJoinItem"/>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/">MongoDB $lookup</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/aggregation-pipeline-optimization/">MongoDB Aggregation Pipeline Optimization</see>
    [Inheritance(InheritanceLevel.NotInherited)]
    public MongoDBNextJoinItem[]? NextJoins { get; set; }
}

/// <summary>
/// Represents configuration for subsequent joins in MongoDB aggregation pipeline
/// </summary>
/// <remarks>
/// Extends <see cref="MongoDBJoin"/> with properties for specifying join conditions between collections.
/// Used to define relationships between documents in different collections for complex data retrieval.
///
/// > [!IMPORTANT]
/// > Both LocalField and ForeignField must reference existing fields in their respective collections
///
/// > [!NOTE]
/// > Join operations are performed using MongoDB's $lookup aggregation stage
///
/// > [!TIP]
/// > Create indexes on join fields to improve query performance
///
/// Example:
/// <example>
/// <code>
/// // Basic join between users and orders
/// var nextJoin = new MongoDBNextJoinItem
/// {
///     Collection = "orders",
///     As = "userOrders",
///     LocalField = "_id",
///     ForeignField = "userId",
///     FieldNames = ["orderId", "total"],
///     Filter = new JObject { ["status"] = "completed" }
/// };
///
/// // Example result document:
/// // {
/// //     "_id": "user123",
/// //     "name": "John Doe",
/// //     "userOrders": [
/// //         { "orderId": "ORD001", "total": 100 },
/// //         { "orderId": "ORD002", "total": 150 }
/// //     ]
/// // }
/// </code>
/// </example>
/// </remarks>
/// <returns>A configured join operation for MongoDB aggregation pipeline</returns>
/// <exception cref="ArgumentException">Thrown when LocalField or ForeignField contains invalid characters</exception>
/// <exception cref="ArgumentNullException">Thrown when required fields are null or empty</exception>
/// <see href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/">MongoDB $lookup</see>
/// <see href="https://www.mongodb.com/docs/manual/core/indexes/">MongoDB Indexes</see>
/// <see href="https://www.mongodb.com/docs/manual/tutorial/equality-match-joins-with-lookup/">MongoDB Join Examples</see>
public class MongoDBNextJoinItem : MongoDBJoin
{
    /// <summary>
    /// Gets or sets the field name from the input documents to be used in join operations
    /// </summary>
    /// <remarks>
    /// Specifies the field from the current collection to use in the join operation with another collection.
    /// This field will be matched against the ForeignField in the target collection.
    ///
    /// > [!IMPORTANT]
    /// > The field must exist in the source collection and should be indexed for optimal performance
    ///
    /// > [!NOTE]
    /// > Commonly used with document _id field or other unique identifiers
    ///
    /// > [!TIP]
    /// > Use compound indexes when joining on multiple fields
    ///
    /// Example:
    /// <example>
    /// <code>
    /// // Simple join using _id field
    /// var userOrdersJoin = new MongoDBNextJoinItem
    /// {
    ///     Collection = "orders",
    ///     As = "userOrders",
    ///     LocalField = "_id",           // Field from users collection
    ///     ForeignField = "userId",      // Matching field in orders collection
    ///     FieldNames = ["orderId", "total"]
    /// };
    ///
    /// // Join using custom field
    /// var productCategoryJoin = new MongoDBNextJoinItem
    /// {
    ///     Collection = "categories",
    ///     As = "productCategory",
    ///     LocalField = "categoryId",    // Custom field from products collection
    ///     ForeignField = "_id",         // Matching field in categories collection
    ///     FieldNames = ["name", "description"]
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>The field name as string to be used in the join operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when field name is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when field name contains invalid characters</exception>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/#lookup-syntax">MongoDB $lookup Syntax</see>
    /// <see href="https://www.mongodb.com/docs/manual/indexes/#indexes">MongoDB Indexes</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/index-compound/">MongoDB Compound Indexes</see>
    [Required(ErrorMessage = "Local field name is required.", AllowEmptyStrings = false)]
    public required string LocalField { get; set; }

    /// <summary>
    /// Gets or sets the field name from the documents of the joined collection to match with LocalField
    /// </summary>
    /// <remarks>
    /// Specifies the field from the target collection that will be matched against the LocalField during join operations.
    /// This field is used in MongoDB's $lookup stage to establish relationships between collections.
    ///
    /// > [!IMPORTANT]
    /// > The field must exist in the target collection and should be indexed for optimal performance
    ///
    /// > [!NOTE]
    /// > Commonly used with document _id field or foreign key fields
    ///
    /// > [!TIP]
    /// > Create appropriate indexes on frequently used foreign fields to improve join performance
    ///
    /// Example:
    /// <example>
    /// <code>
    /// // Basic join configuration
    /// var orderDetailsJoin = new MongoDBNextJoinItem
    /// {
    ///     Collection = "orderDetails",
    ///     As = "details",
    ///     LocalField = "orderId",
    ///     ForeignField = "_id",         // References the _id field in orderDetails collection
    ///     FieldNames = ["items", "total"]
    /// };
    ///
    /// // Complex join with custom fields
    /// var userProfileJoin = new MongoDBNextJoinItem
    /// {
    ///     Collection = "profiles",
    ///     As = "userProfile",
    ///     LocalField = "profileReference",
    ///     ForeignField = "referenceId",  // Custom field in profiles collection
    ///     FieldNames = ["bio", "avatar"]
    /// };
    /// </code>
    /// </example>
    /// </remarks>
    /// <returns>The field name as string to match against LocalField in join operations</returns>
    /// <exception cref="ArgumentNullException">Thrown when field name is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when field name contains invalid characters</exception>
    /// <see href="https://www.mongodb.com/docs/manual/reference/operator/aggregation/lookup/">MongoDB $lookup</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/indexes/">MongoDB Indexes</see>
    /// <see href="https://www.mongodb.com/docs/manual/core/index-compound/">MongoDB Compound Indexes</see>
    [Required(ErrorMessage = "Foreign field name is required.", AllowEmptyStrings = false)]
    public required string ForeignField { get; set; }
}
