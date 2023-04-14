/*
 * PxApi
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using PxWeb.Attributes.Api2;
using PxWeb.Api2.Server.Models;
using PCAxis.Paxiom;
using Px.Abstractions.Interfaces;
using PxWeb.Helper.Api2;
using PxWeb.Mappers;
using Px.Search;
using System.Linq;
using Lucene.Net.Util;
using PxWeb.Code.Api2.Serialization;
using Microsoft.AspNetCore.Http;
using PxWeb.Config.Api2;
using System.Runtime.Serialization;
using PxWeb.Code.Api2.DataSelection;
using Microsoft.Extensions.Options;

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
        private readonly ITablesResponseMapper _tablesResponseMapper;
        private readonly ISearchBackend _backend;
        private PxApiConfigurationOptions _configOptions;
        private readonly ISelectionHandler _selectionHandler;

        public TableApiController(IDataSource dataSource, ILanguageHelper languageHelper, ITableMetadataResponseMapper responseMapper, ISearchBackend backend, IOptions<PxApiConfigurationOptions> configOptions, ITablesResponseMapper tablesResponseMapper, ISelectionHandler selectionHandler)
        {
            _dataSource = dataSource;
            _languageHelper = languageHelper;
            _tableMetadataResponseMapper = responseMapper;
            _backend = backend;
            _configOptions = configOptions.Value;
            _tablesResponseMapper = tablesResponseMapper;
            _selectionHandler = selectionHandler;   
        }


        public override IActionResult GetMetadataById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            lang = _languageHelper.HandleLanguage(lang);
            IPXModelBuilder? builder = _dataSource.CreateBuilder(id, lang);


            if (builder != null)
            {
                try
                {
                    builder.BuildForSelection();
                    var model = builder.Model;

                    TableMetadataResponse tm = _tableMetadataResponseMapper.Map(model, id, lang);

                    return new ObjectResult(tm);
                }
                catch (Exception)
                {
                    return NotFound(NonExistentTable());
                }
            }
            else
            {
                return NotFound(NonExistentTable());
            }
        }

        private Problem NonExistentTable()
        {
            Problem p = new Problem();
            p.Type = "Parameter error";
            p.Status = 404;
            p.Title = "Non-existent table";
            return p;
        }

        private Problem OutOfRange()
        {
            Problem p = new Problem();
            p.Type = "Parameter error";
            p.Detail = "Non-existent page";
            p.Status = 404;
            p.Title = "Non-existent page";
            return p;
        }

        public override IActionResult GetTableById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }

        public override IActionResult GetTableCodeListById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }



        private IDataSerializer GetSerializer(string outputFormat)
        {
            switch (outputFormat.ToLower())
            {
                case "xlsx":
                case "xlsx_doublecolumn":
                case "csv":
                case "csv_tab":
                case "csv_tabhead":
                case "csv_comma":
                case "csv_commahead":
                case "csv_space":
                case "csv_spacehead":
                case "csv_semicolon":
                case "csv_semicolonhead":
                case "csv2":
                case "csv3":
                case "json_stat":
                case "json_stat2":
                case "html5_table":
                case "relational_table":
                case "px":
                default:
                    return new PxDataSerializer();
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
                return NotFound(OutOfRange());
            }

            return Ok(_tablesResponseMapper.Map(searchResultContainer, lang, query));

        }

        public override IActionResult GetTableData([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "valuecodes")] Dictionary<string, List<string>>? valuecodes, [FromQuery(Name = "codelist")] Dictionary<string, string>? codelist, [FromQuery(Name = "outputvalues")] Dictionary<string, CodeListOutputValuesType>? outputvalues, [FromQuery(Name = "outputFormat")] string? outputFormat)
        {
            // Map querystring parameters to VariablesSelection object
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

            return GetData(id, lang, selections, outputFormat);
        }

        public override IActionResult GetTableDataByPost([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "outputFormat")] string? outputFormat, [FromBody] VariablesSelection? variablesSelection)
        {
            return GetData(id, lang, variablesSelection, outputFormat);
        }

        private IActionResult GetData(string id, string? lang, VariablesSelection? variablesSelection, string? outputFormat)
        {
            Problem? problem;

            lang = _languageHelper.HandleLanguage(lang);

            var builder = _dataSource.CreateBuilder(id, lang);
            if (builder == null)
            {
                return NotFound(NonExistentTable());
            }

            builder.BuildForSelection();

            if (!_selectionHandler.Verify(builder.Model, variablesSelection, out problem))
            {
                return BadRequest(problem);
            }

            var selection = _selectionHandler.GetSelection(builder.Model, variablesSelection);

            builder.BuildForPresentation(selection);
            var serializer = GetSerializer(outputFormat);
            serializer.Serialize(builder.Model, Response);

            return Ok();
        }

 
    }
}
