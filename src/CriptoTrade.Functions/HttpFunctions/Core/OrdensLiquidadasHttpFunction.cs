using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Repository.Core;
using CriptoTrade.Domain.Model.Core.Ordem;
using System.Linq;
using CriptoTrade.Common.Env;

namespace CriptoTrade.Functions.HttpFunctions.Core
{
    public class OrdensLiquidadasHttpFunction
    {
        private readonly ICriptoOrdemMonitorRepository _monitorRepository;

        public OrdensLiquidadasHttpFunction(ICriptoOrdemMonitorRepository monitorRepository)
        {
            _monitorRepository = monitorRepository;
        }

        [FunctionName("OrdensLiquidadasHttpFunction")]
        public async Task<IActionResult> Run(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ordens/liquidadas")] HttpRequest req,
                ILogger log)
        {
            try
            {
                var user = req.Query["username"];
                log.LogInformation($"Iniciando requisicao ordens liquidadas para o usuario: {user}");

                var ordensLiquidadas = await _monitorRepository.GetByStatus(user, new System.Collections.Generic.List<StatusOrdem> { StatusOrdem.Vendida, StatusOrdem.VendaCriada });

                var result = new
                {
                    OrdensLiquidadas = ordensLiquidadas,
                    QuantidadeOrdens = ordensLiquidadas != null ? ordensLiquidadas.Count : 0,
                    SomaSpread = ordensLiquidadas != null ? ordensLiquidadas.Where(item => item.Status == StatusOrdem.Vendida).Sum(item => item.ValorSpread) : 0
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex.ToString());
            }
        }
    }
}
