namespace SmartAlarmCFData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Priority")]
    public partial class Priority
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PriorityID { get; set; }

        [StringLength(32)]
        public string PriorityName { get; set; }
    }
}
