namespace SmartAlarmCFData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RestorationAlarmList")]
    public partial class RestorationAlarmList
    {
        [Key]
        public int PkAlarmListID { get; set; }

        public DateTime? DateTime { get; set; }

        public byte? PointType { get; set; }

        public int? FkIndexID { get; set; }

        [StringLength(15)]
        public string StationName { get; set; }

        [StringLength(40)]
        public string PointName { get; set; }

        public int? AlarmType { get; set; }

        public byte? Flashing { get; set; }

        public double? ActualValue { get; set; }

        [StringLength(40)]
        public string Message { get; set; }

        [StringLength(20)]
        public string SourceName { get; set; }

        public int? SourceID { get; set; }

        public byte? SourceType { get; set; }

        public byte? AlarmFlag { get; set; }

        [StringLength(100)]
        public string DeviceType { get; set; }

        [StringLength(255)]
        public string MACName { get; set; }

        [StringLength(32)]
        public string Priority { get; set; }
    }
}
