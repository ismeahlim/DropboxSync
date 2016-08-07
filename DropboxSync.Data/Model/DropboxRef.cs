using System;
using System.ComponentModel.DataAnnotations;

namespace DropboxSync.Data.Model
{
    public class DropboxRef
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        public string DropboxId { get; set; }

        [StringLength(50)]
        public string ExactId { get; set; }

        public DateTime DropboxModifiedOn { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
