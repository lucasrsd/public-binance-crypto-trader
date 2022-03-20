using CriptoTrade.Domain.Model.BaseRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.Core.Parametros
{
    public class CriptoConfig : BaseRepositoryEntity
    {
        public string CriptoSymbol { get; set; }
        public string CriptoPair { get; set; }
        public int CasasDecimaisCripto { get; set; }
        public int CasasDecimaisFiat { get; set; }
        public int ValorAporte { get; set; }
        public DateTime? DataCadastro { get; set; }
        public decimal? LimiteFiatCripto { get; set; }
    }
}
