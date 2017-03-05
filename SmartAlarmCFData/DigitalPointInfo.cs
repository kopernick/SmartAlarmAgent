namespace SmartAlarmCFData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DigitalPointInfo")]
    public partial class DigitalPointInfo
    {
        public int? FkStationID { get; set; }

        public int? PointNumber { get; set; }

        [StringLength(15)]
        public string StationName { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PkDigitalID { get; set; }

        public int? FkRtuID { get; set; }

        public int? FkRtuDigitalInID { get; set; }

        [StringLength(40)]
        public string PointName { get; set; }

        [StringLength(14)]
        public string ShortName { get; set; }

        public byte? SourceType { get; set; }

        public byte? ShowType { get; set; }

        public short? RawValue { get; set; }

        public byte? ActualValue { get; set; }

        public double? OffsetValue { get; set; }

        public byte? NormalState { get; set; }

        public byte? ChangeDetect { get; set; }

        public byte? ReplaceValue { get; set; }

        public byte? AudioInhibit { get; set; }

        public byte? AlarmInhibit { get; set; }

        public byte? RTNAlarmInhibit { get; set; }

        public byte? AlarmPriority { get; set; }

        public byte? LogInhibit { get; set; }

        public byte? RTNLogInhibit { get; set; }

        public byte? ControlType { get; set; }

        public byte? ControlInhibit { get; set; }

        public byte? ControlInhibitType { get; set; }

        public byte? OutOfService { get; set; }

        public byte? OperationTag { get; set; }

        public int? FkRtuDigitalOutID { get; set; }

        public int? FkCalcID { get; set; }

        public int? FkEcaSwID { get; set; }

        public int? CaptionCode { get; set; }

        public int? FkAlarmGroupID { get; set; }

        public int? FkEquipmentID { get; set; }

        public int? FkBayID { get; set; }

        public byte? Flashing { get; set; }

        public byte? TelemeterFail { get; set; }

        public byte? UnderControl { get; set; }

        public byte? DataLinkFlag { get; set; }

        public DateTime? DateTime { get; set; }

        public int? DeviceID { get; set; }

        [StringLength(100)]
        public string DeviceType { get; set; }

        [StringLength(100)]
        public string MACName { get; set; }

        [StringLength(32)]
        public string Priority { get; set; }

        public virtual Device Device { get; set; }
    }
}
