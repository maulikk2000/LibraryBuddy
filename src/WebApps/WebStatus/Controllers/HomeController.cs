using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.HealthChecks;
using WebStatus.Models;

namespace WebStatus.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HomeController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }
        public async Task<IActionResult> Index()
        {
            var healthCheckResult = await _healthCheckService.CheckHealthAsync();

            var checkStatusdata = new HealthStatusViewModel(healthCheckResult.CheckStatus);

            foreach (var result in healthCheckResult.Results)
            {
                checkStatusdata.AddResult(result.Key, result.Value);
            }

            ViewBag.RefreshSeconds = 60;
            return View(checkStatusdata);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
