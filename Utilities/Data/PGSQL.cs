using IDC.Utilities.Data;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Data;

internal class PGSQL(PostgreHelper pgHelper)
{
    internal async Task<JArray?> GetListSourceNonDynamic(
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();
        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "get_list_non_dynamic",
                Parameters = [],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JObject?> CheckSourceND(
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        var data = new JObject();
        var dtReturn = await GetListSourceNonDynamic(cancellationToken);
        var items = dtReturn?.SelectTokens($"$.[?(@.code=='{sourceId}')]").ToList();

        if (items?.Count > 0)
            data = JObject.Parse(items[0].ToString());

        return data;
    }

    internal async Task<JObject?> CheckSourceDF(
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        async Task<JArray?> GetListSourceDF(CancellationToken cancellationToken = default)
        {
            var result = new List<JObject?>();
            await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

            (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
                spCallInfo: new PostgreHelper.SPCallInfo
                {
                    Schema = "workflow",
                    SPName = "get_list_dflow",
                    Parameters = [],
                },
                cancellationToken: cancellationToken
            );

            return result is not null ? JArray.FromObject(result) : null;
        }

        var data = new JObject();
        var dtReturn = await GetListSourceDF(cancellationToken);
        var items = dtReturn?.SelectTokens($"$.[?(@.code=='{sourceId}')]").ToList();

        if (items?.Count > 0)
            data = JObject.Parse(items[0].ToString());

        return data;
    }

    internal async Task<JArray?> ListIntegrationObject(
        CancellationToken cancellationToken = default
    )
    {
        List<JObject?>? result = [];
        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "wf_list_integration_obj",
                Parameters = [],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JObject?> CheckIntegrationMember(JArray arrData, string code)
    {
        await Task.CompletedTask;

        var data = new JObject();

        var items = arrData
            .SelectTokens("$.[?(@.code=='" + code + "')]")
            .OfType<JObject>()
            .ToList();
        if (items.Count > 0)
        {
            data = JObject.Parse(items[0].ToString());
        }

        return data;
    }

    // TODO: Complete this method
    internal static async Task<JObject?> DecisionResult(
        JObject data,
        CancellationToken cancellationToken = default
    )
    {
        await Task.CompletedTask;
        return [];
    }

    internal async Task<JArray?> UpdateProcessLogTransfertaskPreint(
        string appNo,
        string sourceId,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();
        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "update_process_log_transfer_task",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_rsh_id",
                        Value = appNo,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_source_id",
                        Value = sourceId,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JObject> GetDetailModuleXTR(
        string name,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "interface",
                SPName = "get_module_wsdetail",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_name",
                        Value = name,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_code",
                        Value = code,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result?.FirstOrDefault() ?? [];
    }

    internal async Task<JObject?> GetCchAllocate(
        string id,
        string? ttable,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "public",
                SPName = "cch_distribution_alloc",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_id",
                        Value = long.Parse(id),
                        DataType = NpgsqlTypes.NpgsqlDbType.Bigint,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_ttable",
                        Value = ttable,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result?.FirstOrDefault() ?? [];
    }

    internal async Task UpdateCchLog(
        string id,
        string decision,
        CancellationToken cancellationToken = default
    )
    {
        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        await pgHelper.ExecuteNonQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "public",
                SPName = "cch_distribution_update",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_id",
                        Value = long.Parse(id),
                        DataType = NpgsqlTypes.NpgsqlDbType.Bigint,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_decision",
                        Value = decision,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );
    }

    internal async Task<JObject?> GetDataTempTableByTblname(
        string tablename,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "public",
                SPName = "get_tmp_data2",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_tablename",
                        Value = tablename,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result?.FirstOrDefault() ?? [];
    }

    internal async Task<JArray?> UpdateRshByTTable(
        string ttable,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "df_update_rsh_byttable",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_table",
                        Value = ttable,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JArray?> InsertWorkflowPending(
        string rshId,
        string flowId,
        string ttable,
        string node,
        string gccode,
        string info,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "sch_insert_workflow_pending",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_rshid",
                        Value = rshId,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_flowid",
                        Value = flowId,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_ttable",
                        Value = ttable,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_node",
                        Value = node,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_gccode",
                        Value = gccode,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_info",
                        Value = info,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JArray?> ListFormComponentDtl(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "wf_list_form_component_dtl",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_code_hdr",
                        Value = code,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result is not null ? JArray.FromObject(result) : null;
    }

    internal async Task<JArray?> DetailComponentADF(
        JArray jaRaw,
        CancellationToken cancellationToken = default
    )
    {
        var jaData = new JArray();
        if (jaRaw.Count > 0)
        {
            foreach (var jo in jaRaw)
            {
                var joData = JObject.Parse(jo.ToString());
                var compcode = joData["code"]?.ToString();
                if (compcode is not null)
                {
                    var objReturn2 = await ListFormComponentDtl(
                        code: compcode,
                        cancellationToken: cancellationToken
                    );
                    if (objReturn2?.Count > 0)
                    {
                        joData["component_dtl"] = await DetailComponentADF(
                            jaRaw: objReturn2,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                jaData.Add(joData);
            }
        }

        return jaData;
    }

    internal async Task<JObject> DefineComponentADF(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var data = new JObject();
        var jaData = new JArray();
        try
        {
            var objReturn1 = await ListFormComponentDtl(
                code: code,
                cancellationToken: cancellationToken
            );

            if (objReturn1?.Count > 0)
            {
                jaData = new JArray(
                    objReturn1.Select(x =>
                    {
                        var joData = JObject.Parse(json: x.ToString());
                        var objReturn2 = ListFormComponentDtl(
                            code: joData["code"]?.ToString() ?? string.Empty,
                            cancellationToken: cancellationToken
                        );
                        if (objReturn2?.Result?.Count > 0)
                        {
                            joData["component_dtl"] = DetailComponentADF(
                                jaRaw: objReturn2.Result,
                                cancellationToken: cancellationToken
                            ).Result;
                        }
                        return joData;
                    })
                );
            }

            data.Add(propertyName: "status", value: "Success");
            data.Add(propertyName: "result", value: jaData);
        }
        catch (Exception ex)
        {
            data.Add(propertyName: "status", value: "Failed");
            data.Add(propertyName: "message", value: ex.Message);
            return data;
        }

        return data;
    }

    internal async Task<JObject?> GetExternalModuleDetail(
        string name,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var result = new List<JObject?>();

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteQueryAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "interface",
                SPName = "get_module_detail",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_name",
                        Value = name,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_code",
                        Value = code,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return result?.FirstOrDefault() ?? [];
    }

    internal async Task<JObject?> GetWorkflowConfig(
        string workflowId,
        CancellationToken cancellationToken = default
    )
    {
        object? result;

        await pgHelper.ConnectAsync(cancellationToken: cancellationToken);

        (pgHelper, result) = await pgHelper.ExecuteScalarAsync(
            spCallInfo: new PostgreHelper.SPCallInfo
            {
                Schema = "workflow",
                SPName = "workflow_json_generator",
                Parameters =
                [
                    new PostgreHelper.SPParameter
                    {
                        Name = "p_workflow_id",
                        Value = workflowId,
                        DataType = NpgsqlTypes.NpgsqlDbType.Varchar,
                    },
                ],
            },
            cancellationToken: cancellationToken
        );

        return JObject.Parse(result?.ToString() ?? "{}");
    }
}
