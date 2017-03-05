namespace SmartAlarmCFData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Station")]
    public partial class Station
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PkStationID { get; set; }

        [Required]
        [StringLength(15)]
        public string StationName { get; set; }

        public int StationNumber { get; set; }

        [StringLength(80)]
        public string Detail { get; set; }

        [StringLength(15)]
        public string DCSName { get; set; }

        public byte? DCSNumber { get; set; }

        [StringLength(15)]
        public string RegionName { get; set; }

        public byte? RegionNumber { get; set; }
    }
}
