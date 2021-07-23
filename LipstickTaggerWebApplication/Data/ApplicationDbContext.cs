using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LipstickTaggerWebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<UserSettingEntity> UserSettingEntities { get; set; }
    }
    public class UserSettingEntity
    {
        [Key]
        public string UserId { get; set; }
        public bool EnableAutoSave { get; set; }
    }
}
