using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MT_DataAccessLib;
using MT_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static MT_WebAPI.Startup;

namespace MT_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxonomyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly TaxonomyFactory _factory;

        public TaxonomyController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _factory = new TaxonomyFactory();
        }

        [HttpGet]
        public IEnumerable<Taxon> GetAll()
        {
           return _factory.GetAllTaxons();
        }

        [Route("{name}/{filter}")]
        [HttpGet]
        public IEnumerable<Taxon> GetTaxon(string name, string filter = "all")
        {
            return _factory.GetByName(name, filter);
        }
    }
}
