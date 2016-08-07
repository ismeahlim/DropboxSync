using System;
using System.ComponentModel.DataAnnotations;

namespace DropboxSync.Data.Model
{
    public class Settings
    {
        [Key]
        [StringLength(50)]
        public string Key { get; set; }

        [StringLength(1000)]
        public string value { get; set; }
    }
}
