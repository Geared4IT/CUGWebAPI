﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cugonlineWebAPI.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class testEntities : DbContext
    {
        public testEntities()
            : base("name=testEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ReciprocalSeeMainRef> ReciprocalSeeMainRefs { get; set; }
        public virtual DbSet<FileExtension> FileExtensions { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<FilesLink> FilesLinks { get; set; }
        public virtual DbSet<FileType> FileTypes { get; set; }
        public virtual DbSet<IdxMain> IdxMains { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Main> Mains { get; set; }
        public virtual DbSet<Request_detail> Request_detail { get; set; }
        public virtual DbSet<Request_master> Request_master { get; set; }
        public virtual DbSet<SeeMain> SeeMains { get; set; }
        public virtual DbSet<txtBox> txtBoxes { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }
    }
}