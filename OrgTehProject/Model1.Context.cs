﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrgTehProject
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OrgTehEntities : DbContext
    {
        private static OrgTehEntities m_instance;

        public static OrgTehEntities GetInstance()
        {
            if (m_instance == null) m_instance = new OrgTehEntities();
            return m_instance;
        }
        public OrgTehEntities()
            : base("name=OrgTehEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Basket> Baskets { get; set; }
        public virtual DbSet<CategoryOfTehnika> CategoryOfTehnikas { get; set; }
        public virtual DbSet<CountryForMade> CountryForMades { get; set; }
        public virtual DbSet<ItemInZakaz> ItemInZakazs { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StatusZakaza> StatusZakazas { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<Tehnika> Tehnikas { get; set; }
        public virtual DbSet<TypeTehnika> TypeTehnikas { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Zakaz> Zakazs { get; set; }
    }
}
