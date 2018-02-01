using Expedicao.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Expedicao.DAL
{
    public class UserContext : DbContext
    {
        public UserContext() : base("UserContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Sede> Sedes { get; set; }
        public DbSet<GrantedUser> GrantedUsers { get; set; }
        public DbSet<User> Users { get; set; }
    }
}