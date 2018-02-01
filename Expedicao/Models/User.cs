using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Expedicao.Models
{
    [Table("tbuser")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("senha")]
        public string Senha { get; }
        [Column("nome")]
        public string Nome { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("ramal")]
        public string Ramal { get; set; }
        [Column("motivo")]
        public string Motivo { get; set; }
        [Column("sede_id")]
        public int SedeId { get; set; }
        public Sede Sede { get; set; }
    }
}