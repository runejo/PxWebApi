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
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PxWeb.Models.Api2;
using PxWeb.Attributes.Api2;
using Px.Abstractions.Interfaces;
using PxWeb.Config.Api2;
using PxWeb.Helper.Api2;
using PCAxis.Menu;
using Link = PxWeb.Models.Api2.Link;
using System.IO;
using PxWeb.Mappers;

namespace PxWeb.Controllers.Api2
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class NavigationApiController : ControllerBase
    {
        private readonly IDataSource _dataSource;
        private readonly ILanguageHelper _languageHelper;
        private readonly IResponseMapper _responseMapper;   
        
        public NavigationApiController(IDataSource dataSource, ILanguageHelper languageHelper, IResponseMapper responseMapper )
        {
            _dataSource = dataSource;
            _languageHelper = languageHelper;
            _responseMapper = responseMapper;   
        }

        /// <summary>
        /// Gets navigation item with the given id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="lang">The language if the default is not what you want.</param>
        /// <response code="200">Success</response>
        [HttpGet]
        [Route("/v2/navigation/{id}")]
        [ValidateModelState]
        [SwaggerOperation("GetNavigationById")]
        [SwaggerResponse(statusCode: 200, type: typeof(Folder), description: "Success")]
        //public virtual IActionResult GetNavigationById([FromRoute][Required] string id, [FromQuery] string lang)
        public virtual IActionResult GetNavigationById([FromRoute(Name = "id")][Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            bool selectionExists = true;

            lang = _languageHelper.HandleLanguage(lang);

            PxMenuBase menu = _dataSource.CreateMenu(id, lang, out selectionExists);

            if (!selectionExists)
            {
                return new BadRequestObjectResult("No such node id " + id);
            }

            if (menu == null)
            {
                return new BadRequestObjectResult("Error reading data");
            }

            if (menu.CurrentItem == null)
            {
                return new BadRequestObjectResult("Error reading node data");
            }

            Folder folder = _responseMapper.GetFolder((PxMenuItem)menu.CurrentItem, HttpContext);

            return new ObjectResult(folder);

        }


        /// <summary>
        /// Browse the database structure
        /// </summary>
        /// <param name="lang">The language if the default is not what you want.</param>
        /// <response code="200">Success</response>
        /// <response code="429">Error respsone for 429</response>
        [HttpGet]
        [Route("/v2/navigation")]
        [ValidateModelState]
        [SwaggerOperation("GetNavigationRoot")]
        [SwaggerResponse(statusCode: 200, type: typeof(Folder), description: "Success")]
        [SwaggerResponse(statusCode: 429, type: typeof(Problem), description: "Error respsone for 429")]
        //public virtual IActionResult GetNavigationRoot([FromQuery] string lang)
        public virtual IActionResult GetNavigationRoot([FromQuery(Name = "lang")] string? lang)
        {
            bool selectionExists = true;

            lang = _languageHelper.HandleLanguage(lang);

            PxMenuBase menu = _dataSource.CreateMenu("", lang, out selectionExists);

            if (menu == null)
            {
                return new BadRequestObjectResult("Error reading data");
            }

            if (menu.CurrentItem == null)
            {
                return new BadRequestObjectResult("Error reading node data");
            }

            Folder folder = _responseMapper.GetFolder((PxMenuItem)menu.CurrentItem, HttpContext);

            return new ObjectResult(folder);
        }


    }
}
