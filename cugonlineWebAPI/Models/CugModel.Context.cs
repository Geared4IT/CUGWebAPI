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
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
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
        public virtual DbSet<FilesLink> FilesLinks { get; set; }
        public virtual DbSet<FileType> FileTypes { get; set; }
        public virtual DbSet<Request_detail> Request_detail { get; set; }
        public virtual DbSet<Request_master> Request_master { get; set; }
        public virtual DbSet<txtBox> txtBoxes { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<MainFilesLink> MainFilesLinks { get; set; }
        public virtual DbSet<MainFile> MainFiles { get; set; }
        public virtual DbSet<SeeMain> SeeMains { get; set; }
        public virtual DbSet<Main> Mains { get; set; }
        public virtual DbSet<BibloMain> BibloMains { get; set; }
        public virtual DbSet<UserActivity> UserActivities { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }
        public virtual DbSet<BibleAbbreviation> BibleAbbreviations { get; set; }
        public virtual DbSet<BibleBook> BibleBooks { get; set; }
        public virtual DbSet<BibleBooksKJV> BibleBooksKJVs { get; set; }
        public virtual DbSet<BibleFootNoteContent> BibleFootNoteContents { get; set; }
        public virtual DbSet<BibleFootNote> BibleFootNotes { get; set; }
        public virtual DbSet<BibleItalic> BibleItalics { get; set; }
        public virtual DbSet<BibloUpload> BibloUploads { get; set; }
    
        public virtual ObjectResult<sp_EmptyReferences_Result> sp_EmptyReferences(Nullable<int> strLength)
        {
            var strLengthParameter = strLength.HasValue ?
                new ObjectParameter("strLength", strLength) :
                new ObjectParameter("strLength", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_EmptyReferences_Result>("sp_EmptyReferences", strLengthParameter);
        }
    
        public virtual ObjectResult<sp_EmptyReferencesDetails_Result> sp_EmptyReferencesDetails(Nullable<int> strLength, string type)
        {
            var strLengthParameter = strLength.HasValue ?
                new ObjectParameter("strLength", strLength) :
                new ObjectParameter("strLength", typeof(int));
    
            var typeParameter = type != null ?
                new ObjectParameter("type", type) :
                new ObjectParameter("type", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_EmptyReferencesDetails_Result>("sp_EmptyReferencesDetails", strLengthParameter, typeParameter);
        }
    }
}
