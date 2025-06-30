using IDC.Utilities;
using IDC.Utilities.Extensions;
using ScriptDeployerWeb.Utilities.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Data;

internal partial class Mongo(IMongoDatabase mongoDB, Language language, SystemLogging systemLogging)
{
    #region TTable Data Processor
    internal async Task<BsonDocument?> TTableGet(
        string appNo,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return (
                    await mongoDB
                        .GetCollection<BsonDocument>(name: "ttable")
                        .Find(
                            Builders<BsonDocument>.Filter.And(
                                filters: Builders<BsonDocument>.Filter.Eq(
                                    field: "app_no",
                                    value: appNo
                                )
                            )
                        )
                        .FirstOrDefaultAsync(cancellationToken)
                ) ?? throw new Exception("TTable not found.");
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> TTableGetByAppId(
        string appNo,
        string[]? fields = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appNo))
                throw new ArgumentNullException(paramName: nameof(appNo));

            return await mongoDB
                    .GetCollection<BsonDocument>(name: "ttable")
                    .Find(Builders<BsonDocument>.Filter.Eq(field: "app_no", value: appNo))
                    .Project(
                        fields == null
                            ? []
                            : new BsonDocument(fields.Select(f => new BsonElement(f, 1)))
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                ?? throw new Exception("TTable not found.");
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<string?> TTableUpsert(
        JObject data,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            data = data.UpdateProcessTime();

            var bsonData = BsonDocument.Parse(data.ToString() ?? "{}");

            var result = await mongoDB
                .GetCollection<BsonDocument>(name: "ttable")
                .ReplaceOneAsync(
                    filter: Builders<BsonDocument>.Filter.Eq(
                        field: "app_no",
                        value: bsonData["app_no"]
                    ),
                    replacement: bsonData,
                    options: new ReplaceOptions { IsUpsert = true },
                    cancellationToken: cancellationToken
                );

            return bsonData["app_no"].AsString;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<JObject?> TTableUpdateFromDFStages(
        JObject ttable,
        JObject dataDF,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var stages = dataDF.PropGet<JArray>(path: "data.stages", defaultValue: []);
            if (stages is null || stages.Count == 0)
                return ttable;

            var forMerge = new JObject().UpdateProcessTime();

            foreach (var stage in stages)
            {
                var results = (stage as JObject)?.PropGet<JArray>(path: "result", defaultValue: []);
                if (results is null || results.Count == 0)
                    continue;

                foreach (var result in results)
                {
                    var obj = result as JObject;
                    var fname = obj?.PropGet(path: "field_name", defaultValue: string.Empty);
                    var fvalue = obj?.PropGet<object>(path: "field_value", defaultValue: null);

                    if (!string.IsNullOrWhiteSpace(value: fname))
                        forMerge = forMerge.PropUpsert(path: fname, value: fvalue);
                }
            }

            ttable = ttable.PropMerge(other: forMerge, mergeArrays: false);

            _ = Task.Run(
                async () =>
                {
                    await TTableUpsert(data: ttable, cancellationToken: cancellationToken);
                },
                cancellationToken
            );

            await Task.CompletedTask;
            return ttable;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<JObject?> TTableUpdateFromDFTempResult(
        JObject ttable,
        JObject dataDF,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var tempResult = dataDF.PropGet<JObject>(path: "data.temp_results", defaultValue: []);
            if (tempResult is null || tempResult.Count == 0)
                return ttable;

            var forMerge = new JObject().UpdateProcessTime();

            tempResult = new JObject(
                tempResult
                    .Properties()
                    .Where(p => p.Value != null && !string.IsNullOrEmpty(p.Value.ToString()))
            );

            foreach (var item in tempResult)
                forMerge = forMerge.PropUpsert(path: item.Key, value: item.Value);

            ttable = ttable.PropMerge(other: forMerge, mergeArrays: false);

            _ = Task.Run(
                async () =>
                {
                    await TTableUpsert(data: ttable, cancellationToken: cancellationToken);
                },
                cancellationToken
            );

            await Task.CompletedTask;
            return ttable;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
        return null;
    }
    #endregion TTable Data Processor

    #region Workflow configurations Processor
    internal async Task<BsonDocument?> GetWFById(
        string workflowId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await mongoDB
                    .GetCollection<BsonDocument>(name: WF_CONFIG)
                    .Find(
                        Builders<BsonDocument>.Filter.And(
                            filters: Builders<BsonDocument>.Filter.Eq(
                                field: "flows_code",
                                value: workflowId.StartsWith('W') ? workflowId : $"W{workflowId}"
                            )
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new Exception("Workflow configuration not found.");
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> GetWFById(
        long workflowId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await GetWFById(
                workflowId: $"W{workflowId}",
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> FindEdgeBySourceAndLabel(
        BsonDocument workflowConfig,
        string? sourceId,
        string? label
    )
    {
        try
        {
            await Task.CompletedTask;

            return workflowConfig["edges"]
                .AsBsonArray.FirstOrDefault(predicate: edge =>
                    (
                        edge["source"].AsString == (sourceId ?? "start")
                        && (label is null || edge["data"]["label"].AsString == label)
                    )
                )
                ?.AsBsonDocument;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> FindEdgeBySource(
        BsonDocument workflowConfig,
        string? sourceId
    )
    {
        try
        {
            await Task.CompletedTask;

            return workflowConfig["edges"]
                .AsBsonArray.FirstOrDefault(predicate: edge => edge["source"].AsString == sourceId)
                ?.AsBsonDocument;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> FindEdgeByTarget(
        BsonDocument workflowConfig,
        string? targetId,
        string? label
    )
    {
        try
        {
            await Task.CompletedTask;

            return workflowConfig["edges"]
                .AsBsonArray.FirstOrDefault(predicate: edge =>
                    edge["target"].AsString == targetId
                    && (label is null || edge["data"]["label"].AsString == label)
                )
                ?.AsBsonDocument;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> FindNodeById(BsonDocument workflowConfig, string? nodeId)
    {
        try
        {
            await Task.CompletedTask;

            return workflowConfig["nodes"]
                .AsBsonArray.FirstOrDefault(predicate: node =>
                    node["id"]
                        .AsString.Equals(
                            value: nodeId,
                            comparisonType: StringComparison.CurrentCultureIgnoreCase
                        )
                )
                ?.AsBsonDocument;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }
    #endregion Workflow configurations Processor

    internal async Task<BsonDocument?> GetLastProcessDataByAppNo(
        string appNo,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return (
                await mongoDB
                    .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
                    .Find(
                        Builders<BsonDocument>.Filter.And(
                            filters: Builders<BsonDocument>.Filter.Eq(
                                field: "pol_app_id",
                                value: appNo
                            )
                        )
                    )
                    .Sort(Builders<BsonDocument>.Sort.Descending(field: "processing_time"))
                    .FirstOrDefaultAsync(cancellationToken)
            );
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    // TODO: Complete this method
    internal async Task UpdateRuleSetHistoryDetail(
        BsonDocument workflowConfig,
        string tTable,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            @"UpdateRuleSetHistoryDetail can not be executed, because depending on ""workflow.workflow_log_history"""
        );
        await Task.CompletedTask;
    }

    internal async Task<JObject?> CheckDependencies(
        string sourceId,
        string appId,
        string wfId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var wfcd = await WFCheckDependencies(
                sourceId: sourceId,
                appId: appId,
                wfId: wfId,
                cancellationToken: cancellationToken
            );

            if (wfcd is null)
                return null;

            return new JObject
            {
                { "app_id", appId },
                { "wf_id", wfId },
                { "edge_id", wfcd["pol_edge_id"].AsString },
                { "status", wfcd["pol_status"].AsString },
                {
                    "dependecies",
                    wfcd["pol_status"]
                        .AsString.Equals(
                            value: "finished",
                            comparisonType: StringComparison.CurrentCultureIgnoreCase
                        )
                },
            };
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> WFCheckDependencies(
        string sourceId,
        string appId,
        string wfId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return (BsonDocument?)
                await mongoDB
                    .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
                    .Find(
                        filter: Builders<BsonDocument>.Filter.And(
                            [
                                Builders<BsonDocument>.Filter.Eq(field: "pol_app_id", value: appId),
                                Builders<BsonDocument>.Filter.Eq(
                                    field: "pol_wf_id",
                                    value: int.Parse(wfId)
                                ),
                                Builders<BsonDocument>.Filter.Regex(
                                    field: "pol_source_id",
                                    regex: new BsonRegularExpression(
                                        pattern: $"^{sourceId}$",
                                        options: "i"
                                    )
                                ),
                            ]
                        )
                    )
                    .Sort(sort: Builders<BsonDocument>.Sort.Descending(field: "pol_id"))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    // TODO: Complete this method
    internal async Task<JObject?> UpsertWLH(
        string tblName,
        string rshId,
        string wfCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> GetActionNextProcess(
        string workflowId,
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var wfConfig =
                await GetWFById(workflowId: workflowId, cancellationToken: cancellationToken)
                ?? throw new Exception("Workflow configuration not found.");

            return await GetActionNextProcess(
                workflowConfig: wfConfig,
                sourceId: sourceId,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<BsonDocument?> GetActionNextProcess(
        BsonDocument workflowConfig,
        string sourceId,
        string label = "...",
        CancellationToken cancellationToken = default
    )
    {
        var endEdge = new BsonDocument
        {
            {
                "data",
                new BsonDocument
                {
                    { "label", "..." },
                    { "id", new Guid().ToString() },
                    { "type", "end" },
                }
            },
            { "source", "end" },
            { "target", "end" },
        };

        try
        {
            await Task.CompletedTask;

            return (
                    label != "..." && !string.IsNullOrWhiteSpace(label)
                        ? await FindEdgeBySourceAndLabel(
                            workflowConfig: workflowConfig,
                            sourceId: !string.IsNullOrWhiteSpace(sourceId) ? sourceId : "start",
                            label: label
                        )
                        : await FindEdgeBySource(
                            workflowConfig: workflowConfig,
                            sourceId: !string.IsNullOrWhiteSpace(sourceId) ? sourceId : "start"
                        )
                ) ?? endEdge;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return endEdge;
        }
    }

    // NOTE: Proses ini memang di skip, biarkan method ini tetak kosong.
    internal async Task<JObject?> EndProcess(
        string ttable,
        string appNo,
        string category,
        string wfCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }

    internal async Task<(int, int)> GetCalcProcessStep(
        BsonDocument workflowConfig,
        string appNo,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await Task.CompletedTask;

            var totalStep = workflowConfig["edges"].AsBsonArray.Count;

            var currentStep = (
                await mongoDB
                    .GetCollection<BsonDocument>(name: WF_PROCESS_LOG)
                    .Distinct<string>(
                        field: "pol_source_id",
                        filter: Builders<BsonDocument>.Filter.Eq(field: "pol_app_id", value: appNo),
                        cancellationToken: cancellationToken
                    )
                    .ToListAsync(cancellationToken: cancellationToken)
            ).Count;

            return (totalStep, currentStep);
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return (0, 0);
        }
    }

    internal async Task<BsonDocument?> UpsertWorkflowConfig(
        JObject workflowConfig,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(argument: workflowConfig);

            var document = workflowConfig.AsBsonDocument()!.UpdateProcessTime();

            await mongoDB
                .GetCollection<BsonDocument>(name: WF_CONFIG)
                .ReplaceOneAsync(
                    filter: Builders<BsonDocument>.Filter.Eq(
                        field: "flows_code",
                        value: document["flows_code"]
                    ),
                    replacement: document!,
                    options: new ReplaceOptions { IsUpsert = true },
                    cancellationToken: cancellationToken
                );

            return document;
        }
        catch (Exception ex)
        {
            ex.AdditionalLoggingAction(language: language, systemLogging: systemLogging);
            return null;
        }
    }
}
