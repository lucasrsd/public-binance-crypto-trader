using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.Core.Ordem
{
    public enum StatusOrdem
    {
        Pendente = 1,
        CompraConfirmada,
        VendaCriada,
        Vendida,
        Cancelada,
        Erro
    }
}
