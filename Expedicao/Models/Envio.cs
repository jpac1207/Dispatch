using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Expedicao.Models
{
    [Table("tbenvio")]
    public class Envio
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("OrdemManutencao")]
        public string OrdemManutencao { get; set; }
        [Column("Descricao")]
        [Required]
        public string Descricao { get; set; }
        [Column("Quantidade")]
        public int Quantidade { get; set; }
        [Column("NumeroSerie")]
        public string NumeroSerie { get; set; }
        [Column("IdTipoTransporte")]
        public int TipoTransporteId { get; set; }
        public TipoTransporte TipoTransporte { get; set; }
        [Column("NotaFiscal")]
        public string NotaFiscal { get; set; }
        [Column("IdMotivo")]
        public int MotivoId { get; set; }
        public Motivo Motivo { get; set; }
        [Column("NumeroImpressaoNota")]
        public string NumeroImpressaoNota { get; set; }
        [Column("NotaTransferenciaSap")]
        public string NotaTransferenciaSap { get; set; }
        [Column("IdSolicitacao")]
        public int? IdSolicitacao { get; set; }
        [Column("IdSedeOrigem")]
        public Int32 SedeOrigemId { get; set; }
        [ForeignKey("SedeOrigemId")]
        public Sede SedeOrigem { get; set; }
        [Column("IdSedeDestino")]
        public Int32 SedeDestinoId { get; set; }
        [ForeignKey("SedeDestinoId")]
        public Sede SedeDestino { get; set; }

        /*ViewModel Attributes*/
        [NotMapped]
        public string CurrentStatus { get; set; }
        [NotMapped]
        public string DataCriacao { get; set; }
        [NotMapped]
        public string DataEnvio { get; set; }
        [NotMapped]
        public string DataRecebimento { get; set; }
        [NotMapped]
        public string MatriculaDespachante { get; set; }
        [NotMapped]
        public string MatriculaRecebedor { get; set; }
    }
}