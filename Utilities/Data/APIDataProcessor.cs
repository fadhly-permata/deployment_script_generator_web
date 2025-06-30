using IDC.Utilities.Comm.Http;
using IDC.Utilities.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using ScriptDeployerWeb.Utilities.DI;
using ScriptDeployerWeb.Utilities.Extensions;

namespace ScriptDeployerWeb.Utilities.Data;

internal class APIDataProcessor(
    HttpClientUtility httpClient,
    AppSettingsHandler appSettings,
    bool ensureStatusCode = false
)
{
    internal async Task<JObject?> ProcessAsliri(
        JObject json,
        CancellationToken cancellationToken = default
    )
    {
        var data = new JObject();

        try
        {
            var joDataTempTbl = await CollectDataTemp(
                appId: json.PropGet<string>("app_id")!,
                cancellationToken: cancellationToken
            );

            if (json.ContainsKey("module"))
            {
                if (json.PropGet<string>("module")?.ToLower() == "verify_tax_personal")
                    joDataTempTbl.Add("data_type", "personal");
                else if (json.PropGet<string>("module")?.ToLower() == "verify_tax_company")
                    joDataTempTbl.Add("data_type", "company");

                data = await httpClient.PostJObjectAsync(
                    uri: appSettings.GetUriByName(
                        name: "urlAPI_idcocr",
                        path: $"VisionReader/{json.GetValue("endcode")?.ToString()}"
                    ),
                    content: joDataTempTbl,
                    ensureStatusCodeSuccess: ensureStatusCode,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            data?.PropUpsert(
                updates: new Dictionary<string, object?>
                {
                    { "status", "Error" },
                    { "message", ex.Message },
                }
            );
        }

        return data;
    }

    internal async Task<JObject> CollectDataTemp(
        string appId,
        CancellationToken cancellationToken = default
    )
    {
        var data = new JObject();

        var joRawRtnData = await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcen", path: "Kinicintaku/ttable"),
            content: new JObject { { "rsh_id", appId } },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

        if (joRawRtnData is not null && joRawRtnData.ContainsKey(propertyName: "data"))
        {
            var jaRawRtnData = joRawRtnData.PropGet<JArray>(path: "data") ?? [];
            if (jaRawRtnData.Count > 0)
            {
                var joRawData = JObject.Parse(json: jaRawRtnData[index: 0].ToString());
                var full_name =
                    $"{joRawData.PropGet(
                        path: "cst_fname",
                        defaultValue: ""
                     )} {joRawData.PropGet(
                            path: "cst_lname",
                            defaultValue: ""
                        )}".Trim();

                data = data.PropUpsert(
                    new Dictionary<string, object?>
                    {
                        { "app_id", appId },
                        { "nik", joRawData.PropGet<string>("cst_ktp") },
                        { "name", full_name },
                        { "birthdate", joRawData.PropGet<string>("cst_dob") },
                        { "birthplace", joRawData.PropGet<string>("cst_pob") },
                        { "address", joRawData.PropGet<string>("cst_address_ktp") },
                        { "npwp", joRawData.PropGet<string>("cst_npwp") },
                        { "income", joRawData.PropGet<string>("monthly_income") },
                        { "phone", joRawData.PropGet<string>("cst_phone_mobile") },
                    }
                );
            }
        }

        return data;
    }

    internal async Task<JObject?> IntegrationServices(
        string componentCode,
        string appNo,
        JObject data,
        BsonDocument? ttableData,
        CancellationToken cancellationToken = default
    )
    {
        var joTtable = ttableData.AsJObject() ?? [];
        joTtable.Remove("_id");

        return await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(
                name: "urlAPI_idclibrary",
                path: "idclibrary/integration/Process"
            ),
            content: new JObject
            {
                { "code", componentCode.ToUpper() },
                { "appno", appNo },
                { appNo, joTtable.PropMerge(other: data, mergeArrays: true) },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );
    }

    internal async Task<string?> GetGcValue(
        string gccode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var jaData =
                (
                    await httpClient.GetJObjectAsync(
                        uri: appSettings.GetUriByName(
                            name: "urlAPI_idccore",
                            path: $"global/config/detail//{gccode}"
                        ),
                        ensureStatusCodeSuccess: ensureStatusCode,
                        cancellationToken: cancellationToken
                    )
                )?.PropGet<JArray>("data") ?? throw new Exception("Data not found");

            return ((JObject)jaData[0]).PropGet<string>("glc_value")
                ?? throw new Exception("GLC Value not found");
        }
        catch (Exception ex)
        {
            return $"Failed [{ex.Message}]";
        }
    }

    internal async Task<JObject?> EnquiryPriviledge(
        string? sourceId,
        string? userId,
        string? appId,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcprivilege", path: "enquiry"),
            content: new JObject
            {
                { "source", sourceId },
                { "user_id", userId },
                { "tablename", appId },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> FormDynamicAdvance(
        string? sourceId,
        string? appNo,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(
                name: "urlAPI_idcform",
                path: "v1.0/FormAdvance/DynamicAdvance"
            ),
            content: new JObject { { "form_id", sourceId }, { "tablename", appNo } },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> DecisionFlowProcessor(
        string? dfCode,
        BsonDocument? data,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcdecisioncb", path: "api/Process/Start"),
            content: new JObject
            {
                { "code", dfCode },
                { "data", new JArray(data.AsJObject() ?? []) },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> CampionChallangerProcessor(
        string? dfCode,
        BsonDocument? data,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcdecisioncb", path: "api/Process/Start"),
            content: new JObject
            {
                { "code", dfCode },
                { "data", new JArray(data.AsJObject() ?? []) },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> EmailNotifProcessor(
        string appNo,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(
                name: "urlAPI_idcmail",
                path: "DataRequired/Notification/GenerateWF"
            ),
            content: new JObject { { "app_id", appNo } },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> XmlDataProcessor(
        string? req_name,
        string? res_name,
        string? appNo,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcservice", path: "Executes/XML"),
            content: new JObject
            {
                { "request_filename", req_name },
                { "response_filename", res_name },
                { "tablename", appNo },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> KBIJDataProcessor(
        string? appId,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcen", path: "productpurchase/exec/kbij"),
            content: new JObject { { "rsh_id", appId }, { "tablename", appId } },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> PefindoWorkflowProcessor(
        string? appNo,
        BsonDocument? ttableData,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(name: "urlAPI_idcpefindo", path: "EnqueryExec/Workflow"),
            content: new JObject
            {
                { "type_data", "Individual" },
                { "cust_code", ttableData?["cst_ktp"].AsString },
                { "dob", ttableData?["cst_dob"].AsString },
                { "id_number", ttableData?["cst_ktp"].AsString },
                { "usrid", appNo },
                { "ttable", appNo },
                {
                    "name",
                    $"{ttableData?["cst_fname"].AsString} {ttableData?["cst_lname"].AsString}".Trim()
                },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );

    internal async Task<JObject?> CheckPreviAction(
        string? workflowId,
        string? appId,
        string? sourceId,
        string? label,
        string? edgeId,
        CancellationToken cancellationToken = default
    ) =>
        await httpClient.PostJObjectAsync(
            uri: appSettings.GetUriByName(
                name: "urlAPI_idcflows",
                path: "Workflow/CheckPreviAction"
            ),
            content: new JObject
            {
                { "workflow_id", workflowId },
                { "app_id", appId },
                { "source_id", sourceId },
                { "label", label },
                { "edge_id", edgeId },
            },
            ensureStatusCodeSuccess: ensureStatusCode,
            cancellationToken: cancellationToken
        );
}
