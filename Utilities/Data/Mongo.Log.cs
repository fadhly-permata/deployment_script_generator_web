using IDC.Utilities.Extensions;
using ScriptDeployerWeb.Utilities.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Data;

internal partial class Mongo
{
    internal async Task<ObjectId> InsertProcessLog(
        BsonDocument workflowConfig,
        string appNo,
        string tTable,
        string userId,
        string sourceId,
        string edgeId,
        BsonDocument? data = null,
        string? notes = null,
        string? processStatus = "process",
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var wfConfigJson = workflowConfig.AsJObject();
            notes = string.IsNullOrWhiteSpace(notes) ? "" : notes;

            BsonDocument document = new()
            {
                { "pol_app_id", appNo },
                { "pol_ttable", tTable },
                { "pol_usr", userId },
                { "pol_source_id", sourceId },
                { "pol_action_date", DateTime.UtcNow },
                { "pol_status", processStatus },
                { "pol_edges_id", edgeId },
                { "pol_data", data ?? [] },
                {
                    "pol_wf_id",
                    wfConfigJson!.PropGet<string>(
                        path: "header.flows_id",
                        throwOnNull: true,
                        onNullMessage: "Workflow ID not found."
                    )
                },
                {
                    "pol_notes",
                    sourceId.Equals(
                        value: "start",
                        comparisonType: StringComparison.OrdinalIgnoreCase
                    )
                        ? wfConfigJson!.PropGet<string>(
                            path: "header.version",
                            throwOnNull: true,
                            onNullMessage: "Workflow version not found."
                        )
                        : notes
                },
                { "processing_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
            };

            await mongoDB
                .GetCollection<BsonDocument>(WF_PROCESS_LOG)
                .InsertOneAsync(
                    document: document,
                    options: new InsertOneOptions(),
                    cancellationToken: cancellationToken
                );

            return document["_id"].AsObjectId;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return ObjectId.Empty;
        }
    }

    internal async Task<ObjectId?> InsertProcessLog(
        string workflowId,
        string appId,
        string tTable,
        string usr,
        string sourceId,
        string edgeId,
        string? notes,
        string? processStatus = "process",
        BsonDocument? data = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var workflowConfig = await GetWFById(
                workflowId: workflowId,
                cancellationToken: cancellationToken
            );

            return await InsertProcessLog(
                workflowConfig: workflowConfig!,
                appNo: appId,
                tTable: tTable,
                userId: usr,
                sourceId: sourceId,
                edgeId: edgeId,
                notes: notes,
                processStatus: processStatus,
                data: data,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<ObjectId?> UpdateProcessLogs(
        ObjectId logId,
        string userId,
        string? processStatus = "process",
        BsonDocument? data = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await mongoDB
                .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
                .UpdateOneAsync(
                    filter: Builders<BsonDocument>.Filter.Eq(field: "_id", value: logId),
                    update: Builders<BsonDocument>
                        .Update.Set(field: "pol_status", value: processStatus)
                        .Set(field: "pol_usr", value: userId)
                        .Set(field: "pol_finish_date", value: DateTime.UtcNow)
                        .Set(field: "pol_data", value: data ?? [])
                        .Set(
                            field: "processing_time",
                            value: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        ),
                    cancellationToken: cancellationToken
                );

            return logId;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return ObjectId.Empty;
        }
    }

    internal async Task<ObjectId?> RemoveProcessLog(
        ObjectId logId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await mongoDB
                .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
                .DeleteOneAsync(
                    filter: Builders<BsonDocument>.Filter.Eq(field: "_id", value: logId),
                    cancellationToken: cancellationToken
                );

            return logId;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return ObjectId.Empty;
        }
    }

    internal async Task<string> UpdateRshPic(
        BsonDocument workflowConfig,
        string appNo,
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        await mongoDB
            .GetCollection<BsonDocument>(name: PUB_RULE_SET_HISTORY_PIC)
            .UpdateOneAsync(
                filter: Builders<BsonDocument>.Filter.And(
                    [
                        Builders<BsonDocument>.Filter.Eq(field: "rhp_rsh_id", value: appNo),
                        Builders<BsonDocument>.Filter.Eq(field: "rhp_flow_edges", value: sourceId),
                    ]
                ),
                update: Builders<BsonDocument>
                    .Update.Set(field: "rhp_status", value: "Close")
                    .Set(field: "rhp_updated_date", value: DateTime.UtcNow)
                    .Set(
                        field: "processing_time",
                        value: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    ),
                cancellationToken: cancellationToken
            );

        await InsertProcessLogPIC(
            rshId: appNo,
            sourceId: sourceId,
            cancellationToken: cancellationToken
        );

        await UpdateRuleSetHistoryDetail(
            workflowConfig: workflowConfig,
            tTable: "",
            cancellationToken: cancellationToken
        );

        return appNo;
    }

    internal async Task<BsonDocument> UpdateProcessLogData(
        ObjectId logId,
        BsonDocument data,
        CancellationToken cancellationToken = default
    ) =>
        await mongoDB
            .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
            .FindOneAndUpdateAsync(
                filter: Builders<BsonDocument>.Filter.Eq(field: "pol_id", value: logId),
                update: Builders<BsonDocument>
                    .Update.Set(field: "pol_data", value: data)
                    .Set(
                        field: "processing_time",
                        value: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    ),
                options: new FindOneAndUpdateOptions<BsonDocument>
                {
                    ReturnDocument = ReturnDocument.After,
                },
                cancellationToken: cancellationToken
            );

    // TODO: Implement the method to update user process logs
    internal async Task<JArray?> UpdateUserProcessLogs(string logId, string userId)
    {
        await Task.CompletedTask;
        return [];
    }

    // TODO: Complete this method
    internal async Task InsertProcessLogPIC(
        string rshId,
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            @"InsertProcessLogPIC can not be executed, because depending on ""workflow.rpt_process_log_pic"""
        );
        await Task.CompletedTask;
    }
}
