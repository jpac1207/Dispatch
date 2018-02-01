using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Expedicao.Models
{
    [Table("tbgranteduser")]
    public class GrantedUser
    {
        [Key, Column("user_id", Order = 0)]
        public int UserId { get; set; }
        public User User { get; set; }
        [Key, Column("app_id", Order = 1)]
        public int AppId { get; set; }
        [Key, Column("grant_id", Order = 2)]
        public int GrantId { get; set; }
    }
}