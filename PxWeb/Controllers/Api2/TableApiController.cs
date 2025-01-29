/*
 * PxApi
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using PCAxis.Paxiom;
using PCAxis.Paxiom.Operations;

using Px.Abstractions;
using Px.Abstractions.Interfaces;
using Px.Search;

using PxWeb.Api2.Server.Models;
using PxWeb.Code.Api2.DataSelection;
using PxWeb.Code.Api2.ModelBinder;
using PxWeb.Code.Api2.Serialization;
using PxWeb.Helper.Api2;
using PxWeb.Mappers;

namespace PxWeb.Controllers.Api2
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class TableApiController : PxWeb.Api2.Server.Controllers.TableApiController
    {
        private readonly IDataSource _dataSource;
        private readonly ILanguageHelper _languageHelper;
        private readonly ITableMetadataResponseMapper _tableMetadataResponseMapper;
        private readonly IDatasetMapper _datasetMapper;
        private readonly ITablesResponseMapper _tablesResponseMapper;
        private readonly ITableResponseMapper _tableResponseMapper;
        private readonly ICodelistResponseMapper _codelistResponseMapper;
        private readonly ISearchBackend _backend;
        private readonly ISerializeManager _serializeManager;
        private readonly PxApiConfigurationOptions _configOptions;
        private readonly ISelectionHandler _selectionHandler;
        private readonly IPlacementHandler _placementHandler;
        private readonly ISelectionResponseMapper _selectionResponseMapper;

        public TableApiController(IDataSource dataSource, ILanguageHelper languageHelper, ITableMetadataResponseMapper responseMapper, IDatasetMapper datasetMapper, ISearchBackend backend, IOptions<PxApiConfigurationOptions> configOptions, ITablesResponseMapper tablesResponseMapper, ITableResponseMapper tableResponseMapper, ICodelistResponseMapper codelistResponseMapper, ISelectionResponseMapper selectionResponseMapper, ISerializeManager serializeManager, ISelectionHandler selectionHandler, IPlacementHandler placementHandler)
        {
            _dataSource = dataSource;
            _languageHelper = languageHelper;
            _tableMetadataResponseMapper = responseMapper;
            _datasetMapper = datasetMapper;
            _backend = backend;
            _configOptions = configOptions.Value;
            _tablesResponseMapper = tablesResponseMapper;
            _tableResponseMapper = tableResponseMapper;
            _codelistResponseMapper = codelistResponseMapper;
            _serializeManager = serializeManager;
            _selectionHandler = selectionHandler;
            _selectionResponseMapper = selectionResponseMapper;
            _placementHandler = placementHandler;
        }

        public override IActionResult GetMetadataById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "outputFormat")] MetadataOutputFormatType? outputFormat, [FromQuery(Name = "defaultSelection")] bool? defaultSelection)
        {
            lang = _languageHelper.HandleLanguage(lang);
            IPXModelBuilder? builder = _dataSource.CreateBuilder(id, lang);



            if (builder != null)
            {
                try
                {
                    builder.BuildForSelection();
                    var model = builder.Model;

                    if (defaultSelection is not null && defaultSelection == true)
                    {
                        //apply the default selection
                        Problem? problem;
                        var selectionx = _selectionHandler.GetDefaultSelection(builder, out problem);
                    }


                    if (outputFormat != null && outputFormat == MetadataOutputFormatType.Stat2Enum)
                    {

                        Dataset ds = _datasetMapper.Map(model, id, lang);
                        return new ObjectResult(ds);
                    }
                    else
                    {
                        TableMetadataResponse tm = _tableMetadataResponseMapper.Map(model, id, lang);
                        return new ObjectResult(tm);
                    }
                }
                catch (Exception)
                {
                    return NotFound(ProblemUtility.NonExistentTable());
                }
            }
            else
            {
                return NotFound(ProblemUtility.NonExistentTable());
            }
        }

        public override IActionResult GetTableById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            Searcher searcher = new Searcher(_dataSource, _backend);
            lang = _languageHelper.HandleLanguage(lang);

            if (!_dataSource.TableExists(id, lang))
            {
                return NotFound(ProblemUtility.NonExistentTable());
            }

            SearchResult? searchResult = searcher.FindTable(id, lang);
            if (searchResult == null)
            {
                return NotFound(ProblemUtility.NonExistentTable());
            }

            return Ok(_tableResponseMapper.Map(searchResult, lang));


        }

        public override IActionResult GetTableCodeListById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            lang = _languageHelper.HandleLanguage(lang);
            Codelist? codelist = _dataSource.GetCodelist(id, lang);

            if (codelist != null)
            {
                return Ok(_codelistResponseMapper.Map(codelist, lang));
            }
            else
            {
                return NotFound(ProblemUtility.NonExistentCodelist());
            }
        }

        public override IActionResult ListAllTables([FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "query")] string? query, [FromQuery(Name = "pastDays")] int? pastDays, [FromQuery(Name = "includeDiscontinued")] bool? includeDiscontinued, [FromQuery(Name = "pageNumber")] int? pageNumber, [FromQuery(Name = "pageSize")] int? pageSize)
        {
            Searcher searcher = new Searcher(_dataSource, _backend);

            lang = _languageHelper.HandleLanguage(lang);

            if (pageNumber == null || pageNumber <= 0)
                pageNumber = 1;

            if (pageSize == null || pageSize <= 0)
                pageSize = _configOptions.PageSize;

            var searchResultContainer = searcher.Find(query, lang, pastDays, includeDiscontinued ?? false, pageSize.Value, pageNumber.Value);

            if (searchResultContainer.outOfRange == true)
            {
                return NotFound(ProblemUtility.OutOfRange());
            }

            return Ok(_tablesResponseMapper.Map(searchResultContainer, lang, query));

        }

        public override IActionResult GetTableData([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "valuecodes"), ModelBinder(typeof(QueryStringToDictionaryOfStrings))] Dictionary<string, List<string>>? valuecodes, [FromQuery(Name = "codelist")] Dictionary<string, string>? codelist, [FromQuery(Name = "outputvalues")] Dictionary<string, CodeListOutputValuesType>? outputvalues, [FromQuery(Name = "outputFormat")] string? outputFormat, [FromQuery(Name = "outputFormatParams"), ModelBinder(typeof(CommaSeparatedStringToListOfStrings))] List<string>? outputFormatParams, [FromQuery(Name = "heading"), ModelBinder(typeof(CommaSeparatedStringToListOfStrings))] List<string>? heading, [FromQuery(Name = "stub"), ModelBinder(typeof(CommaSeparatedStringToListOfStrings))] List<string>? stub)
        {
            VariablesSelection variablesSelection = MapDataParameters(valuecodes, codelist, outputvalues, heading, stub);
            return GetData(id, lang, variablesSelection, outputFormat, outputFormatParams is null ? new List<string>() : outputFormatParams);
        }

        public override IActionResult GetTableDataByPost([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "outputFormat")] string? outputFormat, [FromQuery(Name = "outputFormatParams"), ModelBinder(typeof(CommaSeparatedStringToListOfStrings))] List<string>? outputFormatParams, [FromBody] VariablesSelection? variablesSelection)
        {
            return GetData(id, lang, variablesSelection, outputFormat, outputFormatParams is null ? new List<string>() : outputFormatParams);
        }

        private IActionResult GetData(string id, string? lang, VariablesSelection? variablesSelection, string? outputFormat, List<string> outputFormatParams)
        {
            Problem? problem = null;

            lang = _languageHelper.HandleLanguage(lang);

            var builder = _dataSource.CreateBuilder(id, lang);
            if (builder == null)
            {
                return NotFound(ProblemUtility.NonExistentTable());
            }

            builder.BuildForSelection();

            Selection[]? selection = null;
            //bool IsDefaultSelection = false;
            VariablePlacementType? placment = null;

            if (_selectionHandler.UseDefaultSelection(variablesSelection))
            {
                List<string> heading, stub;
                (selection, heading, stub) = _selectionHandler.GetDefaultSelection(builder, out problem);
                placment = new VariablePlacementType() { Heading = heading, Stub = stub };

                //IsDefaultSelection = true;
            }
            else
            {
                if (variablesSelection is not null)
                {
                    selection = _selectionHandler.GetSelection(builder, variablesSelection, out problem);

                    if (problem is null && selection is not null)
                    {
                        //Check if we should pivot the table
                        placment = _placementHandler.GetPlacment(variablesSelection, selection, builder.Model.Meta, out problem);
                        //GetPlacment(variablesSelection, selection, builder, out problem);
                    }
                }
            }

            if (problem is not null)
            {
                return BadRequest(problem);
            }

            builder.BuildForPresentation(selection);

            var model = builder.Model;

            if (placment is not null)
            {
                var descriptions = new List<PivotDescription>();

                descriptions.AddRange(placment.Heading.Select(h => new PivotDescription()
                {
                    VariableName = model.Meta.Variables.First(v => v.Code.Equals(h, StringComparison.OrdinalIgnoreCase)).Name,
                    VariablePlacement = PlacementType.Heading
                }));

                descriptions.AddRange(placment.Stub.Select(h => new PivotDescription()
                {
                    VariableName = model.Meta.Variables.First(v => v.Code == h).Name,
                    VariablePlacement = PlacementType.Stub
                }));

                var pivot = new PCAxis.Paxiom.Operations.Pivot();
                model = pivot.Execute(model, descriptions.ToArray());
            }

            if (outputFormat == null)
            {
                outputFormat = _configOptions.DefaultOutputFormat;
            }
            else if (!_configOptions.OutputFormats.Contains(outputFormat, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ProblemUtility.UnsupportedOutputFormat());
            }

            var serializationInfo = _serializeManager.GetSerializer(outputFormat, model.Meta.CodePage, outputFormatParams);

            Response.ContentType = serializationInfo.ContentType;
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{model.Meta.Matrix}{serializationInfo.Suffix}\"");
            serializationInfo.Serializer.Serialize(model, Response.Body);

            return Ok();
        }



        /// <summary>
        /// Map querystring parameters to VariablesSelection object
        /// </summary>
        /// <param name="valuecodes"></param>
        /// <param name="codelist"></param>
        /// <param name="outputvalues"></param>
        /// <param name="heading"></param>
        /// <param name="stub"></param> 
        /// <returns></returns>
        private VariablesSelection MapDataParameters(Dictionary<string, List<string>>? valuecodes, Dictionary<string, string>? codelist, Dictionary<string, CodeListOutputValuesType>? outputvalues, List<string>? heading, List<string>? stub)
        {
            VariablesSelection selections = new VariablesSelection();
            if (valuecodes != null)
            {
                selections.Selection = new List<VariableSelection>();
                foreach (var variableCode in valuecodes.Keys)
                {
                    VariableSelection variableSelection = new VariableSelection();
                    variableSelection.VariableCode = variableCode;
                    variableSelection.ValueCodes = valuecodes[variableCode];
                    if (codelist != null && codelist.ContainsKey(variableCode))
                    {
                        variableSelection.CodeList = codelist[variableCode];
                    }
                    if (outputvalues != null && outputvalues.ContainsKey(variableCode))
                    {
                        variableSelection.OutputValues = outputvalues[variableCode];
                    }
                    selections.Selection.Add(variableSelection);
                }
            }

            selections.Placement = new VariablePlacementType();
            selections.Placement.Heading = heading ?? new List<string>();
            selections.Placement.Stub = stub ?? new List<string>();

            return selections;
        }



        public override IActionResult GetDefaultSelection([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            Problem? problem;

            lang = _languageHelper.HandleLanguage(lang);

            var builder = _dataSource.CreateBuilder(id, lang);
            if (builder == null)
            {
                return NotFound(ProblemUtility.NonExistentTable());
            }

            builder.BuildForSelection();

            //No variable selection is provided, so we will return the default selection

            var (selection, heading, stub) = _selectionHandler.GetDefaultSelection(builder, out problem);

            if (problem is not null || selection is null)
            {
                return BadRequest(problem);
            }

            //Map selection to SelectionResponse
            SelectionResponse selectionResponse = _selectionResponseMapper.Map(selection, heading, stub, builder.Model.Meta, id, lang);
            return Ok(selectionResponse);
        }

    }

}
