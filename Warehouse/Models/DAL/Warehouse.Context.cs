﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Warehouse.Models.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WarehouseEntities : DbContext
    {
        public WarehouseEntities()
            : base("name=WarehouseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Client> Clients { get; set; }
        public DbSet<CMR_Dispatches> CMR_Dispatches { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Deliveries_Dispatches> Deliveries_Dispatches { get; set; }
        public DbSet<Dispatch> Dispatches { get; set; }
        public DbSet<Dispatches_Positions> Dispatches_Positions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Orders_Positions> Orders_Positions { get; set; }
        public DbSet<Protocols_of_Difference> Protocols_of_Difference { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
