using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Expedicao.DAL
{
    public class ExpedicaoContext : DbContext
    {
        public ExpedicaoContext() : base("ExpedicaoContext")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<Expedicao.Models.Motivo> Motivos { get; set; }

        public System.Data.Entity.DbSet<Expedicao.Models.TipoTransporte> TipoTransportes { get; set; }

        public System.Data.Entity.DbSet<Expedicao.Models.Sede> Sedes { get; set; }

        public System.Data.Entity.DbSet<Expedicao.Models.Envio> Envios { get; set; }

        public System.Data.Entity.DbSet<Expedicao.Models.Status> Status { get; set; }

        public System.Data.Entity.DbSet<Expedicao.Models.Transacao> Transacoes { get; set; }
    }
}