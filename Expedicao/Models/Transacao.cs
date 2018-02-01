using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Expedicao.Models
{
    [Table("tbtransacao")]
    public class Transacao
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Data")]
        public DateTime Data { get; set; }
        [Column("IdStatus")]
        public int StatusId { get; set; }
        Status Status { get; set; }
        [Column("IdEnvio")]
        public int EnvioId { get; set; }
        public Envio Envio { get; set; }
        [Column("MatriculaModificador")]
        public string MatriculaModificador { get; set; }
    }
}